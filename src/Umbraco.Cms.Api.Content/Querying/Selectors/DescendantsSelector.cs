using Examine.Search;
using Umbraco.Cms.Core.ContentApi;
using Umbraco.Cms.Core.PublishedCache;

namespace Umbraco.Cms.Api.Content.Querying.Selectors;

internal sealed class DescendantsSelector : QueryOptionBase, ISelectorHandler
{
    private const string DescendantsSpecifier = "descendants:";

    public DescendantsSelector(IPublishedSnapshotAccessor publishedSnapshotAccessor,
        IRequestRoutingService requestRoutingService)
        : base(publishedSnapshotAccessor, requestRoutingService)
    {
    }

    /// <inheritdoc />
    public bool CanHandle(string queryString)
        => queryString.StartsWith(DescendantsSpecifier, StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc />
    public IBooleanOperation? BuildApiIndexQuery(IQuery query, string queryString)
    {
        var fieldValue = queryString.Substring(DescendantsSpecifier.Length);
        Guid? id = GetGuidFromQuery(fieldValue);

        return query.Field("ancestorKeys", id.ToString());
    }
}
