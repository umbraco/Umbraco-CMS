using Umbraco.Cms.Core;
using Umbraco.Cms.Core.ContentApi;

namespace Umbraco.Cms.Api.Content.Querying.Sorts;

internal sealed class PathSort : ISortHandler
{
    private const string SortOptionSpecifier = "path:";

    /// <inheritdoc />
    public bool CanHandle(string queryString)
        => queryString.StartsWith(SortOptionSpecifier, StringComparison.OrdinalIgnoreCase);

    public SortOption BuildSortOption(string sortValueString)
    {
        var sortDirection = sortValueString.Substring(SortOptionSpecifier.Length);

        return new SortOption
        {
            FieldName = "path",
            Direction = sortDirection.StartsWith("asc") ? Direction.Ascending : Direction.Descending,
            FieldType = FieldType.String
        };
    }
}
