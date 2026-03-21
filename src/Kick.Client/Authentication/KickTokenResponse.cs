using System.Text.Json.Serialization;

namespace Kick.Client.Authentication;

/// <summary>Token endpoint response from <c>POST https://id.kick.com/oauth/token</c>.</summary>
public sealed class KickTokenResponse
{
    /// <summary>The OAuth 2.1 access token to be used as a Bearer credential.</summary>
    [JsonPropertyName("access_token")]
    public string AccessToken { get; init; } = string.Empty;

    /// <summary>Refresh token that can be exchanged for a new access token. May be <see langword="null"/> for non-refresh grants.</summary>
    [JsonPropertyName("refresh_token")]
    public string? RefreshToken { get; init; }

    /// <summary>Token type returned by the server (always <c>Bearer</c> for Kick).</summary>
    [JsonPropertyName("token_type")]
    public string TokenType { get; init; } = "Bearer";

    /// <summary>Lifetime of the access token in seconds.</summary>
    [JsonPropertyName("expires_in")]
    public int ExpiresInSeconds { get; init; }

    /// <summary>Space-separated list of scopes granted by the token, if returned by the server.</summary>
    [JsonPropertyName("scope")]
    public string? Scope { get; init; }

    /// <summary>Computed expiry, populated by <see cref="KickOAuthClient"/> after token exchange.</summary>
    [JsonIgnore]
    public DateTimeOffset ExpiresAtUtc { get; set; }
}
