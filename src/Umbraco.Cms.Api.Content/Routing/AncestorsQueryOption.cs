using Examine.Search;
using Umbraco.Extensions;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;

namespace Umbraco.Cms.Api.Content.Routing;

public class AncestorsQueryOption : IQueryOptionHandler
{
    private readonly IPublishedSnapshotAccessor _publishedSnapshotAccessor;

    public AncestorsQueryOption(IPublishedSnapshotAccessor publishedSnapshotAccessor)
    {
        _publishedSnapshotAccessor = publishedSnapshotAccessor;
    }

    public bool CanHandle(string queryString)
    {
        const string childrenSpecifier = "ancestors:";
        return queryString.StartsWith(childrenSpecifier, StringComparison.OrdinalIgnoreCase);
    }

    public IBooleanOperation? BuildApiIndexQuery(IQuery query, string fieldValue)
    {
        if (!Guid.TryParse(fieldValue, out Guid id))
        {
            return null;
        }

        if (!_publishedSnapshotAccessor.TryGetPublishedSnapshot(out IPublishedSnapshot? publishedSnapshot) || publishedSnapshot?.Content is null)
        {
            return null;
        }

        IPublishedContent? contentItem = publishedSnapshot.Content.GetById(id);

        if (contentItem is null)
        {
            return null;
        }

        IEnumerable<Guid> ancestorKeys = contentItem.Ancestors().Select(a => a.Key);

        return query.Field("id", string.Join(" ", ancestorKeys)); // Look for any documents that have either of the ids
    }
}
