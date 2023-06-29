using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.Builders;
using Umbraco.Cms.Api.Delivery.Routing;
using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Infrastructure.DeliveryApi;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Delivery.Controllers;

[VersionedDeliveryApiRoute("media")]
[ApiExplorerSettings(GroupName = "Media")]
public abstract class MediaApiControllerBase : DeliveryApiControllerBase
{
    private readonly IApiMediaWithCropsBuilder _apiMediaWithCropsBuilder;
    private readonly IPublishedSnapshotAccessor _publishedSnapshotAccessor;
    private IPublishedMediaCache? _publishedMediaCache;

    protected MediaApiControllerBase(IPublishedSnapshotAccessor publishedSnapshotAccessor, IApiMediaWithCropsBuilder apiMediaWithCropsBuilder)
    {
        _publishedSnapshotAccessor = publishedSnapshotAccessor;
        _apiMediaWithCropsBuilder = apiMediaWithCropsBuilder;
    }

    protected IPublishedMediaCache PublishedMediaCache => _publishedMediaCache
        ??= _publishedSnapshotAccessor.GetRequiredPublishedSnapshot().Media
            ?? throw new InvalidOperationException("Could not obtain the published media cache");

    protected ApiMediaWithCrops BuildApiMediaWithCrops(IPublishedContent media)
        => _apiMediaWithCropsBuilder.Build(media);

    protected IActionResult ApiMediaQueryOperationStatusResult(ApiMediaQueryOperationStatus status) =>
        status switch
        {
            ApiMediaQueryOperationStatus.FilterOptionNotFound => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Filter option not found")
                .WithDetail("One of the attempted 'filter' options does not exist")
                .Build()),
            ApiMediaQueryOperationStatus.SelectorOptionNotFound => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Selector option not found")
                .WithDetail("The attempted 'fetch' option does not exist")
                .Build()),
            ApiMediaQueryOperationStatus.SortOptionNotFound => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Sort option not found")
                .WithDetail("One of the attempted 'sort' options does not exist")
                .Build()),
            _ => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Unknown media query status")
                .WithDetail($"Media query status \"{status}\" was not expected here")
                .Build()),
        };
}
