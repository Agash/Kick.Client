using System.Security.Cryptography;
using System.Text;

namespace Kick.Client.Authentication;

/// <summary>Helpers for generating RFC 7636 PKCE code_verifier / code_challenge pairs.</summary>
public static class KickPkceFlowHelper
{
    private const int VerifierByteLength = 32;

    /// <summary>Generates a cryptographically random code_verifier string.</summary>
    public static string GenerateCodeVerifier()
    {
        byte[] bytes = RandomNumberGenerator.GetBytes(VerifierByteLength);
        return Base64UrlEncode(bytes);
    }

    /// <summary>Derives the S256 code_challenge from a <paramref name="codeVerifier"/>.</summary>
    public static string DeriveCodeChallenge(string codeVerifier)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(codeVerifier);
        byte[] hash = SHA256.HashData(Encoding.ASCII.GetBytes(codeVerifier));
        return Base64UrlEncode(hash);
    }

    /// <summary>Builds the Kick authorization URL for the Authorization Code + PKCE flow.</summary>
    public static string BuildAuthorizationUrl(
        string oAuthBaseUrl,
        KickOAuthOptions options,
        string codeChallenge,
        string state)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(oAuthBaseUrl);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentException.ThrowIfNullOrWhiteSpace(codeChallenge);
        ArgumentException.ThrowIfNullOrWhiteSpace(state);

        string baseUrl = oAuthBaseUrl.TrimEnd('/');
        string encodedScopes = Uri.EscapeDataString(options.Scopes);
        string encodedRedirect = Uri.EscapeDataString(options.RedirectUri);

        return $"{baseUrl}/oauth/authorize" +
               $"?client_id={options.ClientId}" +
               $"&response_type=code" +
               $"&redirect_uri={encodedRedirect}" +
               $"&state={state}" +
               $"&scope={encodedScopes}" +
               $"&code_challenge={codeChallenge}" +
               $"&code_challenge_method=S256";
    }

    private static string Base64UrlEncode(byte[] bytes) =>
        Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
}
