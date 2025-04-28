namespace Umbraco.Cms.Core.Webhooks;

public enum WebhookPayloadType
{
    /// <summary>
    /// Returns the minimal information required to identify the resources affected, providing identifiers to support retrieval of additional detail about the event from the source system.
    /// </summary>
    /// <remarks>
    /// Expected to be the default option from Umbraco 17.
    /// </remarks>
    Minimal = 0,

    /// <summary>
    /// Provides the minimal payload extended for certain webhooks with relevant information ready to consume. For example, content delivery API models are provided for content and media save and publish events.
    /// </summary>
    Extended = 1,

    /// <summary>
    /// Legacy payloads containing a mix of minimal information and full service models with legacy integer references.
    /// </summary>
    /// <remarks>
    /// This is the default option for Umbraco 16 and will be available as a configurable option for Umbraco 17. 
 Expected to be removed in Umbraco 18.
    /// </remarks>
    [Obsolete("Planned for removal in v18")]
    Legacy = 2,
}
