namespace Kick.Client;

/// <summary>Configuration options for the Kick REST API client.</summary>
public sealed class KickClientOptions
{
    /// <summary>Base URL for the Kick REST API. Defaults to <c>https://api.kick.com</c>.</summary>
    public string ApiBaseUrl { get; set; } = "https://api.kick.com";

    /// <summary>Base URL for the Kick OAuth / identity server. Defaults to <c>https://id.kick.com</c>.</summary>
    public string OAuthBaseUrl { get; set; } = "https://id.kick.com";
}
