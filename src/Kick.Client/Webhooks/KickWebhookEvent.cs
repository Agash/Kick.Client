using System.Text.Json.Serialization;

namespace Kick.Client.Webhooks;

// ─── Shared sub-types ────────────────────────────────────────────────────────

/// <summary>A Kick user/broadcaster object as it appears in webhook payloads.</summary>
public sealed class KickWebhookUser
{
    /// <summary>Whether the user sent the action anonymously.</summary>
    [JsonPropertyName("is_anonymous")] public bool? IsAnonymous { get; init; }

    /// <summary>The user's numeric Kick user ID.</summary>
    [JsonPropertyName("user_id")] public long? UserId { get; init; }

    /// <summary>The user's Kick username.</summary>
    [JsonPropertyName("username")] public string? Username { get; init; }

    /// <summary>Whether the user's account has been verified by Kick.</summary>
    [JsonPropertyName("is_verified")] public bool? IsVerified { get; init; }

    /// <summary>URL of the user's profile picture.</summary>
    [JsonPropertyName("profile_picture")] public string? ProfilePicture { get; init; }

    /// <summary>The URL slug for the user's channel (e.g. <c>my_channel</c>).</summary>
    [JsonPropertyName("channel_slug")] public string? ChannelSlug { get; init; }

    /// <summary>Chat identity information including colour and badges.</summary>
    [JsonPropertyName("identity")] public KickUserIdentity? Identity { get; init; }
}

/// <summary>Chat identity information for a user (username colour and chat badges).</summary>
public sealed class KickUserIdentity
{
    /// <summary>The hex colour code for the user's username in chat, if set.</summary>
    [JsonPropertyName("username_color")] public string? UsernameColor { get; init; }

    /// <summary>The list of chat badges currently displayed for the user.</summary>
    [JsonPropertyName("badges")] public IReadOnlyList<KickBadge>? Badges { get; init; }
}

/// <summary>A single chat badge displayed beside a user's name.</summary>
public sealed class KickBadge
{
    /// <summary>Human-readable label for the badge (e.g. <c>Moderator</c>).</summary>
    [JsonPropertyName("text")] public string Text { get; init; } = string.Empty;

    /// <summary>Machine-readable badge type identifier (e.g. <c>moderator</c>).</summary>
    [JsonPropertyName("type")] public string Type { get; init; } = string.Empty;

    /// <summary>Optional numeric count associated with the badge (e.g. months subscribed).</summary>
    [JsonPropertyName("count")] public int? Count { get; init; }
}

// ─── Webhook envelope ────────────────────────────────────────────────────────

/// <summary>
/// Fully-resolved Kick webhook event with all headers and a typed payload.
/// Produced by <see cref="KickWebhookHandler"/> after successful RSA signature verification.
/// </summary>
public sealed class KickWebhookEvent
{
    /// <summary>Unique message ID provided in the <c>Kick-Event-Message-Id</c> header.</summary>
    public required string MessageId { get; init; }

    /// <summary>Subscription ID provided in the <c>Kick-Event-Subscription-Id</c> header.</summary>
    public required string SubscriptionId { get; init; }

    /// <summary>Event type string from the <c>Kick-Event-Type</c> header (see <see cref="KickEventTypes"/>).</summary>
    public required string EventType { get; init; }

    /// <summary>Event schema version from the <c>Kick-Event-Version</c> header.</summary>
    public required string EventVersion { get; init; }

    /// <summary>UTC timestamp when Kick sent the message, parsed from the <c>Kick-Event-Message-Timestamp</c> header.</summary>
    public required DateTimeOffset MessageTimestamp { get; init; }

    /// <summary>Raw JSON body bytes — kept for downstream re-parsing if needed.</summary>
    public required ReadOnlyMemory<byte> RawBody { get; init; }

    /// <summary>
    /// Deserialized payload — one of the <c>Kick*Payload</c> types corresponding to
    /// <see cref="EventType"/>. May be <see langword="null"/> if the event type is unknown.
    /// </summary>
    public required object Payload { get; init; }
}

// ─── Event-type string constants ─────────────────────────────────────────────

/// <summary>String constants for every Kick webhook event type.</summary>
public static class KickEventTypes
{
    /// <summary>Fired when a chat message is sent in a channel (<c>chat.message.sent</c>).</summary>
    public const string ChatMessageSent = "chat.message.sent";

    /// <summary>Fired when a user follows a channel (<c>channel.followed</c>).</summary>
    public const string ChannelFollowed = "channel.followed";

    /// <summary>Fired when a user renews an existing subscription (<c>channel.subscription.renewal</c>).</summary>
    public const string SubscriptionRenewal = "channel.subscription.renewal";

