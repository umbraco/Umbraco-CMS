using Examine;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Examine;

internal sealed class DeliveryApiContentIndexPopulator : IndexPopulator
{
    private readonly IContentService _contentService;
    private readonly IUmbracoDatabaseFactory _umbracoDatabaseFactory;
    private readonly IDeliveryApiContentIndexValueSetBuilder _deliveryContentIndexValueSetBuilder;
    private DeliveryApiSettings _deliveryApiSettings;

    public DeliveryApiContentIndexPopulator(
        IContentService contentService,
        IDeliveryApiContentIndexValueSetBuilder deliveryContentIndexValueSetBuilder,
        IUmbracoDatabaseFactory umbracoDatabaseFactory,
        IOptionsMonitor<DeliveryApiSettings> deliveryApiSettings)
    {
        _contentService = contentService;
        _deliveryContentIndexValueSetBuilder = deliveryContentIndexValueSetBuilder;
        _umbracoDatabaseFactory = umbracoDatabaseFactory;
        _deliveryApiSettings = deliveryApiSettings.CurrentValue;
        deliveryApiSettings.OnChange(settings => _deliveryApiSettings = settings);
        RegisterIndex(Constants.UmbracoIndexes.DeliveryApiContentIndexName);
    }

    protected override void PopulateIndexes(IReadOnlyList<IIndex> indexes)
    {
        if (indexes.Any() is false)
        {
            return;
        }

        const int pageSize = 10000;
        var pageIndex = 0;
        var publishedContentIds = new HashSet<int>();

        IContent[] descendants;
        IQuery<IContent> publishedQuery = _umbracoDatabaseFactory.SqlContext.Query<IContent>().Where(x => x.Published && x.Trashed == false);
        do
        {
            descendants = _contentService.GetPagedDescendants(Constants.System.Root, pageIndex, pageSize, out _, publishedQuery, Ordering.By("Path")).ToArray();

            // there are a few rules we need to abide to when populating the index:
            // - children of unpublished content can still be published; we need to filter them out, as they're not supposed to go into the index.
            // - content of disallowed content types are not allowed in the index, but their children are
            // as we're querying published content and ordering by path, we can construct a list of "allowed" published content IDs like this.
            var allowedDescendants = new List<IContent>();
            foreach (IContent content in descendants)
            {
                if (_deliveryApiSettings.IsDisallowedContentType(content.ContentType.Alias))
                {
                    // the content type is disallowed; make sure we consider all its children as candidates for the index anyway
                    publishedContentIds.Add(content.Id);
                    continue;
                }

                // content at root level is by definition published, because we only fetch published content in the query above.
                // content not at root level should be included only if their parents are included (unbroken chain of published content)
                if (content.Level == 1 || publishedContentIds.Contains(content.ParentId))
                {
                    publishedContentIds.Add(content.Id);
                    allowedDescendants.Add(content);
                }
            }

            // now build the value sets based on the "allowed" published content only
            ValueSet[] valueSets = _deliveryContentIndexValueSetBuilder.GetValueSets(allowedDescendants.ToArray()).ToArray();

            foreach (IIndex index in indexes)
            {
                index.IndexItems(valueSets);
            }

            pageIndex++;
        }
        while (descendants.Length == pageSize);
    }
}
