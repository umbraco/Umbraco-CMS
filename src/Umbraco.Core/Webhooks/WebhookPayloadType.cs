namespace Umbraco.Cms.Core.Webhooks;

public enum WebhookPayloadType
{
    /// <summary>
    /// Mostly returns only entity ids (guids) but definitely no service models
    /// </summary>
    Minimal = 0,

    /// <summary>
    /// Minimal extended for certain webhooks with relevant information ready to consume, like deliveryApi models for content/media saving/publishing
    /// </summary>
    Extended = 1,

    /// <summary>
    /// Mix of minimal and full service models with old int references
    /// </summary>
    [Obsolete("Planned for removal in v17")]
    Legacy = 2,
}
