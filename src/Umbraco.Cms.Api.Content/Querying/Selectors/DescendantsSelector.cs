using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.PublishedCache;

namespace Umbraco.Cms.Api.Content.Querying.Selectors;

internal sealed class DescendantsSelector : QueryOptionBase, ISelectorHandler
{
    private const string DescendantsSpecifier = "descendants:";

    public DescendantsSelector(IPublishedSnapshotAccessor publishedSnapshotAccessor,
        IRequestRoutingService requestRoutingService)
        : base(publishedSnapshotAccessor, requestRoutingService)
    {
    }

    /// <inheritdoc />
    public bool CanHandle(string query)
        => query.StartsWith(DescendantsSpecifier, StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc/>
    public SelectorOption BuildSelectorOption(string selector)
    {
        var fieldValue = selector.Substring(DescendantsSpecifier.Length);
        Guid? id = GetGuidFromQuery(fieldValue);

        return new SelectorOption
        {
            FieldName = "ancestorKeys",
            Value = id.ToString() ?? string.Empty
        };
    }
}
