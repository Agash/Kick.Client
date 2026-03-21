using Kick.Client;
using Kick.Client.Authentication;
using Kick.Client.Webhooks;
using Microsoft.Extensions.DependencyInjection;

namespace Kick.Client.DependencyInjection;

/// <summary><see cref="IServiceCollection"/> extensions for registering Kick client services.</summary>
public static class KickClientServiceExtensions
{
    /// <summary>
    /// Registers <see cref="KickWebhookHandler"/>, <see cref="KickSubscriptionClient"/>,
    /// and <see cref="KickOAuthClient"/> with the DI container.
    /// </summary>
    public static IServiceCollection AddKickClient(
        this IServiceCollection services,
        KickClientOptions? options = null,
        KickWebhookOptions? webhookOptions = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        options ??= new KickClientOptions();
        webhookOptions ??= new KickWebhookOptions();

        _ = services.AddSingleton(options);
        _ = services.AddSingleton(webhookOptions);
        _ = services.AddSingleton<KickWebhookHandler>();

        _ = services.AddHttpClient<KickSubscriptionClient>(client =>
        {
            client.BaseAddress = new Uri(options.ApiBaseUrl);
        });

        _ = services.AddHttpClient<KickOAuthClient>(client =>
        {
            client.BaseAddress = new Uri(options.OAuthBaseUrl);
        });

        return services;
    }
}
