using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Delivery.Indexing.Selectors;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.DependencyInjection;
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
            StaticServiceProvider.Instance.GetRequiredService<IRequestCultureService>(),
            StaticServiceProvider.Instance.GetRequiredService<IApiDocumentUrlService>())
    {
    }

    [Obsolete("Please use the non-obsolete constructor. Will be removed in V17.")]
    public ChildrenSelector(
        IPublishedContentCache publishedContentCache,
        IRequestRoutingService requestRoutingService,
        IRequestPreviewService requestPreviewService,
        IRequestCultureService requestCultureService,
        IApiDocumentUrlService apiDocumentUrlService)
        : this(requestRoutingService, requestPreviewService, requestCultureService, apiDocumentUrlService)
    {
    }

    public ChildrenSelector(
        IRequestRoutingService requestRoutingService,
        IRequestPreviewService requestPreviewService,
        IRequestCultureService requestCultureService,
        IApiDocumentUrlService apiDocumentUrlService)
        : base(requestRoutingService, requestPreviewService, requestCultureService, apiDocumentUrlService)
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
