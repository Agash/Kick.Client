using DevTunnels.Client;
using Kick.Client.AspNetCore;
using Kick.Client.Authentication;
using Kick.Client.DependencyInjection;
using Kick.Client.Webhooks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Spectre.Console;
using System.Collections.Concurrent;
using System.Diagnostics;

CancellationTokenSource shutdown = new();
Console.CancelKeyPress += (_, e) => { e.Cancel = true; shutdown.Cancel(); };

try
{
    await SampleApplication.RunAsync(shutdown.Token).ConfigureAwait(false);
}
catch (OperationCanceledException)
{
    AnsiConsole.MarkupLine("[grey]Shutting down...[/]");
}
catch (Exception ex)
{
    AnsiConsole.WriteException(ex);
    Environment.ExitCode = 1;
}

internal static class SampleApplication
{
    public static async Task RunAsync(CancellationToken ct)
    {
        AnsiConsole.Clear();
        AnsiConsole.Write(new FigletText("Kick.Client Sample").Color(Color.Green1));
        AnsiConsole.MarkupLine("[grey]Kick Platform API · OAuth 2.1 PKCE · RSA webhook demo[/]");
        AnsiConsole.WriteLine();

        int localPort = AnsiConsole.Ask("Local port", 5200);
        string webhookPath = AnsiConsole.Ask("Webhook path", "/kick/webhook");
        string clientId = AnsiConsole.Prompt(new TextPrompt<string>("Client ID:").Secret(' '));
        string clientSecret = AnsiConsole.Prompt(
            new TextPrompt<string>("Client Secret (blank for public client):").AllowEmpty().Secret(' '));
        string broadcasterId = AnsiConsole.Ask<string>("Broadcaster user ID:");

        ConcurrentQueue<(DateTimeOffset At, string Type, string Summary)> events = new();

        // ── Build ASP.NET Core host ────────────────────────────────────────
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls($"http://127.0.0.1:{localPort}");
        builder.Services.AddKickClient(
            options: new() { ApiBaseUrl = "https://api.kick.com" },
            webhookOptions: new KickWebhookOptions());

        WebApplication app = builder.Build();
        app.MapGet("/", () => "Kick.Client.Sample is running.");
        app.MapKickWebhook(
            webhookPath,
            static (_, _) => Task.FromResult(new KickWebhookOptions()),
            (evt, _, _) =>
            {
                events.Enqueue((DateTimeOffset.UtcNow, evt.EventType, Summarize(evt)));
                return Task.CompletedTask;
            });

        await app.StartAsync(ct).ConfigureAwait(false);
        AnsiConsole.MarkupLine($"[green]Server listening on port {localPort}[/]");

        // ── DevTunnels setup ───────────────────────────────────────────────
        AnsiConsole.MarkupLine("[cyan]Setting up DevTunnel...[/]");
        DevTunnelsClient tunnelClient = new(new DevTunnelsClientOptions
        {
            CommandTimeout = TimeSpan.FromSeconds(30)
        });
        DevTunnelCliProbeResult probe = await tunnelClient.ProbeCliAsync(ct).ConfigureAwait(false);
        if (!probe.IsInstalled)
        {
            AnsiConsole.MarkupLine("[red]devtunnel CLI not found. Install from https://aka.ms/TunnelsCliDownload[/]");
            return;
        }

        await tunnelClient.EnsureLoggedInAsync(LoginProvider.GitHub, ct).ConfigureAwait(false);
        const string tunnelId = "kick-client-sample";
        await tunnelClient.CreateOrUpdateTunnelAsync(tunnelId,
            new DevTunnelOptions { Description = "Kick.Client.Sample", AllowAnonymous = true }, ct)
            .ConfigureAwait(false);
        await tunnelClient.CreateOrReplacePortAsync(tunnelId, localPort,
            new DevTunnelPortOptions { Protocol = "https" }, ct)
            .ConfigureAwait(false);

        IDevTunnelHostSession session = await tunnelClient.StartHostSessionAsync(
            new DevTunnelHostStartOptions { TunnelId = tunnelId }, ct).ConfigureAwait(false);
        await session.WaitForReadyAsync(ct).ConfigureAwait(false);

        string publicUrl = session.PublicUrl?.ToString() ?? string.Empty;
        string webhookUrl = $"{publicUrl.TrimEnd('/')}{webhookPath}";
        AnsiConsole.MarkupLine($"[green]Tunnel ready:[/] {publicUrl}");
        AnsiConsole.MarkupLine($"[bold]Webhook URL:[/] {webhookUrl}");

        // ── OAuth 2.1 PKCE ─────────────────────────────────────────────────
        KickOAuthOptions oauthOptions = new()
        {
            ClientId = clientId,
            ClientSecret = string.IsNullOrWhiteSpace(clientSecret) ? null : clientSecret,
            RedirectUri = $"http://127.0.0.1:{localPort}/oauth/callback",
            Scopes = "user:read channel:read events:subscribe",
        };

        string codeVerifier = KickPkceFlowHelper.GenerateCodeVerifier();
        string codeChallenge = KickPkceFlowHelper.DeriveCodeChallenge(codeVerifier);
        string state = Guid.NewGuid().ToString("N");
        string authUrl = KickPkceFlowHelper.BuildAuthorizationUrl(
            "https://id.kick.com", oauthOptions, codeChallenge, state);

        AnsiConsole.MarkupLine($"[blue]Opening Kick authorization page...[/]");
        AnsiConsole.MarkupLine(authUrl);
        try { Process.Start(new ProcessStartInfo(authUrl) { UseShellExecute = true }); }
        catch { /* shell-open may fail in headless environments */ }

        string authCode = AnsiConsole.Ask<string>("Paste the authorization code:");

        using KickOAuthClient oauthClient = new(new HttpClient(), oauthOptions);
        _ = await oauthClient.ExchangeCodeAsync(authCode, codeVerifier, ct).ConfigureAwait(false);
        AnsiConsole.MarkupLine("[green]Authenticated![/]");

        // ── Subscribe to all webhook event types ───────────────────────────
        AnsiConsole.MarkupLine("[cyan]Subscribing to Kick webhook events...[/]");
        using HttpClient apiClient = new() { BaseAddress = new Uri("https://api.kick.com") };
        apiClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue(
                "Bearer", await oauthClient.GetAccessTokenAsync(ct).ConfigureAwait(false));

        KickSubscriptionClient subClient = new(apiClient);
        IReadOnlyList<KickSubscriptionResult> subResults =
            await subClient.SubscribeAllAsync(broadcasterId, webhookUrl, ct).ConfigureAwait(false);

        foreach (KickSubscriptionResult r in subResults)
        {
            string status = r.IsSuccess ? "[green]OK[/]" : $"[red]FAIL ({r.StatusCode})[/]";
            AnsiConsole.MarkupLine($"  {r.EventType}: {status}");
        }

        AnsiConsole.MarkupLine("[bold green]Ready — waiting for events. Ctrl+C to stop.[/]");

        // ── Event display loop ─────────────────────────────────────────────
        try
        {
            while (!ct.IsCancellationRequested)
            {
                await Task.Delay(500, ct).ConfigureAwait(false);
                while (events.TryDequeue(out var e))
                {
                    AnsiConsole.MarkupLine(
                        $"[grey]{e.At:HH:mm:ss}[/] [cyan]{e.Type}[/] {Markup.Escape(e.Summary)}");
                }
            }
        }
        catch (OperationCanceledException) { }

        // ── Cleanup ────────────────────────────────────────────────────────
        AnsiConsole.MarkupLine("[yellow]Unsubscribing...[/]");
        await subClient.UnsubscribeAllAsync(broadcasterId, CancellationToken.None).ConfigureAwait(false);
        await session.StopAsync(CancellationToken.None).ConfigureAwait(false);
        await app.StopAsync(CancellationToken.None).ConfigureAwait(false);
    }

    private static string Summarize(KickWebhookEvent evt) => evt.Payload switch
    {
        KickChatMessagePayload m => $"{m.Sender.Username}: {m.Content}",
        KickChannelFollowedPayload f => $"{f.Follower.Username} followed",
        KickSubscriptionPayload s => $"{s.Subscriber.Username} subscribed ({s.Duration}mo)",
        KickSubscriptionGiftsPayload g => $"{g.Gifter.Username} gifted {g.Giftees?.Count ?? 0} subs",
        KickRewardRedemptionPayload r => $"{r.Redeemer.Username} redeemed '{r.Reward.Title}' [{r.Status}]",
        KickLivestreamStatusPayload ls => ls.IsLive ? $"Stream started: {ls.Title}" : "Stream ended",
        KickLivestreamMetadataPayload lm => $"Metadata: {lm.Metadata.Title}",
        KickModerationBannedPayload b => $"{b.BannedUser.Username} banned",
        KickKicksGiftedPayload k => $"{k.Sender.Username} gifted {k.Gift.Amount} kicks",
        _ => "(unknown)",
    };
}
