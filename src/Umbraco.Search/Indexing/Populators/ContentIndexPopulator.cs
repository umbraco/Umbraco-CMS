using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Search.Configuration;

namespace Umbraco.Search.Indexing.Populators;

/// <summary>
///     Performs the data lookups required to rebuild a content index
/// </summary>
public class ContentIndexPopulator : IndexPopulator
{
    private readonly ISearchProvider _provider;
    private readonly IUmbracoIndexesConfiguration _configuration;
    private readonly IContentService _contentService;
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
        ISearchProvider provider,
        IUmbracoIndexesConfiguration configuration,
        IUmbracoDatabaseFactory umbracoDatabaseFactory)
        : this(logger, provider, false, null, contentService, configuration, umbracoDatabaseFactory)
    {
        _provider = provider;
        _configuration = configuration;
    }

    public override bool IsRegistered(string index)
    {
        if (base.IsRegistered(index))
        {
            return true;
        }

        var indexer = _provider.GetIndex(index);
        if (!(indexer is IUmbracoIndex<IContentBase> casted))
        {
            return false;
        }

        var configuration = _configuration.Configuration(index);
        return configuration.PublishedValuesOnly == _publishedValuesOnly;
    }

    /// <summary>
    ///     Optional constructor allowing specifying custom query parameters
    /// </summary>
    public ContentIndexPopulator(
        ILogger<ContentIndexPopulator> logger,
        ISearchProvider provider,
        bool publishedValuesOnly,
        int? parentId,
        IContentService contentService,
        IUmbracoIndexesConfiguration configuration,
        IUmbracoDatabaseFactory umbracoDatabaseFactory)
    {
        _contentService = contentService ?? throw new ArgumentNullException(nameof(contentService));
        _configuration = configuration;
        _umbracoDatabaseFactory =
            umbracoDatabaseFactory ?? throw new ArgumentNullException(nameof(umbracoDatabaseFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _provider = provider;
        _publishedValuesOnly = publishedValuesOnly;
        _parentId = parentId;
    }

    private IQuery<IContent> PublishedQuery => _publishedQuery ??=
        _umbracoDatabaseFactory.SqlContext.Query<IContent>().Where(x => x.Published);


    protected override void PopulateIndexes(IReadOnlyList<string> indexes)
    {
        if (indexes.Count == 0)
        {
            _logger.LogDebug(
                $"{nameof(PopulateIndexes)} called with no indexes to populate. Typically means no index is registered with this populator.");
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

    protected void IndexAllContent(int contentParentId, int pageIndex, int pageSize, IReadOnlyList<string> indexes)
    {
        IContent[] content;

        do
        {
            content = _contentService.GetPagedDescendants(contentParentId, pageIndex, pageSize, out _).ToArray();


            // ReSharper disable once PossibleMultipleEnumeration
            // ReSharper disable once PossibleMultipleEnumeration
            foreach (string index in indexes)
            {
                _provider.GetIndex<IContentBase>(index)?.IndexItems(content);
            }

            pageIndex++;
        } while (content.Length == pageSize);
    }

    protected void IndexPublishedContent(int contentParentId, int pageIndex, int pageSize,
        IReadOnlyList<string> indexes)
    {
        IContent[] content;

        var publishedPages = new HashSet<int>();

        do
        {
            // add the published filter
            // note: We will filter for published variants in the validator
            content = _contentService.GetPagedDescendants(contentParentId, pageIndex, pageSize, out _, PublishedQuery,
                Ordering.By("Path")).ToArray();

            var indexableContent = new List<IContent>();

            foreach (IContent item in content)
            {
                if (item.Level == 1)
                {
                    // first level pages are always published so no need to filter them
                    indexableContent.Add(item);
                    publishedPages.Add(item.Id);
                }
                else if (publishedPages.Contains(item.ParentId))
                {
                    // only index when parent is published
                    publishedPages.Add(item.Id);
                    indexableContent.Add(item);
                }
            }


            foreach (string index in indexes)
            {
                _provider.GetIndex<IContentBase>(index)?.IndexItems(content);
            }

            pageIndex++;
        } while (content.Length == pageSize);
    }
}
