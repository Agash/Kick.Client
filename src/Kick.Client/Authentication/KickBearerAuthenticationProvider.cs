using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Authentication;

namespace Kick.Client.Authentication;

/// <summary>
/// Kiota <see cref="IAuthenticationProvider"/> that attaches a Bearer token to every request.
/// </summary>
public sealed class KickBearerAuthenticationProvider : IAuthenticationProvider
{
    private readonly Func<CancellationToken, ValueTask<string>> _tokenFactory;

    /// <param name="tokenFactory">
    /// Returns the current access token. Called before each request so token refresh is transparent.
    /// </param>
    public KickBearerAuthenticationProvider(Func<CancellationToken, ValueTask<string>> tokenFactory)
    {
        ArgumentNullException.ThrowIfNull(tokenFactory);
        _tokenFactory = tokenFactory;
    }

    /// <inheritdoc/>
    public async Task AuthenticateRequestAsync(
        RequestInformation request,
        Dictionary<string, object>? additionalAuthenticationContext = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        string token = await _tokenFactory(cancellationToken).ConfigureAwait(false);
        request.Headers.Add("Authorization", $"Bearer {token}");
    }
}
