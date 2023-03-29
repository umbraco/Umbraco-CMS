namespace Umbraco.Cms.Core.ContentApi;

public interface ISortHandler : IQueryHandler
{
    SortOption? BuildSortOption(string sortValueString);
}
