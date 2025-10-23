using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.Webhooks;

public class WebhookPayloadProviderCollection(Func<IEnumerable<IWebhookPayloadProvider>> items) : BuilderCollectionBase<IWebhookPayloadProvider>(items);
