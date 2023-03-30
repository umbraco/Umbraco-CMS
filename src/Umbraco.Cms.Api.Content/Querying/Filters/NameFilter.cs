using Umbraco.Cms.Core.ContentApi;
using Umbraco.Cms.Core.PublishedCache;

namespace Umbraco.Cms.Api.Content.Querying.Filters;

internal sealed class NameFilter : QueryOptionBase, IFilterHandler
{
    private const string NameSpecifier = "name:";

    public NameFilter(IPublishedSnapshotAccessor publishedSnapshotAccessor, IRequestRoutingService requestRoutingService)
        : base(publishedSnapshotAccessor, requestRoutingService)
    {
    }

    /// <inheritdoc />
    public bool CanHandle(string queryString)
        => queryString.StartsWith(NameSpecifier, StringComparison.OrdinalIgnoreCase);

    public FilterOption BuildFilterOption(string filterValueString)
    {
        var value = filterValueString.Substring(NameSpecifier.Length);

        var filter = new FilterOption
        {
            FieldName = "name",
            Value = string.Empty
        };

        // TODO: do we support negation?
        if (value.StartsWith('!'))
        {
            filter.Value = value.Substring(1);
            filter.Operator = FilterOperation.IsNot;
        }
        else
        {
            filter.Value = value;
            filter.Operator = FilterOperation.Is;
        }

        return filter;
    }
}
