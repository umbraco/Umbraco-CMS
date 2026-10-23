using Examine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
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
    private const int MaxPropertyAliasesToLoad = 1000;

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

        string[]? propertyAliases = GetPropertyAliasesToLoad(indexes);
        if (propertyAliases is not null && _logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("Loading only the properties the indexes include fields for: {PropertyAliases}", propertyAliases);
        }

        if (_publishedValuesOnly)
        {
            IndexPublishedContent(contentParentId, pageIndex, _indexingSettings.BatchSize, indexes, propertyAliases);
        }
        else
        {
            IndexAllContent(contentParentId, pageIndex, _indexingSettings.BatchSize, indexes, propertyAliases);
        }
    }

    /// <summary>
    ///     Gets the aliases of the properties to load when populating the given indexes.
    /// </summary>
    /// <param name="indexes">The indexes to populate.</param>
    /// <returns>The aliases of the properties to load, or <c>null</c> to load all properties.</returns>
    /// <remarks>
    ///     By default all properties are loaded. When <see cref="IndexingSettings.LoadOnlyPropertiesForIncludedFields" />
    ///     is enabled and every index validates with a <see cref="ValueSetValidator" /> that only includes specific
    ///     fields, only the properties that can produce those fields are loaded. They are worked out from the field
    ///     names, assuming each is the property alias, optionally prefixed with
    ///     <see cref="UmbracoExamineFieldNames.RawFieldPrefix" /> and followed by a nested value path or a culture suffix.
    ///     Override this to load the properties needed for fields named differently.
    /// </remarks>
    protected virtual string[]? GetPropertyAliasesToLoad(IReadOnlyList<IIndex> indexes)
        => _indexingSettings.LoadOnlyPropertiesForIncludedFields
            ? GetPropertyAliasesForIncludedFields(indexes)
            : null;

    /// <summary>
    ///     Gets the aliases of the properties that can produce the fields the given indexes include.
    /// </summary>
    /// <param name="indexes">The indexes to populate.</param>
    /// <returns>
    ///     The aliases of the properties that may produce a field the indexes include, or <c>null</c> if all properties
    ///     are needed. All properties are needed unless every index validates its value sets with a
    ///     <see cref="ValueSetValidator" /> that only includes specific fields.
    /// </returns>
    /// <remarks>
    ///     <para>
    ///         The validator removes any field it doesn't include before the index, or any handler transforming its
    ///         values, sees the value set, so a property that cannot produce an included field doesn't need to be loaded.
    ///     </para>
    ///     <para>
    ///         The aliases are worked out from the included field names, assuming each property field is named by the
    ///         property alias, optionally prefixed with <see cref="UmbracoExamineFieldNames.RawFieldPrefix" /> and
    ///         followed by a nested value path or a culture suffix, as the built-in property index value factories name
    ///         them. They may also contain names that are not property aliases.
    ///     </para>
    /// </remarks>
    internal static string[]? GetPropertyAliasesForIncludedFields(IEnumerable<IIndex> indexes)
    {
        var propertyAliases = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            // URL segment providers may read this when building the URL name fields.
            Constants.Conventions.Content.UrlName,
        };

        foreach (IIndex index in indexes)
        {
            if (index is not BaseIndexProvider { ValueSetValidator: ValueSetValidator { IncludeFields: { } includeFields } })
            {
                return null;
            }

            foreach (var fieldName in includeFields)
            {
                AddPropertyAliases(fieldName, propertyAliases);
            }
        }

        // Each alias is a parameter of the property data query, which must stay well within the database parameter limit.
        return propertyAliases.Count > MaxPropertyAliasesToLoad ? null : propertyAliases.ToArray();
    }

    /// <summary>
    ///     Adds the property aliases that may produce a field with the given name. Property fields are named by the
    ///     property alias, optionally prefixed for raw values, followed by the path of a nested value and the culture.
    /// </summary>
    private static void AddPropertyAliases(string fieldName, HashSet<string> propertyAliases)
    {
        var name = fieldName.StartsWith(UmbracoExamineFieldNames.RawFieldPrefix, StringComparison.Ordinal)
            ? fieldName[UmbracoExamineFieldNames.RawFieldPrefix.Length..]
            : fieldName;

        var nestedPathIndex = name.IndexOf('.');
        if (nestedPathIndex > 0)
        {
            name = name[..nestedPathIndex];
        }

        propertyAliases.Add(name);

        // The name may end with a culture suffix, or the alias itself may contain an underscore.
        var cultureSuffixIndex = name.LastIndexOf('_');
        if (cultureSuffixIndex > 0)
        {
            propertyAliases.Add(name[..cultureSuffixIndex]);
        }
    }

    [Obsolete("Please use the overload taking the property aliases to load. Scheduled for removal in Umbraco 19.")]
    protected void IndexAllContent(int contentParentId, int pageIndex, int pageSize, IReadOnlyList<IIndex> indexes)
        => IndexAllContent(contentParentId, pageIndex, pageSize, indexes, propertyAliases: null);

    /// <summary>
    ///     Indexes all content below the given parent.
    /// </summary>
    /// <param name="contentParentId">The identifier of the parent of the content to index.</param>
    /// <param name="pageIndex">The index of the first page of content to index.</param>
    /// <param name="pageSize">The number of content items to index at a time.</param>
    /// <param name="indexes">The indexes to populate.</param>
    /// <param name="propertyAliases">The aliases of the properties to load, or <c>null</c> to load all properties.</param>
    protected void IndexAllContent(int contentParentId, int pageIndex, int pageSize, IReadOnlyList<IIndex> indexes, string[]? propertyAliases)
    {
        IContent[] content;

        do
        {
            content = _contentService.GetPagedDescendants(contentParentId, pageIndex, pageSize, out _, propertyAliases, filter: null, ordering: null).ToArray();

            ValueSetIndexer.IndexItems(indexes, _contentValueSetBuilder.GetValueSets(content));

            pageIndex++;
        }
        while (content.Length == pageSize);
    }

    [Obsolete("Please use the overload taking the property aliases to load. Scheduled for removal in Umbraco 19.")]
    protected void IndexPublishedContent(int contentParentId, int pageIndex, int pageSize, IReadOnlyList<IIndex> indexes)
        => IndexPublishedContent(contentParentId, pageIndex, pageSize, indexes, propertyAliases: null);

    /// <summary>
    ///     Indexes the published content below the given parent, excluding content below unpublished content.
    /// </summary>
    /// <param name="contentParentId">The identifier of the parent of the content to index.</param>
    /// <param name="pageIndex">The index of the first page of content to index.</param>
    /// <param name="pageSize">The number of content items to index at a time.</param>
    /// <param name="indexes">The indexes to populate.</param>
    /// <param name="propertyAliases">The aliases of the properties to load, or <c>null</c> to load all properties.</param>
    protected void IndexPublishedContent(int contentParentId, int pageIndex, int pageSize, IReadOnlyList<IIndex> indexes, string[]? propertyAliases)
    {
        IContent[] content;

        var publishedPages = new HashSet<int>();

        do
        {
            // add the published filter
            // note: We will filter for published variants in the validator
            content = _contentService.GetPagedDescendants(contentParentId, pageIndex, pageSize, out _, propertyAliases, PublishedQuery, Ordering.By("Path")).ToArray();

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

            ValueSetIndexer.IndexItems(indexes, _contentValueSetBuilder.GetValueSets(indexableContent.ToArray()));

            pageIndex++;
        }
        while (content.Length == pageSize);
    }
}
