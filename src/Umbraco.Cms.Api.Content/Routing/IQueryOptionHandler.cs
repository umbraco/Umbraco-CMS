using Examine.Search;
using Umbraco.Cms.Core.ContentApi;

namespace Umbraco.Cms.Api.Content.Routing;

public interface IQueryOptionHandler : IQueryHandler
{
    IBooleanOperation? BuildApiIndexQuery(IQuery query, string fieldValue);
}
