using Umbraco.Cms.Core.ContentApi;
using Umbraco.Cms.Core.PublishedCache;

namespace Umbraco.Cms.Api.Content.Querying.Filters;

internal sealed class ContentTypeFilter : QueryOptionBase, IFilterHandler
{
    private const string ContentTypeSpecifier = "contentType:";

    public ContentTypeFilter(IPublishedSnapshotAccessor publishedSnapshotAccessor, IRequestRoutingService requestRoutingService)
        : base(publishedSnapshotAccessor, requestRoutingService)
    {
    }

    /// <inheritdoc />
    public bool CanHandle(string queryString)
        => queryString.StartsWith(ContentTypeSpecifier, StringComparison.OrdinalIgnoreCase);

    public FilterOption BuildFilterOption(string filterValueString)
    {
        var alias = filterValueString.Substring(ContentTypeSpecifier.Length);

        var filter = new FilterOption
        {
            FieldName = "__NodeTypeAlias",
            Value = string.Empty
        };

        if (alias.StartsWith('!'))
        {
            filter.Value = alias.Substring(1);
            filter.Operator = FilterOperation.IsNot;
        }
        else
        {
            filter.Value = alias;
            filter.Operator = FilterOperation.Is;
        }

        return filter;
    }
}
