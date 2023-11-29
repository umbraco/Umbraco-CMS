using Umbraco.Cms.Api.Delivery.Indexing.Sorts;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DeliveryApi;

namespace Umbraco.Cms.Api.Delivery.Querying.Sorts;

public sealed class SortOrderSort : ISortHandler
{
    private const string SortOptionSpecifier = "sortOrder:";

    /// <inheritdoc />
    public bool CanHandle(string query)
        => query.StartsWith(SortOptionSpecifier, StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc/>
    public SortOption BuildSortOption(string sort)
    {
        var sortDirection = sort.Substring(SortOptionSpecifier.Length);

        return new SortOption
        {
            FieldName = SortOrderSortIndexer.FieldName,
            Direction = sortDirection.StartsWith("asc") ? Direction.Ascending : Direction.Descending
        };
    }
}
