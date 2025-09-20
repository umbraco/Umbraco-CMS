using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Infrastructure.DeliveryApi;

public interface IApiMediaWithCropsResponseBuilder
{
    IApiMediaWithCropsResponse Build(IPublishedContent media);
}
