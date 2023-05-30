using Umbraco.Cms.Api.Delivery.Indexing.Selectors;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Extensions;

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
