using System;
using Examine.Search;

namespace Umbraco.Cms.Api.Content.Querying.Sorts;

internal sealed class NameSort : ISortHandler
{
    private const string SortOptionSpecifier = "name:";

    /// <inheritdoc />
    public bool CanHandle(string queryString)
        => queryString.StartsWith(SortOptionSpecifier, StringComparison.OrdinalIgnoreCase);

    public IOrdering? BuildSortIndexQuery(IBooleanOperation queryCriteria, string sortValueString)
    {
        var sortDirection = sortValueString.Substring(SortOptionSpecifier.Length);

        return sortDirection.StartsWith("asc")
            ? queryCriteria.OrderBy(new SortableField("name", SortType.String))
            : queryCriteria.OrderByDescending(new SortableField("name", SortType.String));
    }
}
