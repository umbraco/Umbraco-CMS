using Examine.Search;
using Umbraco.Cms.Core.ContentApi;
using Umbraco.Cms.Core.PublishedCache;

namespace Umbraco.Cms.Api.Content.Querying.Selectors;

internal sealed class ChildrenSelector : QueryOptionBase, ISelectorHandler
{
    private const string ChildrenSpecifier = "children:";

    public ChildrenSelector(IPublishedSnapshotAccessor publishedSnapshotAccessor, IRequestRoutingService requestRoutingService)
        : base(publishedSnapshotAccessor, requestRoutingService)
    {
    }

    /// <inheritdoc />
    public bool CanHandle(string queryString)
        => queryString.StartsWith(ChildrenSpecifier, StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc />
    public IBooleanOperation? BuildApiIndexQuery(IQuery query, string queryString)
    {
        var fieldValue = queryString.Substring(ChildrenSpecifier.Length);
        Guid? id = GetGuidFromQuery(fieldValue);

        return query.Field("parentKey", id.ToString());
    }
}
