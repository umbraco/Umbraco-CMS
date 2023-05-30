using Umbraco.Cms.Api.Delivery.Indexing.Selectors;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Delivery.Querying.Selectors;

public sealed class AncestorsSelector : QueryOptionBase, ISelectorHandler
{
    private const string AncestorsSpecifier = "ancestors:";
    private readonly IPublishedSnapshotAccessor _publishedSnapshotAccessor;

    public AncestorsSelector(IPublishedSnapshotAccessor publishedSnapshotAccessor, IRequestRoutingService requestRoutingService)
        : base(publishedSnapshotAccessor, requestRoutingService) =>
        _publishedSnapshotAccessor = publishedSnapshotAccessor;

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

        IPublishedSnapshot publishedSnapshot = _publishedSnapshotAccessor.GetRequiredPublishedSnapshot();

        IPublishedContent contentItem = publishedSnapshot.Content?.GetById((Guid)id)
                                        ?? throw new InvalidOperationException("Could not obtain the content cache");

        var ancestorKeys = contentItem.Ancestors().Select(a => a.Key.ToString("D")).ToArray();

        return new SelectorOption
        {
            FieldName = AncestorsSelectorIndexer.FieldName,
            Values = ancestorKeys
        };
    }
}
