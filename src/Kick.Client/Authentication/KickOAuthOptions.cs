namespace Kick.Client.Authentication;

/// <summary>OAuth 2.1 / PKCE configuration for Kick.</summary>
public sealed class KickOAuthOptions
{
    /// <summary>OAuth client ID registered at <c>https://kick.com/settings/developers</c>.</summary>
    public required string ClientId { get; init; }

    /// <summary>OAuth client secret. May be <see langword="null"/> for public clients using PKCE only.</summary>
    public string? ClientSecret { get; init; }

    /// <summary>
    /// Redirect URI registered with the application.
    /// When using 127.0.0.1 (not localhost) prepend a dummy <c>redirect</c> query parameter
    /// per the Kick API workaround for the NextJS IP→localhost rewrite bug.
    /// </summary>
    public required string RedirectUri { get; init; }

    /// <summary>
    /// Space-separated OAuth scopes to request. Defaults to <see cref="KickScopes.WebhookProvider"/>.
    /// Use <see cref="KickScopes.Join"/> or a preset such as <see cref="KickScopes.ChatBot"/>
    /// to construct this value.
    /// </summary>
    public string Scopes { get; init; } = KickScopes.WebhookProvider;

    /// <summary>
    /// Creates a <see cref="KickOAuthOptions"/> pre-configured with <see cref="KickScopes.ChatBot"/> scopes.
    /// </summary>
    /// <param name="clientId">OAuth client ID registered at <c>https://kick.com/settings/developers</c>.</param>
    /// <param name="redirectUri">Redirect URI registered with the application.</param>
    /// <returns>A new <see cref="KickOAuthOptions"/> instance for chat bot use.</returns>
    public static KickOAuthOptions ForChatBot(string clientId, string redirectUri) =>
        new() { ClientId = clientId, RedirectUri = redirectUri, Scopes = KickScopes.ChatBot };

    /// <summary>
    /// Creates a <see cref="KickOAuthOptions"/> pre-configured with <see cref="KickScopes.WebhookProvider"/> scopes.
    /// </summary>
    /// <param name="clientId">OAuth client ID registered at <c>https://kick.com/settings/developers</c>.</param>
    /// <param name="redirectUri">Redirect URI registered with the application.</param>
    /// <returns>A new <see cref="KickOAuthOptions"/> instance for webhook-based provider use.</returns>
    public static KickOAuthOptions ForWebhookProvider(string clientId, string redirectUri) =>
        new() { ClientId = clientId, RedirectUri = redirectUri, Scopes = KickScopes.WebhookProvider };
}
