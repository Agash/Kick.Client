namespace Kick.Client.Webhooks;

/// <summary>Options controlling Kick webhook validation behaviour.</summary>
public sealed class KickWebhookOptions
{
    /// <summary>
    /// Hard-coded Kick RSA public key (PEM). Defaults to the key published in the Kick docs.
    /// Set to <see langword="null"/> to disable signature verification (not recommended in production).
    /// </summary>
    public string? PublicKeyPem { get; set; } = KickWebhookDefaults.PublicKeyPem;

    /// <summary>
    /// When <see langword="true"/> (default), requests with an invalid or missing RSA signature
    /// are rejected with HTTP 401.
    /// </summary>
    public bool RequireValidSignature { get; set; } = true;
}

/// <summary>Kick's published RSA-2048 public key (as of 2026-03).</summary>
public static class KickWebhookDefaults
{
    /// <summary>
    /// The Kick RSA-2048 public key used to verify the <c>Kick-Event-Signature</c> header.
    /// Also available at runtime from <c>GET https://api.kick.com/public/v1/public-key</c>.
    /// </summary>
    public const string PublicKeyPem =
        "-----BEGIN PUBLIC KEY-----\n" +
        "MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAq/+l1WnlRrGSolDMA+A8\n" +
        "6rAhMbQGmQ2SapVcGM3zq8ANXjnhDWocMqfWcTd95btDydITa10kDvHzw9WQOqp2\n" +
        "MZI7ZyrfzJuz5nhTPCiJwTwnEtWft7nV14BYRDHvlfqPUaZ+1KR4OCaO/wWIk/rQ\n" +
        "L/TjY0M70gse8rlBkbo2a8rKhu69RQTRsoaf4DVhDPEeSeI5jVrRDGAMGL3cGuyY\n" +
        "6CLKGdjVEM78g3JfYOvDU/RvfqD7L89TZ3iN94jrmWdGz34JNlEI5hqK8dd7C5EF\n" +
        "BEbZ5jgB8s8ReQV8H+MkuffjdAj3ajDDX3DOJMIut1lBrUVD1AaSrGCKHooWoL2e\n" +
        "twIDAQAB\n" +
        "-----END PUBLIC KEY-----";
}
