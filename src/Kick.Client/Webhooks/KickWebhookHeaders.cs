namespace Kick.Client.Webhooks;

/// <summary>Kick webhook request header name constants.</summary>
public static class KickWebhookHeaders
{
    /// <summary>Header carrying the unique message ID for deduplication (<c>Kick-Event-Message-Id</c>).</summary>
    public const string MessageId = "Kick-Event-Message-Id";

    /// <summary>Header carrying the subscription ID that triggered the delivery (<c>Kick-Event-Subscription-Id</c>).</summary>
    public const string SubscriptionId = "Kick-Event-Subscription-Id";

    /// <summary>Header carrying the RSA-PKCS1v15 SHA-256 signature for verification (<c>Kick-Event-Signature</c>).</summary>
    public const string Signature = "Kick-Event-Signature";

    /// <summary>Header carrying the ISO-8601 UTC timestamp when the message was sent (<c>Kick-Event-Message-Timestamp</c>).</summary>
    public const string MessageTimestamp = "Kick-Event-Message-Timestamp";

    /// <summary>Header carrying the event type string (see <see cref="KickEventTypes"/>) (<c>Kick-Event-Type</c>).</summary>
    public const string EventType = "Kick-Event-Type";

    /// <summary>Header carrying the event schema version (<c>Kick-Event-Version</c>).</summary>
    public const string EventVersion = "Kick-Event-Version";
}
