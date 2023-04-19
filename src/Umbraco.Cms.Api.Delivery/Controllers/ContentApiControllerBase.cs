using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Delivery.Routing;
using Umbraco.Cms.Core.DeliveryApi;

namespace Umbraco.Cms.Api.Delivery.Controllers;

[VersionedDeliveryApiRoute("content")]
[ApiExplorerSettings(GroupName = "Content")]
public abstract class ContentApiControllerBase : DeliveryApiControllerBase
{
    protected IApiPublishedContentCache ApiPublishedContentCache { get; }

    protected IApiContentResponseBuilder ApiContentResponseBuilder { get; }

    protected ContentApiControllerBase(IApiPublishedContentCache apiPublishedContentCache, IApiContentResponseBuilder apiContentResponseBuilder)
    {
        ApiPublishedContentCache = apiPublishedContentCache;
        ApiContentResponseBuilder = apiContentResponseBuilder;
    }
}
