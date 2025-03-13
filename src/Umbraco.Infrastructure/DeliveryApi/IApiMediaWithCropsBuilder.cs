using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Infrastructure.DeliveryApi;

public interface IApiMediaWithCropsBuilder
{
    IApiMediaWithCrops Build(MediaWithCrops media);

    IApiMediaWithCrops Build(IPublishedContent media);
}
