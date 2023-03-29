using Umbraco.Cms.Core.ContentApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Content.Querying.Selectors;

internal sealed class AncestorsSelector : QueryOptionBase, ISelectorHandler
{
    private const string AncestorsSpecifier = "ancestors:";
    private readonly IPublishedSnapshotAccessor _publishedSnapshotAccessor;

    public AncestorsSelector(IPublishedSnapshotAccessor publishedSnapshotAccessor, IRequestRoutingService requestRoutingService)
        : base(publishedSnapshotAccessor, requestRoutingService) =>
        _publishedSnapshotAccessor = publishedSnapshotAccessor;

    /// <inheritdoc />
    public bool CanHandle(string queryString)
        => queryString.StartsWith(AncestorsSpecifier, StringComparison.OrdinalIgnoreCase);

    public SelectorOption? BuildSelectorOption(string selectorValueString)
    {
        var fieldValue = selectorValueString.Substring(AncestorsSpecifier.Length);
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

        return new SelectorOption
        {
            FieldName = "id",
            Value = string.Join(" ", ancestorKeys)
        };
    }
}
