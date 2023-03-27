using System;
using Examine.Search;

namespace Umbraco.Cms.Api.Content.Querying.Sorts;

internal sealed class SortOrderSort : ISortHandler
{
    private const string SortOptionSpecifier = "sortOrder:";

    /// <inheritdoc />
    public bool CanHandle(string queryString)
        => queryString.StartsWith(SortOptionSpecifier, StringComparison.OrdinalIgnoreCase);

    public IOrdering? BuildSortIndexQuery(IBooleanOperation queryCriteria, string sortValueString)
    {
        var sortDirection = sortValueString.Substring(SortOptionSpecifier.Length);

        return sortDirection.StartsWith("asc")
            ? queryCriteria.OrderBy(new SortableField("sortOrder", SortType.Int))
            : queryCriteria.OrderByDescending(new SortableField("sortOrder", SortType.Int));
    }
}
