using Examine;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence;

namespace Umbraco.Cms.Infrastructure.Examine;

public class DeliveryApiContentIndexPopulator : IndexPopulator
{
    private readonly IContentService _contentService;
    private readonly IUmbracoDatabaseFactory _umbracoDatabaseFactory;
    private readonly IDeliveryApiContentIndexValueSetBuilder _deliveryContentIndexValueSetBuilder;

    public DeliveryApiContentIndexPopulator(IContentService contentService, IDeliveryApiContentIndexValueSetBuilder deliveryContentIndexValueSetBuilder, IUmbracoDatabaseFactory umbracoDatabaseFactory)
    {
        _contentService = contentService;
        _deliveryContentIndexValueSetBuilder = deliveryContentIndexValueSetBuilder;
        _umbracoDatabaseFactory = umbracoDatabaseFactory;
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
        IQuery<IContent> publishedQuery = _umbracoDatabaseFactory.SqlContext.Query<IContent>().Where(x => x.Published);
        do
        {
            descendants = _contentService.GetPagedDescendants(Constants.System.Root, pageIndex, pageSize, out _, publishedQuery, Ordering.By("Path")).ToArray();

            // children of unpublished content can still be published; we need to filter them out, as they're not supposed to go into the index.
            // as we're querying published content and ordering by path, we can construct a list of "allowed" published content IDs like this.
            foreach (IContent content in descendants)
            {
                if (content.Level == 1 || publishedContentIds.Contains(content.ParentId))
                {
                    publishedContentIds.Add(content.Id);
                }
            }

            // now we can utilize the list of "allowed" published content to filter out the ones we don't want in the index.
            IContent[] allowedDescendants = descendants.Where(content => publishedContentIds.Contains(content.Id)).ToArray();
            ValueSet[] valueSets = _deliveryContentIndexValueSetBuilder.GetValueSets(allowedDescendants).ToArray();

            // ReSharper disable once PossibleMultipleEnumeration
            foreach (IIndex index in indexes)
            {
                index.IndexItems(valueSets);
            }

            pageIndex++;
        }
        while (descendants.Length == pageSize);
    }
}
