using Examine;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence;

namespace Umbraco.Cms.Infrastructure.Examine;

/// <summary>
///     Performs the data lookups required to rebuild a content index
/// </summary>
public class ContentIndexPopulator : IndexPopulator<IUmbracoContentIndex>
{
    private readonly IContentService _contentService;
    private readonly IValueSetBuilder<IContent> _contentValueSetBuilder;
    private readonly ILogger<ContentIndexPopulator> _logger;
    private readonly int? _parentId;

    private readonly bool _publishedValuesOnly;
    private readonly IUmbracoDatabaseFactory _umbracoDatabaseFactory;

    /// <summary>
    ///     This is a static query, it's parameters don't change so store statically
    /// </summary>
    private IQuery<IContent>? _publishedQuery;

    /// <summary>
    ///     Default constructor to lookup all content data
    /// </summary>
    public ContentIndexPopulator(
        ILogger<ContentIndexPopulator> logger,
        IContentService contentService,
        IUmbracoDatabaseFactory umbracoDatabaseFactory,
        IContentValueSetBuilder contentValueSetBuilder)
        : this(logger, false, null, contentService, umbracoDatabaseFactory, contentValueSetBuilder)
    {
    }

    /// <summary>
    ///     Optional constructor allowing specifying custom query parameters
    /// </summary>
    public ContentIndexPopulator(
        ILogger<ContentIndexPopulator> logger,
        bool publishedValuesOnly,
        int? parentId,
        IContentService contentService,
        IUmbracoDatabaseFactory umbracoDatabaseFactory,
        IValueSetBuilder<IContent> contentValueSetBuilder)
    {
        _contentService = contentService ?? throw new ArgumentNullException(nameof(contentService));
        _umbracoDatabaseFactory = umbracoDatabaseFactory ?? throw new ArgumentNullException(nameof(umbracoDatabaseFactory));
        _contentValueSetBuilder = contentValueSetBuilder ?? throw new ArgumentNullException(nameof(contentValueSetBuilder));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _publishedValuesOnly = publishedValuesOnly;
        _parentId = parentId;
    }

    private IQuery<IContent> PublishedQuery => _publishedQuery ??=
        _umbracoDatabaseFactory.SqlContext.Query<IContent>().Where(x => x.Published);

    public override bool IsRegistered(IUmbracoContentIndex index) =>

        // check if it should populate based on published values
        _publishedValuesOnly == index.PublishedValuesOnly;

    protected override void PopulateIndexes(IReadOnlyList<IIndex> indexes)
    {
        if (indexes.Count == 0)
        {
            _logger.LogDebug($"{nameof(PopulateIndexes)} called with no indexes to populate. Typically means no index is registered with this populator.");
            return;
        }

        const int pageSize = 10000;
        var pageIndex = 0;

        var contentParentId = -1;
        if (_parentId.HasValue && _parentId.Value > 0)
        {
            contentParentId = _parentId.Value;
        }

        if (_publishedValuesOnly)
        {
            IndexPublishedContent(contentParentId, pageIndex, pageSize, indexes);
        }
        else
        {
            IndexAllContent(contentParentId, pageIndex, pageSize, indexes);
        }
    }

    protected void IndexAllContent(int contentParentId, int pageIndex, int pageSize, IReadOnlyList<IIndex> indexes)
    {
        IContent[] content;

        do
        {
            content = _contentService.GetPagedDescendants(contentParentId, pageIndex, pageSize, out _).ToArray();

            if (content.Length > 0)
            {
                var valueSets = _contentValueSetBuilder.GetValueSets(content).ToList();

                // ReSharper disable once PossibleMultipleEnumeration
                foreach (IIndex index in indexes)
                {
                    index.IndexItems(valueSets);
                }
            }

            pageIndex++;
        }
        while (content.Length == pageSize);
    }

    protected void IndexPublishedContent(int contentParentId, int pageIndex, int pageSize, IReadOnlyList<IIndex> indexes)
    {
        IContent[] content;

        var publishedPages = new HashSet<int>();

        do
        {
            // add the published filter
            // note: We will filter for published variants in the validator
            content = _contentService.GetPagedDescendants(contentParentId, pageIndex, pageSize, out _, PublishedQuery, Ordering.By("Path")).ToArray();

            if (content.Length > 0)
            {
                var indexableContent = new List<IContent>();

                foreach (IContent item in content)
                {
                    if (item.Level == 1)
                    {
                        // first level pages are always published so no need to filter them
                        indexableContent.Add(item);
                        publishedPages.Add(item.Id);
                    }
                    else
                    {
                        if (publishedPages.Contains(item.ParentId))
                        {
                            // only index when parent is published
                            publishedPages.Add(item.Id);
                            indexableContent.Add(item);
                        }
                    }
                }

                var valueSets = _contentValueSetBuilder.GetValueSets(indexableContent.ToArray()).ToList();

                foreach (IIndex index in indexes)
                {
                    index.IndexItems(valueSets);
                }
            }

            pageIndex++;
        }
        while (content.Length == pageSize);
    }
}
