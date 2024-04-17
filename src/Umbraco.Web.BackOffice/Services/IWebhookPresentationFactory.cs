using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Web.Common.Models;

namespace Umbraco.Cms.Web.BackOffice.Services;

[Obsolete("Will be moved to a new namespace in V14")]
public interface IWebhookPresentationFactory
{
    WebhookViewModel Create(IWebhook webhook);
}
