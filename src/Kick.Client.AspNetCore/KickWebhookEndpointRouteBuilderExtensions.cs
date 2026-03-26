using Agash.Webhook.Abstractions;
using Kick.Client.Webhooks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Kick.Client.AspNetCore;

/// <summary>
/// ASP.NET Core minimal-API extensions for mapping a Kick webhook receiver endpoint.
/// </summary>
public static class KickWebhookEndpointRouteBuilderExtensions
{
    /// <summary>
    /// Maps a POST endpoint at <paramref name="pattern"/> that validates the Kick RSA signature
    /// and invokes <paramref name="onEvent"/> with the deserialized <see cref="KickWebhookEvent"/>.
    /// </summary>
    public static IEndpointConventionBuilder MapKickWebhook(
        this IEndpointRouteBuilder endpoints,
        string pattern,
        Func<HttpContext, CancellationToken, Task<KickWebhookOptions>> optionsFactory,
        Func<KickWebhookEvent, HttpContext, CancellationToken, Task> onEvent)
    {
        ArgumentNullException.ThrowIfNull(endpoints);
        ArgumentNullException.ThrowIfNull(optionsFactory);
        ArgumentNullException.ThrowIfNull(onEvent);

        return endpoints.MapPost(pattern, async (HttpContext ctx) =>
        {
            CancellationToken ct = ctx.RequestAborted;
            KickWebhookOptions options = await optionsFactory(ctx, ct).ConfigureAwait(false);

            ctx.Request.EnableBuffering();
            using MemoryStream ms = new();
            await ctx.Request.Body.CopyToAsync(ms, ct).ConfigureAwait(false);
            byte[] body = ms.ToArray();

            WebhookRequest webhookRequest = new()
            {
                Method = ctx.Request.Method,
                Path = ctx.Request.Path.Value ?? "/",
                Headers = ctx.Request.Headers.ToDictionary(
                    h => h.Key,
                    h => h.Value.Select(static value => value ?? string.Empty).ToArray(),
                    StringComparer.OrdinalIgnoreCase),
                Body = body,
            };

            KickWebhookHandler handler = ctx.RequestServices.GetRequiredService<KickWebhookHandler>();
            WebhookHandleResult<KickWebhookEvent> result =
                await handler.HandleAsync(webhookRequest, options, ct).ConfigureAwait(false);

            if (!result.IsAuthenticated)
            {
                return Results.StatusCode(result.Response.StatusCode);
            }

            if (!result.IsKnownEvent || result.Event is null)
            {
                return Results.Ok();
            }

            await onEvent(result.Event, ctx, ct).ConfigureAwait(false);
            return Results.Ok();
        });
    }
}
