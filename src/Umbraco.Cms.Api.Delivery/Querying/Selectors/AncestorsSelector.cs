using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Delivery.Indexing.Selectors;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Delivery.Querying.Selectors;

public sealed class AncestorsSelector : QueryOptionBase, ISelectorHandler
{
    private const string AncestorsSpecifier = "ancestors:";
    private readonly IPublishedSnapshotAccessor _publishedSnapshotAccessor;
    private readonly IRequestPreviewService _requestPreviewService;

    [Obsolete("Please use the non-obsolete constructor. Will be removed in V17.")]
    public AncestorsSelector(IPublishedSnapshotAccessor publishedSnapshotAccessor,
        IRequestRoutingService requestRoutingService)
        : this(publishedSnapshotAccessor, requestRoutingService, StaticServiceProvider.Instance.GetRequiredService<IRequestPreviewService>())
    {
    }

    public AncestorsSelector(IPublishedSnapshotAccessor publishedSnapshotAccessor, IRequestRoutingService requestRoutingService, IRequestPreviewService requestPreviewService)
        : base(publishedSnapshotAccessor, requestRoutingService)
    {
        _publishedSnapshotAccessor = publishedSnapshotAccessor;
        _requestPreviewService = requestPreviewService;
    }

    /// <inheritdoc />
    public bool CanHandle(string query)
        => query.StartsWith(AncestorsSpecifier, StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc/>
    public SelectorOption BuildSelectorOption(string selector)
    {
        var fieldValue = selector[AncestorsSpecifier.Length..];
        Guid? id = GetGuidFromQuery(fieldValue);

        if (id is null)
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

        IPublishedContentCache contentCache = _publishedSnapshotAccessor.GetRequiredPublishedSnapshot()?.Content
                                         ?? throw new InvalidOperationException("Could not obtain the content cache");

        IPublishedContent? contentItem = contentCache.GetById(_requestPreviewService.IsPreview(), id.Value);

        if (contentItem is null)
        {
            // no such content item, make sure the selector does not yield any results
            return new SelectorOption
            {
                FieldName = AncestorsSelectorIndexer.FieldName,
                Values = Array.Empty<string>()
            };
        }

        var ancestorKeys = contentItem.Ancestors().Select(a => a.Key.ToString("D")).ToArray();

        return new SelectorOption
        {
            FieldName = AncestorsSelectorIndexer.FieldName,
            Values = ancestorKeys
        };
    }
}
