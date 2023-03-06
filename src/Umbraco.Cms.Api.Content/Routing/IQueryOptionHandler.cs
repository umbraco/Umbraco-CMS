using Examine.Search;

namespace Umbraco.Cms.Api.Content.Routing;

public interface IQueryOptionHandler
{
    bool CanHandle(string queryString);

    IBooleanOperation BuildApiIndexQuery(IQuery query, string fieldValue);
}
