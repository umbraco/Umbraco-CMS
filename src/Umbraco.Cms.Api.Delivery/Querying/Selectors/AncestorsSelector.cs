using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Delivery.Indexing.Selectors;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services.Navigation;

namespace Umbraco.Cms.Api.Delivery.Querying.Selectors;

public sealed class AncestorsSelector : QueryOptionBase, ISelectorHandler
{
    private readonly IDocumentNavigationQueryService _navigationQueryService;
    private const string AncestorsSpecifier = "ancestors:";

    [Obsolete("Please use the non-obsolete constructor. Will be removed in V17.")]
    public AncestorsSelector(
        IPublishedContentCache publishedContentCache,
        IRequestRoutingService requestRoutingService,
        IDocumentNavigationQueryService navigationQueryService,
        IRequestPreviewService requestPreviewService)
        : this(
            requestRoutingService,
            requestPreviewService,
            StaticServiceProvider.Instance.GetRequiredService<IApiDocumentUrlService>(),
            StaticServiceProvider.Instance.GetRequiredService<IVariationContextAccessor>(),
            navigationQueryService)
    {
    }

    [Obsolete("Please use the non-obsolete constructor. Will be removed in V17.")]
    public AncestorsSelector(
        IPublishedContentCache publishedContentCache,
        IRequestRoutingService requestRoutingService,
        IDocumentNavigationQueryService navigationQueryService)
        : this(
            requestRoutingService,
            StaticServiceProvider.Instance.GetRequiredService<IRequestPreviewService>(),
            StaticServiceProvider.Instance.GetRequiredService<IApiDocumentUrlService>(),
            StaticServiceProvider.Instance.GetRequiredService<IVariationContextAccessor>(),
            navigationQueryService)
    {
    }

    [Obsolete("Use the constructor that takes all parameters. Scheduled for removal in V17.")]
    public AncestorsSelector(IPublishedContentCache publishedContentCache, IRequestRoutingService requestRoutingService)
        : this(
            requestRoutingService,
            StaticServiceProvider.Instance.GetRequiredService<IRequestPreviewService>(),
            StaticServiceProvider.Instance.GetRequiredService<IApiDocumentUrlService>(),
            StaticServiceProvider.Instance.GetRequiredService<IVariationContextAccessor>(),
            StaticServiceProvider.Instance.GetRequiredService<IDocumentNavigationQueryService>())
    {
    }

    public AncestorsSelector(
        IRequestRoutingService requestRoutingService,
        IRequestPreviewService requestPreviewService,
        IApiDocumentUrlService apiDocumentUrlService,
        IVariationContextAccessor variationContextAccessor,
        IDocumentNavigationQueryService navigationQueryService)
        : base(requestRoutingService, requestPreviewService, apiDocumentUrlService, variationContextAccessor)
        => _navigationQueryService = navigationQueryService;

    [Obsolete("Use the constructor that takes all parameters. Scheduled for removal in V17.")]
    public AncestorsSelector(
        IRequestRoutingService requestRoutingService,
        IPublishedContentCache publishedContentCache,
        IRequestPreviewService requestPreviewService,
        IApiDocumentUrlService apiDocumentUrlService,
        IVariationContextAccessor variationContextAccessor,
        IDocumentNavigationQueryService navigationQueryService)
        : this(requestRoutingService, requestPreviewService, apiDocumentUrlService, variationContextAccessor, navigationQueryService)
    {
    }

    /// <inheritdoc />
    public bool CanHandle(string query)
        => query.StartsWith(AncestorsSpecifier, StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc/>
    public SelectorOption BuildSelectorOption(string selector)
    {
        var fieldValue = selector[AncestorsSpecifier.Length..];
        Guid? id = GetGuidFromQuery(fieldValue);

        if (id is null || _navigationQueryService.TryGetAncestorsKeys(id.Value, out IEnumerable<Guid> ancestorKeys) is false)
        {
            // Setting the Value to "" since that would yield no results.
            // It won't be appropriate to return null here since if we reached this,
            // it means that CanHandle() returned true, meaning that this Selector should be able to handle the selector value
            return new SelectorOption
            {
                FieldName = AncestorsSelectorIndexer.FieldName,
                Values = Array.Empty<string>()
            };
        }

        return new SelectorOption
        {
            FieldName = AncestorsSelectorIndexer.FieldName,
            Values = ancestorKeys.Select(key => key.ToString("D")).ToArray()
        };
    }
}
