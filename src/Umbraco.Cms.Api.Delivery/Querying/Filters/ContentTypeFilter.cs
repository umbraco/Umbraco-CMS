using Umbraco.Cms.Api.Delivery.Indexing.Filters;
using Umbraco.Cms.Core.DeliveryApi;

namespace Umbraco.Cms.Api.Delivery.Querying.Filters;

public sealed class ContentTypeFilter : IFilterHandler
{
    private const string ContentTypeSpecifier = "contentType:";

    /// <inheritdoc />
    public bool CanHandle(string query)
        => query.StartsWith(ContentTypeSpecifier, StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc/>
    public FilterOption BuildFilterOption(string filter)
    {
        var filterValue = filter.Substring(ContentTypeSpecifier.Length);
        var negate = filterValue.StartsWith('!');
        var aliases = filterValue.TrimStart('!').Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

        return new FilterOption
        {
            FieldName = ContentTypeFilterIndexer.FieldName,
            Values = aliases,
            Operator = negate
                ? FilterOperation.IsNot
                : FilterOperation.Is
        };
    }
}
