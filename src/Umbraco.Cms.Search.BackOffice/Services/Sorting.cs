using Umbraco.Cms.Core;
using Umbraco.Cms.Search.Core.Models.Searching.Sorting;

namespace Umbraco.Cms.Search.BackOffice.Services;

/// <summary>
/// Provides default sorting for backoffice index searches.
/// </summary>
internal static class Sorting
{
    /// <summary>
    /// Gets the default sorter: descending relevance score.
    /// </summary>
    public static Sorter Default() => new ScoreSorter(Direction.Descending);
}
