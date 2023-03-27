using Examine.Search;
using Umbraco.Cms.Core.ContentApi;

namespace Umbraco.Cms.Api.Content.Querying.Sorts;

public interface ISortHandler : IQueryHandler
{
    IOrdering? BuildSortIndexQuery(IBooleanOperation queryCriteria, string sortValueString);
}
