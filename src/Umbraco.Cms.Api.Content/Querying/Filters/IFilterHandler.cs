using Examine.Search;
using Umbraco.Cms.Core.ContentApi;

namespace Umbraco.Cms.Api.Content.Querying.Filters;

public interface IFilterHandler : IQueryHandler
{
    IBooleanOperation? BuildFilterIndexQuery(IQuery query, string filterValueString);
}
