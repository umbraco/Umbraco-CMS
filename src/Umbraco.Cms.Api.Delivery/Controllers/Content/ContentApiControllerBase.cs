using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Umbraco.Cms.Api.Common.Builders;
using Umbraco.Cms.Api.Delivery.Filters;
using Umbraco.Cms.Api.Delivery.Routing;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Delivery.Controllers.Content;

[DeliveryApiAccess]
[VersionedDeliveryApiRoute("content")]
[ApiExplorerSettings(GroupName = "Content")]
[LocalizeFromAcceptLanguageHeader]
[ValidateStartItem]
[OutputCache(PolicyName = Constants.DeliveryApi.OutputCache.ContentCachePolicy)]
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
            _ => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Unknown content query status")
                .WithDetail($"Content query status \"{status}\" was not expected here")
                .Build()),
        };

    /// <summary>
    ///     Creates a 403 Forbidden result.
    /// </summary>
    /// <remarks>
    ///     Use this method instead of <see cref="ControllerBase.Forbid()"/> on the controller base. The latter will yield
    ///     a redirect to an access denied URL because of the default cookie auth scheme. This method ensures that a proper
    ///     403 Forbidden status code is returned to the client.
    /// </remarks>
    protected IActionResult Forbidden() => new StatusCodeResult(StatusCodes.Status403Forbidden);
}
