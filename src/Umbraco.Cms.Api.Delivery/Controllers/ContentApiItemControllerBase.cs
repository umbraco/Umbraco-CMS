using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Delivery.Controllers;

public abstract class ContentApiItemControllerBase : ContentApiControllerBase
{
    private readonly IPublicAccessService _publicAccessService;

    protected ContentApiItemControllerBase(
        IApiPublishedContentCache apiPublishedContentCache,
        IApiContentResponseBuilder apiContentResponseBuilder,
        IPublicAccessService publicAccessService)
        : base(apiPublishedContentCache, apiContentResponseBuilder)
        => _publicAccessService = publicAccessService;

    // NOTE: we're going to test for protected content at item endpoint level, because the check has already been
    //       performed at content index time for the query endpoint and we don't want that extra overhead when
    //       returning multiple items.
    protected bool IsProtected(IPublishedContent content) => _publicAccessService.IsProtected(content.Path);
}
