using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Delivery.Indexing.Selectors;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Delivery.Querying.Selectors;

public sealed class ChildrenSelector : QueryOptionBase, ISelectorHandler
{
    private const string ChildrenSpecifier = "children:";

    [Obsolete("Please use the non-obsolete constructor. Will be removed in V17.")]
    public ChildrenSelector(IPublishedContentCache publishedContentCache, IRequestRoutingService requestRoutingService)
        : this(
            requestRoutingService,
            StaticServiceProvider.Instance.GetRequiredService<IRequestPreviewService>(),
            StaticServiceProvider.Instance.GetRequiredService<IApiDocumentUrlService>(),
            StaticServiceProvider.Instance.GetRequiredService<IVariationContextAccessor>())
    {
    }

    [Obsolete("Please use the non-obsolete constructor. Will be removed in V17.")]
    public ChildrenSelector(
        IPublishedContentCache publishedContentCache,
        IRequestRoutingService requestRoutingService,
        IRequestPreviewService requestPreviewService,
        IApiDocumentUrlService apiDocumentUrlService,
        IVariationContextAccessor variationContextAccessor)
        : this(requestRoutingService, requestPreviewService, apiDocumentUrlService, variationContextAccessor)
    {
    }

    public ChildrenSelector(
        IRequestRoutingService requestRoutingService,
        IRequestPreviewService requestPreviewService,
        IApiDocumentUrlService apiDocumentUrlService,
        IVariationContextAccessor variationContextAccessor)
        : base(requestRoutingService, requestPreviewService, apiDocumentUrlService, variationContextAccessor)
    {
    }

    /// <inheritdoc />
    public bool CanHandle(string query)
        => query.StartsWith(ChildrenSpecifier, StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc/>
    public SelectorOption BuildSelectorOption(string selector)
    {
        var fieldValue = selector[ChildrenSpecifier.Length..];
        var id = GetGuidFromQuery(fieldValue)?.ToString("D");

        return new SelectorOption
        {
            FieldName = ChildrenSelectorIndexer.FieldName,
            Values = id.IsNullOrWhiteSpace() == false
                ? new[] { id }
                : Array.Empty<string>()
        };
    }
}
