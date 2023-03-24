using Examine.Search;
using Umbraco.Cms.Core.ContentApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Content.Routing;

internal sealed class AncestorsSelector : QueryOptionBase, ISelectorHandler
{
    private const string AncestorsSpecifier = "ancestors:";
    private readonly IPublishedSnapshotAccessor _publishedSnapshotAccessor;

    public AncestorsSelector(IPublishedSnapshotAccessor publishedSnapshotAccessor,
        IRequestRoutingService requestRoutingService)
        : base(publishedSnapshotAccessor, requestRoutingService) =>
        _publishedSnapshotAccessor = publishedSnapshotAccessor;

    public bool CanHandle(string queryString)
        => queryString.StartsWith(AncestorsSpecifier, StringComparison.OrdinalIgnoreCase);

    public IBooleanOperation? BuildApiIndexQuery(IQuery query, string queryString)
    {
        var fieldValue = queryString.Substring(AncestorsSpecifier.Length);
        Guid? id = GetGuidFromQuery(fieldValue);

        if (id is null ||
            !_publishedSnapshotAccessor.TryGetPublishedSnapshot(out IPublishedSnapshot? publishedSnapshot) ||
            publishedSnapshot?.Content is null)
        {
            return null;
        }

        // With the previous check we made sure that if we reach this, we already made sure that there is a valid content item
        IPublishedContent contentItem = publishedSnapshot.Content.GetById((Guid)id)!; // so it can't be null
        IEnumerable<Guid> ancestorKeys = contentItem.Ancestors().Select(a => a.Key);

        return query.Field("id", string.Join(" ", ancestorKeys)); // Look for any documents that have either of the ids
    }
}
