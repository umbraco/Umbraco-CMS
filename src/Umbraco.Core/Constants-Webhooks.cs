using Umbraco.Cms.Core.Webhooks;

namespace Umbraco.Cms.Core;

public static partial class Constants
{
    public static class Webhooks
    {
        /// <summary>
        /// Gets the default webhook payload type.
        /// </summary>
        /// <remarks>
        /// Currently, the default payload type is <see cref="WebhookPayloadType.Legacy"/> for backward compatibility toward Umbraco 17.
        /// From Umbraco 18 this will be changed to <see cref="WebhookPayloadType.Minimal"/>.
        /// </remarks>
        public const WebhookPayloadType DefaultPayloadType = WebhookPayloadType.Legacy;
    }
}