    /// <summary>Fired when subscription gifts are sent to the channel (<c>channel.subscription.gifts</c>).</summary>
    public const string SubscriptionGifts = "channel.subscription.gifts";

    /// <summary>Fired when a user starts a new subscription (<c>channel.subscription.new</c>).</summary>
    public const string SubscriptionNew = "channel.subscription.new";

    /// <summary>Fired when a channel-points reward redemption is updated (<c>channel.reward.redemption.updated</c>).</summary>
    public const string RewardRedemptionUpdated = "channel.reward.redemption.updated";

    /// <summary>Fired when the livestream goes live or ends (<c>livestream.status.updated</c>).</summary>
    public const string LivestreamStatusUpdated = "livestream.status.updated";

    /// <summary>Fired when livestream metadata (title, category, etc.) changes (<c>livestream.metadata.updated</c>).</summary>
    public const string LivestreamMetadataUpdated = "livestream.metadata.updated";

    /// <summary>Fired when a user is banned or timed out in a channel (<c>moderation.banned</c>).</summary>
    public const string ModerationBanned = "moderation.banned";

    /// <summary>Fired when KICKs (platform currency) are gifted in a channel (<c>kicks.gifted</c>).</summary>
    public const string KicksGifted = "kicks.gifted";
}

// ─── Payload types ───────────────────────────────────────────────────────────

/// <summary>Payload for a <see cref="KickEventTypes.ChatMessageSent"/> event.</summary>
public sealed class KickChatMessagePayload
{
    /// <summary>Unique identifier for the chat message.</summary>
    [JsonPropertyName("message_id")] public string MessageId { get; init; } = string.Empty;

    /// <summary>The channel broadcaster the message was sent in, if present.</summary>
    [JsonPropertyName("broadcaster")] public KickWebhookUser? Broadcaster { get; init; }

    /// <summary>The user who sent the message.</summary>
    [JsonPropertyName("sender")] public required KickWebhookUser Sender { get; init; }

    /// <summary>The plain-text content of the message.</summary>
    [JsonPropertyName("content")] public string Content { get; init; } = string.Empty;

    /// <summary>Emotes present in the message, with their character positions.</summary>
    [JsonPropertyName("emotes")] public IReadOnlyList<KickEmote>? Emotes { get; init; }

    /// <summary>UTC timestamp when the message was created.</summary>
    [JsonPropertyName("created_at")] public DateTimeOffset CreatedAt { get; init; }

    /// <summary>Information about the message this is a reply to, if applicable.</summary>
    [JsonPropertyName("replies_to")] public KickReplyInfo? RepliesTo { get; init; }
}

/// <summary>Identifies the original message that a chat reply is directed at.</summary>
public sealed class KickReplyInfo
{
    /// <summary>Unique identifier of the original message being replied to.</summary>
    [JsonPropertyName("message_id")] public string MessageId { get; init; } = string.Empty;

    /// <summary>Content of the original message, if provided.</summary>
    [JsonPropertyName("content")] public string? Content { get; init; }

    /// <summary>The user who sent the original message.</summary>
    [JsonPropertyName("sender")] public KickWebhookUser? Sender { get; init; }
}

/// <summary>An emote used within a chat message.</summary>
public sealed class KickEmote
{
    /// <summary>The unique identifier of the emote.</summary>
    [JsonPropertyName("emote_id")] public string EmoteId { get; init; } = string.Empty;

    /// <summary>Character positions within the message text where the emote appears.</summary>
    [JsonPropertyName("positions")] public IReadOnlyList<KickEmotePosition>? Positions { get; init; }
}

/// <summary>Character start/end indices of an emote occurrence within a message string.</summary>
public sealed class KickEmotePosition
{
    /// <summary>Zero-based start character index of the emote token.</summary>
    [JsonPropertyName("s")] public int Start { get; init; }

    /// <summary>Zero-based end character index (exclusive) of the emote token.</summary>
    [JsonPropertyName("e")] public int End { get; init; }
}

/// <summary>Payload for a <see cref="KickEventTypes.ChannelFollowed"/> event.</summary>
public sealed class KickChannelFollowedPayload
{
    /// <summary>The channel that was followed.</summary>
    [JsonPropertyName("broadcaster")] public KickWebhookUser? Broadcaster { get; init; }

    /// <summary>The user who followed the channel.</summary>
    [JsonPropertyName("follower")] public required KickWebhookUser Follower { get; init; }
}

/// <summary>
/// Payload for <see cref="KickEventTypes.SubscriptionNew"/> and
/// <see cref="KickEventTypes.SubscriptionRenewal"/> events.
/// </summary>
public sealed class KickSubscriptionPayload
{
    /// <summary>The channel the subscription is for.</summary>
    [JsonPropertyName("broadcaster")] public KickWebhookUser? Broadcaster { get; init; }

