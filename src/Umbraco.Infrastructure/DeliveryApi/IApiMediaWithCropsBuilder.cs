using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Infrastructure.DeliveryApi;

public interface IApiMediaWithCropsBuilder
{
    ApiMediaWithCrops Build(MediaWithCrops media);

    ApiMediaWithCrops Build(IPublishedContent media);
}
