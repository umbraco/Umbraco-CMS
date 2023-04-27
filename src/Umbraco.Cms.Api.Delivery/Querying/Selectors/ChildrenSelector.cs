using Umbraco.Cms.Api.Delivery.Indexing.Selectors;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.PublishedCache;

namespace Umbraco.Cms.Api.Delivery.Querying.Selectors;

public sealed class ChildrenSelector : QueryOptionBase, ISelectorHandler
{
    private const string ChildrenSpecifier = "children:";

    public ChildrenSelector(IPublishedSnapshotAccessor publishedSnapshotAccessor, IRequestRoutingService requestRoutingService)
        : base(publishedSnapshotAccessor, requestRoutingService)
    {
    }

    /// <inheritdoc />
    public bool CanHandle(string query)
        => query.StartsWith(ChildrenSpecifier, StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc/>
    public SelectorOption BuildSelectorOption(string selector)
    {
        var fieldValue = selector.Substring(ChildrenSpecifier.Length);
        Guid? id = GetGuidFromQuery(fieldValue);

        return new SelectorOption
        {
            FieldName = ChildrenSelectorIndexer.FieldName,
            Value = id.ToString() ?? string.Empty
        };
    }
}