    /// <summary>The user who subscribed or renewed.</summary>
    [JsonPropertyName("subscriber")] public required KickWebhookUser Subscriber { get; init; }

    /// <summary>Subscription duration in months.</summary>
    [JsonPropertyName("duration")] public int Duration { get; init; }

    /// <summary>UTC timestamp when the subscription was created or renewed.</summary>
    [JsonPropertyName("created_at")] public DateTimeOffset CreatedAt { get; init; }

    /// <summary>UTC timestamp when the subscription expires.</summary>
    [JsonPropertyName("expires_at")] public DateTimeOffset ExpiresAt { get; init; }
}

/// <summary>Payload for a <see cref="KickEventTypes.SubscriptionGifts"/> event.</summary>
public sealed class KickSubscriptionGiftsPayload
{
    /// <summary>The channel the subscriptions were gifted to.</summary>
    [JsonPropertyName("broadcaster")] public KickWebhookUser? Broadcaster { get; init; }

    /// <summary>The user who sent the gift subscriptions.</summary>
    [JsonPropertyName("gifter")] public required KickWebhookUser Gifter { get; init; }

    /// <summary>The users who received the gifted subscriptions.</summary>
    [JsonPropertyName("giftees")] public IReadOnlyList<KickWebhookUser>? Giftees { get; init; }

    /// <summary>UTC timestamp when the gifts were sent.</summary>
    [JsonPropertyName("created_at")] public DateTimeOffset CreatedAt { get; init; }

    /// <summary>UTC timestamp when the gifted subscriptions expire.</summary>
    [JsonPropertyName("expires_at")] public DateTimeOffset ExpiresAt { get; init; }
}

/// <summary>Payload for a <see cref="KickEventTypes.RewardRedemptionUpdated"/> event.</summary>
public sealed class KickRewardRedemptionPayload
{
    /// <summary>Unique identifier for the redemption.</summary>
    [JsonPropertyName("id")] public string Id { get; init; } = string.Empty;

    /// <summary>Optional text input provided by the viewer when redeeming the reward.</summary>
    [JsonPropertyName("user_input")] public string? UserInput { get; init; }

    /// <summary>Current status of the redemption (e.g. <c>FULFILLED</c>, <c>CANCELED</c>).</summary>
    [JsonPropertyName("status")] public string Status { get; init; } = string.Empty;

    /// <summary>UTC timestamp when the reward was redeemed.</summary>
    [JsonPropertyName("redeemed_at")] public DateTimeOffset RedeemedAt { get; init; }

    /// <summary>Details of the reward that was redeemed.</summary>
    [JsonPropertyName("reward")] public required KickRewardInfo Reward { get; init; }

    /// <summary>The viewer who redeemed the reward.</summary>
    [JsonPropertyName("redeemer")] public required KickWebhookUser Redeemer { get; init; }

    /// <summary>The channel broadcaster whose reward was redeemed.</summary>
    [JsonPropertyName("broadcaster")] public KickWebhookUser? Broadcaster { get; init; }
}

/// <summary>Summary information about a channel-points reward.</summary>
public sealed class KickRewardInfo
{
    /// <summary>Unique identifier of the reward.</summary>
    [JsonPropertyName("id")] public string Id { get; init; } = string.Empty;

    /// <summary>Display title of the reward.</summary>
    [JsonPropertyName("title")] public string Title { get; init; } = string.Empty;

    /// <summary>Cost of the reward in channel points.</summary>
    [JsonPropertyName("cost")] public int Cost { get; init; }

    /// <summary>Optional description of the reward.</summary>
    [JsonPropertyName("description")] public string? Description { get; init; }
}

/// <summary>Payload for a <see cref="KickEventTypes.LivestreamStatusUpdated"/> event.</summary>
public sealed class KickLivestreamStatusPayload
{
    /// <summary>The broadcaster whose livestream status changed.</summary>
    [JsonPropertyName("broadcaster")] public KickWebhookUser? Broadcaster { get; init; }

    /// <summary><see langword="true"/> if the stream is currently live; <see langword="false"/> if it has ended.</summary>
    [JsonPropertyName("is_live")] public bool IsLive { get; init; }

    /// <summary>Title of the livestream at the time of the status change, if available.</summary>
    [JsonPropertyName("title")] public string? Title { get; init; }

    /// <summary>UTC timestamp when the stream started, if it is live.</summary>
    [JsonPropertyName("started_at")] public DateTimeOffset? StartedAt { get; init; }

    /// <summary>UTC timestamp when the stream ended, if it has finished.</summary>
    [JsonPropertyName("ended_at")] public DateTimeOffset? EndedAt { get; init; }
}

