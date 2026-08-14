using System.Text.Json;
using System.Text.Json.Serialization;
using Kick.Client.Authentication;
using Kick.Client.Webhooks;

namespace Kick.Client.Serialization;

/// <summary>
/// Source-generated JSON contracts for every type Kick.Client serializes or deserializes.
/// </summary>
/// <remarks>
/// Every serialization path in this library resolves its contract from here rather than from a bare
/// <see cref="JsonSerializerOptions"/>. The reflection-based overloads build contracts at run time, which
/// the trimmer cannot see and Native AOT cannot compile, so using them would make the package unusable in
/// a trimmed or AOT-published host. Any new wire type has to be added here as well.
/// </remarks>
[JsonSourceGenerationOptions(JsonSerializerDefaults.Web)]
[JsonSerializable(typeof(KickTokenResponse))]
[JsonSerializable(typeof(KickSubscriptionRequest))]
[JsonSerializable(typeof(KickChatMessagePayload))]
[JsonSerializable(typeof(KickChannelFollowedPayload))]
[JsonSerializable(typeof(KickSubscriptionPayload))]
[JsonSerializable(typeof(KickSubscriptionGiftsPayload))]
[JsonSerializable(typeof(KickRewardRedemptionPayload))]
[JsonSerializable(typeof(KickLivestreamStatusPayload))]
[JsonSerializable(typeof(KickLivestreamMetadataPayload))]
[JsonSerializable(typeof(KickModerationBannedPayload))]
[JsonSerializable(typeof(KickKicksGiftedPayload))]
internal sealed partial class KickJsonContext : JsonSerializerContext;
