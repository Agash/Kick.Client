using System.Net.Http.Json;
using System.Text.Json;

namespace Kick.Client.Authentication;

/// <summary>
/// Handles OAuth 2.1 token exchanges against <c>https://id.kick.com</c>.
/// </summary>
public sealed class KickOAuthClient : IDisposable
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _http;
    private readonly KickOAuthOptions _options;
    private readonly string _oAuthBaseUrl;
    private KickTokenResponse? _currentToken;

    /// <summary>
    /// Initializes a new <see cref="KickOAuthClient"/>.
    /// </summary>
    /// <param name="http">The <see cref="HttpClient"/> to use for token requests.</param>
    /// <param name="options">OAuth configuration (client ID, redirect URI, scopes).</param>
    /// <param name="oAuthBaseUrl">Base URL of the Kick identity server. Defaults to <c>https://id.kick.com</c>.</param>
    public KickOAuthClient(HttpClient http, KickOAuthOptions options, string oAuthBaseUrl = "https://id.kick.com")
    {
        ArgumentNullException.ThrowIfNull(http);
        ArgumentNullException.ThrowIfNull(options);
        _http = http;
        _options = options;
        _oAuthBaseUrl = oAuthBaseUrl.TrimEnd('/');
    }

    /// <summary>Returns the current access token, refreshing automatically if close to expiry.</summary>
    public async ValueTask<string> GetAccessTokenAsync(CancellationToken ct = default)
    {
        if (_currentToken is not null && _currentToken.ExpiresAtUtc > DateTimeOffset.UtcNow.AddMinutes(2))
        {
            return _currentToken.AccessToken;
        }

        if (_currentToken?.RefreshToken is string refreshToken)
        {
            _currentToken = await RefreshTokenAsync(refreshToken, ct).ConfigureAwait(false);
            return _currentToken.AccessToken;
        }

        throw new InvalidOperationException("No valid token available. Call ExchangeCodeAsync first.");
    }

    /// <summary>
    /// Seeds the client with a previously-obtained token (e.g., restored from persisted storage).
    /// After calling this method, <see cref="GetAccessTokenAsync"/> will return or auto-refresh the token.
    /// </summary>
    public void SetToken(KickTokenResponse token)
    {
        ArgumentNullException.ThrowIfNull(token);
        _currentToken = token;
    }

    /// <summary>Exchanges an authorization code (PKCE) for an access + refresh token pair.</summary>
    public async Task<KickTokenResponse> ExchangeCodeAsync(
        string code, string codeVerifier, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentException.ThrowIfNullOrWhiteSpace(codeVerifier);

        Dictionary<string, string> form = new()
        {
            ["grant_type"] = "authorization_code",
            ["code"] = code,
            ["client_id"] = _options.ClientId,
            ["redirect_uri"] = _options.RedirectUri,
            ["code_verifier"] = codeVerifier,
        };
        if (_options.ClientSecret is not null)
        {
            form["client_secret"] = _options.ClientSecret;
        }

        _currentToken = await PostTokenFormAsync(form, ct).ConfigureAwait(false);
        return _currentToken;
    }

    /// <summary>Obtains an app access token via client_credentials grant.</summary>
    public async Task<KickTokenResponse> GetAppTokenAsync(CancellationToken ct = default)
    {
        if (_options.ClientSecret is null)
        {
            throw new InvalidOperationException("ClientSecret is required for client_credentials grant.");
        }

        Dictionary<string, string> form = new()
        {
            ["grant_type"] = "client_credentials",
            ["client_id"] = _options.ClientId,
            ["client_secret"] = _options.ClientSecret,
        };

        _currentToken = await PostTokenFormAsync(form, ct).ConfigureAwait(false);
        return _currentToken;
    }

    private async Task<KickTokenResponse> RefreshTokenAsync(string refreshToken, CancellationToken ct)
    {
        Dictionary<string, string> form = new()
        {
            ["grant_type"] = "refresh_token",
            ["refresh_token"] = refreshToken,
            ["client_id"] = _options.ClientId,
        };
        if (_options.ClientSecret is not null)
        {
            form["client_secret"] = _options.ClientSecret;
        }

        return await PostTokenFormAsync(form, ct).ConfigureAwait(false);
    }

    private async Task<KickTokenResponse> PostTokenFormAsync(
        Dictionary<string, string> form, CancellationToken ct)
    {
        using FormUrlEncodedContent content = new(form);
        using HttpResponseMessage response = await _http
            .PostAsync($"{_oAuthBaseUrl}/oauth/token", content, ct)
            .ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        KickTokenResponse token = (await response.Content
            .ReadFromJsonAsync<KickTokenResponse>(_jsonOptions, ct)
            .ConfigureAwait(false))!;
        token.ExpiresAtUtc = DateTimeOffset.UtcNow.AddSeconds(token.ExpiresInSeconds);
        return token;
    }

    /// <inheritdoc/>
    public void Dispose() => _http.Dispose();
}
