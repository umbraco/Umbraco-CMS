using Umbraco.Cms.Core.Webhooks;

namespace Umbraco.Cms.Core;

public static partial class Constants
{
    /// <summary>
    ///     Contains constants for webhook configuration.
    /// </summary>
    public static class Webhooks
    {
        /// <summary>
        /// Gets the default webhook payload type.
        /// </summary>
        public const WebhookPayloadType DefaultPayloadType = WebhookPayloadType.Minimal;
    }
}
