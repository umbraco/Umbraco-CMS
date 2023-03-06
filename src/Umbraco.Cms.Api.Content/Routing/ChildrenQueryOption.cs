using Examine.Search;

namespace Umbraco.Cms.Api.Content.Routing;

public class ChildrenQueryOption : IQueryOptionHandler
{
    public bool CanHandle(string queryString)
    {
        const string childrenSpecifier = "children:";
        return queryString.StartsWith(childrenSpecifier, StringComparison.OrdinalIgnoreCase);
    }

    public IBooleanOperation BuildApiIndexQuery(IQuery query, string fieldValue)
        => query.Field("parentKey", fieldValue);
}
