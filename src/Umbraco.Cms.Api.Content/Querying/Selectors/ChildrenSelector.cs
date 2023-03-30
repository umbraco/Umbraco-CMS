using Umbraco.Cms.Core.ContentApi;
using Umbraco.Cms.Core.PublishedCache;

namespace Umbraco.Cms.Api.Content.Querying.Selectors;

internal sealed class ChildrenSelector : QueryOptionBase, ISelectorHandler
{
    private const string ChildrenSpecifier = "children:";

    public ChildrenSelector(IPublishedSnapshotAccessor publishedSnapshotAccessor, IRequestRoutingService requestRoutingService)
        : base(publishedSnapshotAccessor, requestRoutingService)
    {
    }

    /// <inheritdoc />
    public bool CanHandle(string queryString)
        => queryString.StartsWith(ChildrenSpecifier, StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc/>
    public SelectorOption BuildSelectorOption(string selectorValueString)
    {
        var fieldValue = selectorValueString.Substring(ChildrenSpecifier.Length);
        Guid? id = GetGuidFromQuery(fieldValue);

        return new SelectorOption
        {
            FieldName = "parentKey",
            Value = id.ToString() ?? string.Empty
        };
    }
}
