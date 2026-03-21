namespace Kick.Client.Authentication;

/// <summary>
/// String constants for every Kick OAuth 2.1 scope, plus preset scope combinations
/// for common integration scenarios.
/// </summary>
public static class KickScopes
{
    // ─── Individual scopes ────────────────────────────────────────────────────

    /// <summary>View user information (username, streamer ID, etc.).</summary>
    public const string UserRead = "user:read";

    /// <summary>View channel information (description, category, etc.).</summary>
    public const string ChannelRead = "channel:read";

    /// <summary>Update livestream metadata (title, category, etc.).</summary>
    public const string ChannelWrite = "channel:write";

    /// <summary>Read channel points rewards.</summary>
    public const string ChannelRewardsRead = "channel:rewards:read";

    /// <summary>Read, add, edit and delete channel points rewards.</summary>
    public const string ChannelRewardsWrite = "channel:rewards:write";

    /// <summary>Send chat messages / operate chat bots.</summary>
    public const string ChatWrite = "chat:write";

    /// <summary>Read the stream URL and stream key.</summary>
    public const string StreamKeyRead = "streamkey:read";

    /// <summary>Subscribe to all channel events (chat, follows, subscriptions, etc.).</summary>
    public const string EventsSubscribe = "events:subscribe";

    /// <summary>Execute ban and unban actions on users.</summary>
    public const string ModerationBan = "moderation:ban";

    /// <summary>Execute moderation actions on chat messages (delete, pin, etc.).</summary>
    public const string ModerationChatMessageManage = "moderation:chat_message:manage";

    /// <summary>View KICKs-related information (leaderboards, etc.).</summary>
    public const string KicksRead = "kicks:read";

    // ─── Preset combinations ─────────────────────────────────────────────────

    /// <summary>
    /// Scopes required for a chat bot integration:
    /// <c>user:read channel:read chat:write events:subscribe</c>.
    /// </summary>
    public static string ChatBot { get; } = Join(UserRead, ChannelRead, ChatWrite, EventsSubscribe);

    /// <summary>
    /// Scopes for a full webhook-based provider (events, metadata, reward management, moderation):
    /// <c>user:read channel:read channel:write channel:rewards:read channel:rewards:write events:subscribe moderation:ban kicks:read</c>.
    /// </summary>
    public static string WebhookProvider { get; } = Join(
        UserRead, ChannelRead, ChannelWrite,
        ChannelRewardsRead, ChannelRewardsWrite,
        EventsSubscribe, ModerationBan, KicksRead);

    /// <summary>
    /// Read-only scopes that grant no write or moderation access:
    /// <c>user:read channel:read channel:rewards:read kicks:read</c>.
    /// </summary>
    public static string ReadOnly { get; } = Join(UserRead, ChannelRead, ChannelRewardsRead, KicksRead);

    // ─── Helper ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Joins the supplied <paramref name="scopes"/> into a single space-separated string
    /// suitable for the OAuth <c>scope</c> parameter.
    /// </summary>
    /// <param name="scopes">One or more scope constants to combine.</param>
    /// <returns>A space-separated scope string.</returns>
    public static string Join(params string[] scopes) => string.Join(' ', scopes);
}
