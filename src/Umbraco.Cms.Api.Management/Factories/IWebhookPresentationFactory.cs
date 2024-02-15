using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Web.Common.Models;

namespace Umbraco.Cms.Api.Management.Factories;

public interface IWebhookPresentationFactory
{
    WebhookViewModel Create(IWebhook webhook);
}
