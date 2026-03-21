using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Agash.Webhook.Abstractions;

namespace Kick.Client.Webhooks;

/// <summary>
/// Transport-neutral Kick webhook handler.
/// Verifies the RSA-PKCS1v15 SHA-256 signature and deserializes the payload
/// into a typed <see cref="KickWebhookEvent"/>.
/// </summary>
public sealed class KickWebhookHandler : IWebhookHandler<KickWebhookEvent>
{
    private static readonly JsonSerializerOptions _json = new(JsonSerializerDefaults.Web);

    private readonly KickWebhookOptions _defaultOptions;

    /// <summary>
    /// Initializes a new <see cref="KickWebhookHandler"/> with optional default options.
    /// </summary>
    /// <param name="defaultOptions">
    /// Default validation options. If <see langword="null"/>, a <see cref="KickWebhookOptions"/>
    /// instance with its default values is used.
    /// </param>
    public KickWebhookHandler(KickWebhookOptions? defaultOptions = null)
    {
        _defaultOptions = defaultOptions ?? new KickWebhookOptions();
    }

    /// <inheritdoc/>
    public Task<WebhookHandleResult<KickWebhookEvent>> HandleAsync(
        WebhookRequest request, CancellationToken ct = default)
        => HandleAsync(request, _defaultOptions, ct);

    /// <summary>Handles the webhook with an explicit <paramref name="options"/> override.</summary>
    public Task<WebhookHandleResult<KickWebhookEvent>> HandleAsync(
        WebhookRequest request, KickWebhookOptions options, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(options);

        string? messageId = request.GetFirstHeaderValue(KickWebhookHeaders.MessageId);
        string? subscriptionId = request.GetFirstHeaderValue(KickWebhookHeaders.SubscriptionId);
        string? signature = request.GetFirstHeaderValue(KickWebhookHeaders.Signature);
        string? timestamp = request.GetFirstHeaderValue(KickWebhookHeaders.MessageTimestamp);
        string? eventType = request.GetFirstHeaderValue(KickWebhookHeaders.EventType);
        string? eventVersion = request.GetFirstHeaderValue(KickWebhookHeaders.EventVersion);

        if (string.IsNullOrEmpty(messageId) || string.IsNullOrEmpty(timestamp)
            || string.IsNullOrEmpty(eventType))
        {
            return Task.FromResult(new WebhookHandleResult<KickWebhookEvent>
            {
                Response = WebhookResponse.PlainText(400, "Missing required Kick-Event-* headers."),
                IsAuthenticated = false,
                IsKnownEvent = false,
                FailureReason = "Missing required Kick-Event-* headers.",
            });
        }

        if (options.RequireValidSignature)
        {
            if (string.IsNullOrEmpty(signature))
            {
                return Task.FromResult(new WebhookHandleResult<KickWebhookEvent>
                {
                    Response = WebhookResponse.Empty(401),
                    IsAuthenticated = false,
                    IsKnownEvent = false,
                    FailureReason = "Missing Kick-Event-Signature header.",
                });
            }

            if (!VerifySignature(request.Body, messageId, timestamp, signature, options.PublicKeyPem))
            {
                return Task.FromResult(new WebhookHandleResult<KickWebhookEvent>
                {
                    Response = WebhookResponse.Empty(401),
                    IsAuthenticated = false,
                    IsKnownEvent = false,
                    FailureReason = "Kick-Event-Signature verification failed.",
                });
            }
        }

        if (!DateTimeOffset.TryParse(timestamp, out DateTimeOffset parsedTimestamp))
        {
            parsedTimestamp = DateTimeOffset.UtcNow;
        }

        object? payload = DeserializePayload(eventType, request.Body);
        if (payload is null)
        {
            return Task.FromResult(new WebhookHandleResult<KickWebhookEvent>
            {
                Response = WebhookResponse.Empty(200),
                IsAuthenticated = true,
                IsKnownEvent = false,
            });
        }

        KickWebhookEvent evt = new()
        {
            MessageId = messageId,
            SubscriptionId = subscriptionId ?? string.Empty,
            EventType = eventType,
            EventVersion = eventVersion ?? "1",
            MessageTimestamp = parsedTimestamp,
            RawBody = request.Body,
            Payload = payload,
        };

        return Task.FromResult(new WebhookHandleResult<KickWebhookEvent>
        {
            Response = WebhookResponse.Empty(200),
            IsAuthenticated = true,
            IsKnownEvent = true,
            Event = evt,
        });
    }

    private static bool VerifySignature(
        byte[] body, string messageId, string timestamp, string signatureHeader, string? publicKeyPem)
    {
        if (string.IsNullOrEmpty(publicKeyPem))
        {
            return false;
        }

        try
        {
            // Signature input: "{messageId}.{timestamp}.{rawBody}"
            byte[] signatureInput = Encoding.UTF8.GetBytes(
                $"{messageId}.{timestamp}.{Encoding.UTF8.GetString(body)}");
            byte[] signatureBytes = Convert.FromBase64String(signatureHeader);

            using RSA rsa = RSA.Create();
            rsa.ImportFromPem(publicKeyPem);
            byte[] hash = SHA256.HashData(signatureInput);
            return rsa.VerifyHash(hash, signatureBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        }
        catch
        {
            return false;
        }
    }

    private static object? DeserializePayload(string eventType, byte[] body)
    {
        try
        {
            return eventType switch
            {
                KickEventTypes.ChatMessageSent =>
                    JsonSerializer.Deserialize<KickChatMessagePayload>(body, _json),
                KickEventTypes.ChannelFollowed =>
                    JsonSerializer.Deserialize<KickChannelFollowedPayload>(body, _json),
                KickEventTypes.SubscriptionNew or KickEventTypes.SubscriptionRenewal =>
                    JsonSerializer.Deserialize<KickSubscriptionPayload>(body, _json),
                KickEventTypes.SubscriptionGifts =>
                    JsonSerializer.Deserialize<KickSubscriptionGiftsPayload>(body, _json),
                KickEventTypes.RewardRedemptionUpdated =>
                    JsonSerializer.Deserialize<KickRewardRedemptionPayload>(body, _json),
                KickEventTypes.LivestreamStatusUpdated =>
                    JsonSerializer.Deserialize<KickLivestreamStatusPayload>(body, _json),
                KickEventTypes.LivestreamMetadataUpdated =>
                    JsonSerializer.Deserialize<KickLivestreamMetadataPayload>(body, _json),
                KickEventTypes.ModerationBanned =>
                    JsonSerializer.Deserialize<KickModerationBannedPayload>(body, _json),
                KickEventTypes.KicksGifted =>
                    JsonSerializer.Deserialize<KickKicksGiftedPayload>(body, _json),
                _ => null,
            };
        }
        catch
        {
            return null;
        }
    }
}
