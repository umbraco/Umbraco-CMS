using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.Builders;
using Umbraco.Cms.Api.Delivery.Filters;
using Umbraco.Cms.Api.Delivery.Routing;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Delivery.Controllers;

[VersionedDeliveryApiRoute("content")]
[ApiExplorerSettings(GroupName = "Content")]
[LocalizeFromAcceptLanguageHeader]
[ValidateStartItem]
public abstract class ContentApiControllerBase : DeliveryApiControllerBase
{
    protected IApiPublishedContentCache ApiPublishedContentCache { get; }

    protected IApiContentResponseBuilder ApiContentResponseBuilder { get; }

    protected ContentApiControllerBase(IApiPublishedContentCache apiPublishedContentCache, IApiContentResponseBuilder apiContentResponseBuilder)
    {
        ApiPublishedContentCache = apiPublishedContentCache;
        ApiContentResponseBuilder = apiContentResponseBuilder;
    }

    protected IActionResult ApiContentQueryOperationStatusResult(ApiContentQueryOperationStatus status) =>
        status switch
        {
            ApiContentQueryOperationStatus.FilterOptionNotFound => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Filter option not found")
                .WithDetail("One of the attempted 'filter' options does not exist")
                .Build()),
            ApiContentQueryOperationStatus.SelectorOptionNotFound => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Selector option not found")
                .WithDetail("The attempted 'fetch' option does not exist")
                .Build()),
            ApiContentQueryOperationStatus.SortOptionNotFound => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Sort option not found")
                .WithDetail("One of the attempted 'sort' options does not exist")
                .Build()),
        };
}
