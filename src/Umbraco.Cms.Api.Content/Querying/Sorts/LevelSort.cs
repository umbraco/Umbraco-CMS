using System;
using Examine.Search;

namespace Umbraco.Cms.Api.Content.Querying.Sorts;

internal sealed class LevelSort : ISortHandler
{
    private const string SortOptionSpecifier = "level:";

    /// <inheritdoc />
    public bool CanHandle(string queryString)
        => queryString.StartsWith(SortOptionSpecifier, StringComparison.OrdinalIgnoreCase);

    public IOrdering? BuildSortIndexQuery(IBooleanOperation queryCriteria, string sortValueString)
    {
        var sortDirection = sortValueString.Substring(SortOptionSpecifier.Length);

        return sortDirection.StartsWith("asc")
            ? queryCriteria.OrderBy(new SortableField("level", SortType.Int))
            : queryCriteria.OrderByDescending(new SortableField("level", SortType.Int));
    }
}
