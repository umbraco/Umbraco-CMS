using Umbraco.Cms.Core;
using Umbraco.Cms.Search.Core.Models.Searching.Sorting;

namespace Umbraco.Cms.Search.BackOffice.Services;

internal static class Sorting
{
    public static Sorter Default() => new ScoreSorter(Direction.Descending);
}
