using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.Filters;
using Umbraco.Cms.Api.Delivery.Filters;
using Umbraco.Cms.Api.Delivery.Routing;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DeliveryApi;

namespace Umbraco.Cms.Api.Delivery.Controllers;

[ApiController]
[VersionedDeliveryApiRoute("content")]
[ApiExplorerSettings(GroupName = "Content")]
[ApiVersion("1.0")]
[DeliveryApiAccess]
[JsonOptionsName(Constants.JsonOptionsNames.DeliveryApi)]
[LocalizeFromAcceptLanguageHeader]
public abstract class DeliveryApiControllerBase : Controller
{
    protected IApiPublishedContentCache ApiPublishedContentCache { get; }

    protected IApiContentResponseBuilder ApiContentResponseBuilder { get; }

    protected DeliveryApiControllerBase(IApiPublishedContentCache apiPublishedContentCache, IApiContentResponseBuilder apiContentResponseBuilder)
    {
        ApiPublishedContentCache = apiPublishedContentCache;
        ApiContentResponseBuilder = apiContentResponseBuilder;
    }
}
