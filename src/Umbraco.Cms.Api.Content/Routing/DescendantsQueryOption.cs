using Examine.Search;

namespace Umbraco.Cms.Api.Content.Routing;

public class DescendantsQueryOption : IQueryOptionHandler
{
    public bool CanHandle(string queryString)
    {
        const string childrenSpecifier = "descendants:";
        return queryString.StartsWith(childrenSpecifier, StringComparison.OrdinalIgnoreCase);
    }

    public IBooleanOperation BuildApiIndexQuery(IQuery query, string fieldValue)
        => query.Field("ancestorKeys", fieldValue);
}
