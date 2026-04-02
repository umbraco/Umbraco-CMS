using Examine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DependencyInjection;
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

    private IndexingSettings _indexingSettings;

    /// <summary>
    ///     This is a static query, it's parameters don't change so store statically
    /// </summary>
    private IQuery<IContent>? _publishedQuery;

    /// <summary>
    /// Initializes a new instance of the <see cref="ContentIndexPopulator"/> class, which is responsible for populating the content index in Examine.
    /// </summary>
    /// <param name="logger">The logger used for logging diagnostic and error information.</param>
    /// <param name="contentService">The service used to access and manage Umbraco content items.</param>
    /// <param name="umbracoDatabaseFactory">The factory for creating Umbraco database connections.</param>
    /// <param name="contentValueSetBuilder">The builder used to create value sets for content indexing.</param>
    [Obsolete("Please use the non-obsolete constructor. Scheduled for removal in Umbraco 19.")]
    public ContentIndexPopulator(
        ILogger<ContentIndexPopulator> logger,
        IContentService contentService,
        IUmbracoDatabaseFactory umbracoDatabaseFactory,
        IContentValueSetBuilder contentValueSetBuilder)
        : this(logger, false, null, contentService, umbracoDatabaseFactory, contentValueSetBuilder, StaticServiceProvider.Instance.GetRequiredService<IOptionsMonitor<IndexingSettings>>())
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentIndexPopulator"/> class, used to populate the content index with all content data.
    /// </summary>
    /// <param name="logger">The logger instance used for diagnostic and error logging.</param>
    /// <param name="contentService">The service used to access and manage content items.</param>
    /// <param name="umbracoDatabaseFactory">The factory for creating Umbraco database connections.</param>
    /// <param name="contentValueSetBuilder">Builds value sets for content items to be indexed.</param>
    /// <param name="indexingSettings">The monitor providing indexing configuration settings.</param>
    public ContentIndexPopulator(
        ILogger<ContentIndexPopulator> logger,
        IContentService contentService,
        IUmbracoDatabaseFactory umbracoDatabaseFactory,
        IContentValueSetBuilder contentValueSetBuilder,
        IOptionsMonitor<IndexingSettings> indexingSettings)
        : this(logger, false, null, contentService, umbracoDatabaseFactory, contentValueSetBuilder, indexingSettings)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ContentIndexPopulator"/> class.
    /// </summary>
    /// <param name="logger">The logger used for diagnostic and operational messages.</param>
    /// <param name="publishedValuesOnly">If set to <c>true</c>, only published content values are indexed.</param>
    /// <param name="parentId">An optional parent content ID to restrict indexing to its descendants, or <c>null</c> for all content.</param>
    /// <param name="contentService">Service for accessing and managing content items.</param>
    /// <param name="umbracoDatabaseFactory">Factory for obtaining Umbraco database connections.</param>
    /// <param name="contentValueSetBuilder">Builds value sets for content items to be indexed.</param>
    [Obsolete("Please use the non-obsolete constructor. Scheduled for removal in Umbraco 19.")]
    public ContentIndexPopulator(
        ILogger<ContentIndexPopulator> logger,
        bool publishedValuesOnly,
        int? parentId,
        IContentService contentService,
        IUmbracoDatabaseFactory umbracoDatabaseFactory,
        IValueSetBuilder<IContent> contentValueSetBuilder)
        : this(logger, publishedValuesOnly, parentId, contentService, umbracoDatabaseFactory, contentValueSetBuilder, StaticServiceProvider.Instance.GetRequiredService<IOptionsMonitor<IndexingSettings>>())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ContentIndexPopulator"/> class with custom query parameters.
    /// </summary>
    /// <param name="logger">The logger used for diagnostic and error messages.</param>
    /// <param name="publishedValuesOnly">If true, only published content values are indexed.</param>
    /// <param name="parentId">An optional parent content ID to restrict indexing to a subtree.</param>
    /// <param name="contentService">The service used to access content items.</param>
    /// <param name="umbracoDatabaseFactory">The factory for obtaining Umbraco database connections.</param>
    /// <param name="contentValueSetBuilder">Builds value sets for content items to be indexed.</param>
    /// <param name="indexingSettings">Monitors configuration settings for indexing.</param>
    public ContentIndexPopulator(
        ILogger<ContentIndexPopulator> logger,
        bool publishedValuesOnly,
        int? parentId,
        IContentService contentService,
        IUmbracoDatabaseFactory umbracoDatabaseFactory,
        IValueSetBuilder<IContent> contentValueSetBuilder,
        IOptionsMonitor<IndexingSettings> indexingSettings)
    {
        _contentService = contentService ?? throw new ArgumentNullException(nameof(contentService));
        _umbracoDatabaseFactory = umbracoDatabaseFactory ?? throw new ArgumentNullException(nameof(umbracoDatabaseFactory));
        _contentValueSetBuilder = contentValueSetBuilder ?? throw new ArgumentNullException(nameof(contentValueSetBuilder));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _publishedValuesOnly = publishedValuesOnly;
        _parentId = parentId;
        _indexingSettings = indexingSettings.CurrentValue;

        indexingSettings.OnChange(change =>
        {
            _indexingSettings = change;
        });
    }

    private IQuery<IContent> PublishedQuery => _publishedQuery ??=
        _umbracoDatabaseFactory.SqlContext.Query<IContent>().Where(x => x.Published);

    /// <summary>
    /// Determines whether the specified content index is registered for population.
    /// </summary>
    /// <param name="index">The content index to check.</param>
    /// <returns><c>true</c> if the content index is registered; otherwise, <c>false</c>.</returns>
    public override bool IsRegistered(IUmbracoContentIndex index) =>

        // check if it should populate based on published values
        _publishedValuesOnly == index.PublishedValuesOnly;

    protected override void PopulateIndexes(IReadOnlyList<IIndex> indexes)
    {
        if (indexes.Count == 0)
        {
            if (_logger.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Debug))
            {
                _logger.LogDebug($"{nameof(PopulateIndexes)} called with no indexes to populate. Typically means no index is registered with this populator.");
            }
            return;
        }

        var pageIndex = 0;

        var contentParentId = -1;
        if (_parentId.HasValue && _parentId.Value > 0)
        {
            contentParentId = _parentId.Value;
        }

        if (_publishedValuesOnly)
        {
            IndexPublishedContent(contentParentId, pageIndex, _indexingSettings.BatchSize, indexes);
        }
        else
        {
            IndexAllContent(contentParentId, pageIndex, _indexingSettings.BatchSize, indexes);
        }
    }

    protected void IndexAllContent(int contentParentId, int pageIndex, int pageSize, IReadOnlyList<IIndex> indexes)
    {
        IContent[] content;

        do
        {
            content = _contentService.GetPagedDescendants(contentParentId, pageIndex, pageSize, out _).ToArray();

            var valueSets = _contentValueSetBuilder.GetValueSets(content).ToArray();

            // ReSharper disable once PossibleMultipleEnumeration
            foreach (IIndex index in indexes)
            {
                index.IndexItems(valueSets);
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

            var valueSets = _contentValueSetBuilder.GetValueSets(indexableContent.ToArray()).ToArray();

            foreach (IIndex index in indexes)
            {
                index.IndexItems(valueSets);
            }

            pageIndex++;
        }
        while (content.Length == pageSize);
    }
}
