using Umbraco.Cms.Core;
using Umbraco.Cms.Core.ContentApi;
using SortType = Umbraco.Cms.Core.ContentApi.SortType;

namespace Umbraco.Cms.Api.Content.Querying.Sorts;

internal sealed class SortOrderSort : ISortHandler
{
    private const string SortOptionSpecifier = "sortOrder:";

    /// <inheritdoc />
    public bool CanHandle(string queryString)
        => queryString.StartsWith(SortOptionSpecifier, StringComparison.OrdinalIgnoreCase);

    public SortOption BuildSortOption(string sortValueString)
    {
        var sortDirection = sortValueString.Substring(SortOptionSpecifier.Length);

        return new SortOption
        {
            FieldName = "sortOrder",
            Direction = sortDirection.StartsWith("asc") ? Direction.Ascending : Direction.Descending,
            SortType = SortType.Int
        };
    }
}
