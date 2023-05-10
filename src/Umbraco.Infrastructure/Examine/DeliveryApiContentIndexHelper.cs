using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Examine;

internal sealed class DeliveryApiContentIndexHelper : IDeliveryApiContentIndexHelper
{
    private readonly IContentService _contentService;
    private readonly IUmbracoDatabaseFactory _umbracoDatabaseFactory;
    private DeliveryApiSettings _deliveryApiSettings;

    public DeliveryApiContentIndexHelper(
        IContentService contentService,
        IUmbracoDatabaseFactory umbracoDatabaseFactory,
        IOptionsMonitor<DeliveryApiSettings> deliveryApiSettings)
    {
        _contentService = contentService;
        _umbracoDatabaseFactory = umbracoDatabaseFactory;
        _deliveryApiSettings = deliveryApiSettings.CurrentValue;
        deliveryApiSettings.OnChange(settings => _deliveryApiSettings = settings);
    }

    public void EnumerateApplicableDescendantsForContentIndex(int rootContentId, Action<IContent[]> actionToPerform)
    {
        const int pageSize = 10000;
        var pageIndex = 0;
        var publishedContentIds = new HashSet<int> { rootContentId };

        IContent[] descendants;
        IQuery<IContent> publishedQuery = _umbracoDatabaseFactory.SqlContext.Query<IContent>().Where(x => x.Published && x.Trashed == false);
        do
        {
            descendants = _contentService.GetPagedDescendants(rootContentId, pageIndex, pageSize, out _, publishedQuery, Ordering.By("Path")).ToArray();

            // there are a few rules we need to abide to when populating the index:
            // - children of unpublished content can still be published; we need to filter them out, as they're not supposed to go into the index.
            // - content of disallowed content types are not allowed in the index, but their children are
            // as we're querying published content and ordering by path, we can construct a list of "allowed" published content IDs like this.
            var allowedDescendants = new List<IContent>();
            foreach (IContent descendant in descendants)
            {
                if (_deliveryApiSettings.IsDisallowedContentType(descendant.ContentType.Alias))
                {
                    // the content type is disallowed; make sure we consider all its children as candidates for the index anyway
                    publishedContentIds.Add(descendant.Id);
                    continue;
                }

                // content at root level is by definition published, because we only fetch published content in the query above.
                // content not at root level should be included only if their parents are included (unbroken chain of published content)
                if (descendant.Level == 1 || publishedContentIds.Contains(descendant.ParentId))
                {
                    publishedContentIds.Add(descendant.Id);
                    allowedDescendants.Add(descendant);
                }
            }

            actionToPerform(allowedDescendants.ToArray());

            pageIndex++;
        }
        while (descendants.Length == pageSize);
    }
}
