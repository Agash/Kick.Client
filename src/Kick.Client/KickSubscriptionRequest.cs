using System.Text.Json.Serialization;

namespace Kick.Client;

/// <summary>
/// Request body for <c>POST /public/v1/events/subscriptions</c>.
/// </summary>
/// <remarks>
/// A named type rather than an anonymous one so the source generator can emit a contract for it:
/// anonymous types cannot be annotated with <see cref="JsonSerializableAttribute"/>, which would force
/// this call back onto the reflection-based serializer and break trimming and Native AOT.
/// </remarks>
internal sealed record KickSubscriptionRequest(
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("version")] string Version,
    [property: JsonPropertyName("broadcaster_user_id")] string BroadcasterUserId,
    [property: JsonPropertyName("webhook_url")] string WebhookUrl);