/// <summary>Payload for a <see cref="KickEventTypes.LivestreamMetadataUpdated"/> event.</summary>
public sealed class KickLivestreamMetadataPayload
{
    /// <summary>The broadcaster whose stream metadata was updated.</summary>
    [JsonPropertyName("broadcaster")] public KickWebhookUser? Broadcaster { get; init; }

    /// <summary>The updated stream metadata.</summary>
    [JsonPropertyName("metadata")] public required KickStreamMetadata Metadata { get; init; }
}

/// <summary>Metadata describing a Kick livestream (title, language, category, etc.).</summary>
public sealed class KickStreamMetadata
{
    /// <summary>Stream title.</summary>
    [JsonPropertyName("title")] public string? Title { get; init; }

    /// <summary>Language code for the stream (e.g. <c>en</c>).</summary>
    [JsonPropertyName("language")] public string? Language { get; init; }

    /// <summary>Whether the stream is flagged as containing mature content.</summary>
    [JsonPropertyName("has_mature_content")] public bool? HasMatureContent { get; init; }

    /// <summary>Stream category/game.</summary>
    [JsonPropertyName("category")] public KickStreamCategory? Category { get; init; }
}

/// <summary>A Kick stream category (game or content category).</summary>
public sealed class KickStreamCategory
{
    /// <summary>Unique numeric identifier of the category.</summary>
    [JsonPropertyName("id")] public int Id { get; init; }

    /// <summary>Display name of the category.</summary>
    [JsonPropertyName("name")] public string Name { get; init; } = string.Empty;

    /// <summary>URL of the category thumbnail image, if available.</summary>
    [JsonPropertyName("thumbnail")] public string? Thumbnail { get; init; }
}

/// <summary>Payload for a <see cref="KickEventTypes.ModerationBanned"/> event.</summary>
public sealed class KickModerationBannedPayload
{
    /// <summary>The channel broadcaster in whose channel the ban occurred.</summary>
    [JsonPropertyName("broadcaster")] public KickWebhookUser? Broadcaster { get; init; }

    /// <summary>The moderator who issued the ban, if known.</summary>
    [JsonPropertyName("moderator")] public KickWebhookUser? Moderator { get; init; }

    /// <summary>The user who was banned.</summary>
    [JsonPropertyName("banned_user")] public required KickWebhookUser BannedUser { get; init; }

    /// <summary>Details about the ban (reason, duration, expiry).</summary>
    [JsonPropertyName("metadata")] public required KickBanMetadata Metadata { get; init; }
}

/// <summary>Details about a ban or timeout action.</summary>
public sealed class KickBanMetadata
{
    /// <summary>Reason provided for the ban, if any.</summary>
    [JsonPropertyName("reason")] public string? Reason { get; init; }

    /// <summary>UTC timestamp when the ban was created.</summary>
    [JsonPropertyName("created_at")] public DateTimeOffset? CreatedAt { get; init; }

    /// <summary>UTC timestamp when the ban expires (permanent ban if <see langword="null"/>).</summary>
    [JsonPropertyName("expires_at")] public DateTimeOffset? ExpiresAt { get; init; }
}

/// <summary>Payload for a <see cref="KickEventTypes.KicksGifted"/> event.</summary>
public sealed class KickKicksGiftedPayload
{
    /// <summary>The channel broadcaster in whose channel the KICKs were gifted.</summary>
    [JsonPropertyName("broadcaster")] public KickWebhookUser? Broadcaster { get; init; }

    /// <summary>The user who sent the KICKs gift.</summary>
    [JsonPropertyName("sender")] public required KickWebhookUser Sender { get; init; }

    /// <summary>Information about the gift (amount, type, etc.).</summary>
    [JsonPropertyName("gift")] public required KickGiftInfo Gift { get; init; }

    /// <summary>UTC timestamp when the gift was sent.</summary>
    [JsonPropertyName("created_at")] public DateTimeOffset CreatedAt { get; init; }
}

/// <summary>Details about a KICKs gift sent in a channel.</summary>
public sealed class KickGiftInfo
{
    /// <summary>Number of KICKs gifted.</summary>
    [JsonPropertyName("amount")] public int Amount { get; init; }

    /// <summary>Display name of the gift item.</summary>
    [JsonPropertyName("name")] public string Name { get; init; } = string.Empty;

    /// <summary>Type identifier for the gift, if applicable.</summary>
    [JsonPropertyName("type")] public string? Type { get; init; }

    /// <summary>Tier of the gift, if applicable.</summary>
    [JsonPropertyName("tier")] public string? Tier { get; init; }

    /// <summary>Optional message included with the gift.</summary>
    [JsonPropertyName("message")] public string? Message { get; init; }

    /// <summary>Duration in seconds that the gift is pinned in chat, if applicable.</summary>
    [JsonPropertyName("pinned_time_seconds")] public int? PinnedTimeSeconds { get; init; }
}
