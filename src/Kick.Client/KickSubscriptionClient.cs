using System.Net.Http.Json;
using System.Text.Json;

namespace Kick.Client;

/// <summary>
/// Manages Kick webhook event subscriptions via
/// <c>POST / DELETE /public/v1/events/subscriptions</c>.
/// Subscribe on runtime start, unsubscribe on runtime stop.
/// Kick auto-unsubscribes after 24 h of consecutive endpoint failures.
/// </summary>
public sealed class KickSubscriptionClient
{
    private static readonly JsonSerializerOptions _json = new(JsonSerializerDefaults.Web);
    private readonly HttpClient _http;

    /// <summary>
    /// Initializes a new <see cref="KickSubscriptionClient"/> with the supplied HTTP client.
    /// </summary>
    /// <param name="http">
    /// An <see cref="HttpClient"/> pre-configured with the Kick API base address and Bearer token.
    /// </param>
    public KickSubscriptionClient(HttpClient http)
    {
        ArgumentNullException.ThrowIfNull(http);
        _http = http;
    }

    /// <summary>Subscribes to a single Kick webhook event type for a broadcaster.</summary>
    public async Task<KickSubscriptionResult> SubscribeAsync(
        string eventType,
        string version,
        string broadcasterId,
        string webhookUrl,
        CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(eventType);
        ArgumentException.ThrowIfNullOrWhiteSpace(broadcasterId);
        ArgumentException.ThrowIfNullOrWhiteSpace(webhookUrl);

        var body = new
        {
            type = eventType,
            version,
            broadcaster_user_id = broadcasterId,
            webhook_url = webhookUrl,
        };

        using HttpResponseMessage response = await _http
            .PostAsJsonAsync("/public/v1/events/subscriptions", body, _json, ct)
            .ConfigureAwait(false);

        return new KickSubscriptionResult(
            IsSuccess: response.IsSuccessStatusCode,
            StatusCode: (int)response.StatusCode,
            EventType: eventType,
            BroadcasterId: broadcasterId);
    }

    /// <summary>Unsubscribes from a Kick webhook event type for a broadcaster.</summary>
    public async Task<bool> UnsubscribeAsync(
        string eventType,
        string version,
        string broadcasterId,
        CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(eventType);
        ArgumentException.ThrowIfNullOrWhiteSpace(broadcasterId);

        string url = $"/public/v1/events/subscriptions" +
                     $"?type={Uri.EscapeDataString(eventType)}" +
                     $"&version={Uri.EscapeDataString(version)}" +
                     $"&broadcaster_user_id={Uri.EscapeDataString(broadcasterId)}";

        using HttpResponseMessage response = await _http.DeleteAsync(url, ct).ConfigureAwait(false);
        return response.IsSuccessStatusCode;
    }

    /// <summary>
    /// Subscribes to all supported Kick webhook event types for a broadcaster.
    /// Recommended to call on provider runtime start.
    /// </summary>
    public async Task<IReadOnlyList<KickSubscriptionResult>> SubscribeAllAsync(
        string broadcasterId,
        string webhookUrl,
        CancellationToken ct = default)
    {
        string[] eventTypes =
        [
            Webhooks.KickEventTypes.ChatMessageSent,
            Webhooks.KickEventTypes.ChannelFollowed,
            Webhooks.KickEventTypes.SubscriptionNew,
            Webhooks.KickEventTypes.SubscriptionRenewal,
            Webhooks.KickEventTypes.SubscriptionGifts,
            Webhooks.KickEventTypes.RewardRedemptionUpdated,
            Webhooks.KickEventTypes.LivestreamStatusUpdated,
            Webhooks.KickEventTypes.LivestreamMetadataUpdated,
            Webhooks.KickEventTypes.ModerationBanned,
            Webhooks.KickEventTypes.KicksGifted,
        ];

        List<KickSubscriptionResult> results = new(eventTypes.Length);
        foreach (string eventType in eventTypes)
        {
            results.Add(await SubscribeAsync(eventType, "1", broadcasterId, webhookUrl, ct)
                .ConfigureAwait(false));
        }

        return results;
    }

    /// <summary>
    /// Unsubscribes from all supported Kick webhook event types for a broadcaster.
    /// Recommended to call on provider runtime stop.
    /// </summary>
    public async Task UnsubscribeAllAsync(string broadcasterId, CancellationToken ct = default)
    {
        string[] eventTypes =
        [
            Webhooks.KickEventTypes.ChatMessageSent,
            Webhooks.KickEventTypes.ChannelFollowed,
            Webhooks.KickEventTypes.SubscriptionNew,
            Webhooks.KickEventTypes.SubscriptionRenewal,
            Webhooks.KickEventTypes.SubscriptionGifts,
            Webhooks.KickEventTypes.RewardRedemptionUpdated,
            Webhooks.KickEventTypes.LivestreamStatusUpdated,
            Webhooks.KickEventTypes.LivestreamMetadataUpdated,
            Webhooks.KickEventTypes.ModerationBanned,
            Webhooks.KickEventTypes.KicksGifted,
        ];

        foreach (string eventType in eventTypes)
        {
            _ = await UnsubscribeAsync(eventType, "1", broadcasterId, ct).ConfigureAwait(false);
        }
    }
}

/// <summary>Outcome of a single webhook event subscription attempt.</summary>
public sealed record KickSubscriptionResult(
    bool IsSuccess,
    int StatusCode,
    string EventType,
    string BroadcasterId);
