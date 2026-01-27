using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services.Navigation;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Services;

/// <summary>
/// Implements <see href="IDocumentUrlService" /> operations for handling document URLs.
/// </summary>
public class DocumentUrlService : IDocumentUrlService
{
    /// <summary>
    /// Represents the key used to identify the URL generation rebuild operation.
    /// </summary>
    public const string RebuildKey = "UmbracoUrlGeneration";

    private readonly ILogger<DocumentUrlService> _logger;
    private readonly IDocumentUrlRepository _documentUrlRepository;
    private readonly IDocumentRepository _documentRepository;
    private readonly ICoreScopeProvider _coreScopeProvider;
    private readonly GlobalSettings _globalSettings;
    private readonly WebRoutingSettings _webRoutingSettings;
    private readonly UrlSegmentProviderCollection _urlSegmentProviderCollection;
    private readonly IContentService _contentService;
    private readonly IShortStringHelper _shortStringHelper;
    private readonly ILanguageService _languageService;
    private readonly IKeyValueService _keyValueService;
    private readonly IIdKeyMap _idKeyMap;
    private readonly IDocumentNavigationQueryService _documentNavigationQueryService;
    private readonly IPublishStatusQueryService _publishStatusQueryService;
    private readonly IDomainCacheService _domainCacheService;
    private readonly IDefaultCultureAccessor _defaultCultureAccessor;

    private readonly ConcurrentDictionary<UrlCacheKey, UrlSegmentCache> _documentUrlCache = new();
    private readonly ConcurrentDictionary<string, int> _cultureToLanguageIdMap = new();
    private bool _isInitialized;

    /// <inheritdoc/>
    public bool IsInitialized => _isInitialized;

    /// <summary>
    /// Struct-based cache key for memory-efficient URL segment caching.
    /// Uses LanguageId instead of culture string to reduce memory footprint.
    /// </summary>
    /// <remarks>Internal for the purpose of unit and benchmark testing.</remarks>
    internal readonly struct UrlCacheKey : IEquatable<UrlCacheKey>
    {
        /// <summary>
        /// Gets the document key.
        /// </summary>
        public Guid DocumentKey { get; }

        /// <summary>
        /// Gets the language Id. NULL indicates invariant content (not language-specific).
        /// </summary>
        public int? LanguageId { get; }

        /// <summary>
        /// Gets a value indicating whether the URL is for a draft or published version.
        /// </summary>
        public bool IsDraft { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="UrlCacheKey"/> struct.
        /// </summary>
        /// <param name="documentKey">The document key.</param>
        /// <param name="languageId">The language Id. NULL for invariant content.</param>
        /// <param name="isDraft">A value indicating the draft or published value.</param>
        public UrlCacheKey(Guid documentKey, int? languageId, bool isDraft)
        {
            DocumentKey = documentKey;
            LanguageId = languageId;
            IsDraft = isDraft;
        }

        /// <inheritdoc/>
        public bool Equals(UrlCacheKey other) =>
            DocumentKey == other.DocumentKey &&
            LanguageId == other.LanguageId &&
            IsDraft == other.IsDraft;

        /// <inheritdoc/>
        public override bool Equals(object? obj) => obj is UrlCacheKey other && Equals(other);

        /// <inheritdoc/>
        public override int GetHashCode() => HashCode.Combine(DocumentKey, LanguageId ?? 0, IsDraft);
    }

    /// <summary>
    /// Optimized cache entry for URL segments. Primary segment stored directly,
    /// alternate segments only allocated when needed (as the common case is a single segment).
    /// </summary>
    /// <remarks>Internal for the purpose of unit and benchmark testing.</remarks>
    internal sealed class UrlSegmentCache
    {
        /// <summary>
        /// Gets the primary URL segment (always present).
        /// </summary>
        public required string PrimarySegment { get; init; }

        /// <summary>
        /// Gets additional URL segments, or null when only primary exists (common case).
        /// </summary>
        public string[]? AlternateSegments { get; init; }

        /// <summary>
        /// Gets all URL segments, primary first.
        /// </summary>
        public IEnumerable<string> GetAllSegments()
        {
            yield return PrimarySegment;
            if (AlternateSegments is not null)
            {
                foreach (var segment in AlternateSegments)
                {
                    yield return segment;
                }
            }
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DocumentUrlService"/> class.
    /// </summary>
    [Obsolete("Please use the constructor taking all parameters. Scheduled for removal in Umbraco 19.")]
    public DocumentUrlService(
        ILogger<DocumentUrlService> logger,
        IDocumentUrlRepository documentUrlRepository,
        IDocumentRepository documentRepository,
        ICoreScopeProvider coreScopeProvider,
        IOptions<GlobalSettings> globalSettings,
        UrlSegmentProviderCollection urlSegmentProviderCollection,
        IContentService contentService,
        IShortStringHelper shortStringHelper,
        ILanguageService languageService,
        IKeyValueService keyValueService,
        IIdKeyMap idKeyMap,
        IDocumentNavigationQueryService documentNavigationQueryService,
        IPublishStatusQueryService publishStatusQueryService,
        IDomainCacheService domainCacheService)
        :this(
            logger,
            documentUrlRepository,
            documentRepository,
            coreScopeProvider,
            globalSettings,
            StaticServiceProvider.Instance.GetRequiredService<IOptions<WebRoutingSettings>>(),
            urlSegmentProviderCollection,
            contentService,
            shortStringHelper,
            languageService,
            keyValueService,
            idKeyMap,
            documentNavigationQueryService,
            publishStatusQueryService,
            domainCacheService,
            StaticServiceProvider.Instance.GetRequiredService<IDefaultCultureAccessor>())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DocumentUrlService"/> class.
    /// </summary>
    public DocumentUrlService(
        ILogger<DocumentUrlService> logger,
        IDocumentUrlRepository documentUrlRepository,
        IDocumentRepository documentRepository,
        ICoreScopeProvider coreScopeProvider,
        IOptions<GlobalSettings> globalSettings,
        IOptions<WebRoutingSettings> webRoutingSettings,
        UrlSegmentProviderCollection urlSegmentProviderCollection,
        IContentService contentService,
        IShortStringHelper shortStringHelper,
        ILanguageService languageService,
        IKeyValueService keyValueService,
        IIdKeyMap idKeyMap,
        IDocumentNavigationQueryService documentNavigationQueryService,
        IPublishStatusQueryService publishStatusQueryService,
        IDomainCacheService domainCacheService,
        IDefaultCultureAccessor defaultCultureAccessor)
    {
        _logger = logger;
        _documentUrlRepository = documentUrlRepository;
        _documentRepository = documentRepository;
        _coreScopeProvider = coreScopeProvider;
        _globalSettings = globalSettings.Value;
        _webRoutingSettings = webRoutingSettings.Value;
        _urlSegmentProviderCollection = urlSegmentProviderCollection;
        _contentService = contentService;
        _shortStringHelper = shortStringHelper;
        _languageService = languageService;
        _keyValueService = keyValueService;
        _idKeyMap = idKeyMap;
        _documentNavigationQueryService = documentNavigationQueryService;
        _publishStatusQueryService = publishStatusQueryService;
        _domainCacheService = domainCacheService;
        _defaultCultureAccessor = defaultCultureAccessor;
    }

    /// <inheritdoc/>
    public async Task InitAsync(bool forceEmpty, CancellationToken cancellationToken)
    {
        if (forceEmpty)
        {
            // We have this use case when umbraco is installing, we know there is no routes. And we can execute the normal logic because the connection string is missing.
            _isInitialized = true;
            return;
        }

        using ICoreScope scope = _coreScopeProvider.CreateCoreScope();
        if (ShouldRebuildUrls())
        {
            _logger.LogInformation("Rebuilding all document URLs.");
            await RebuildAllUrlsAsync();
        }

        _logger.LogInformation("Caching document URLs.");

        IEnumerable<PublishedDocumentUrlSegment> publishedDocumentUrlSegments = _documentUrlRepository.GetAll();

        IEnumerable<ILanguage> languages = await _languageService.GetAllAsync();

        // Populate culture-to-languageId lookup for efficient cache key creation.
        PopulateCultureToLanguageIdMap(languages);

        int numberOfCachedUrls = 0;
        foreach ((UrlCacheKey cacheKey, UrlSegmentCache cache) in ConvertToCacheModel(publishedDocumentUrlSegments))
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            UpdateCache(_coreScopeProvider.Context!, cacheKey, cache);
            numberOfCachedUrls++;
        }

        _logger.LogInformation("Cached {NumberOfUrls} document URLs.", numberOfCachedUrls);

        if (_logger.IsEnabled(LogLevel.Debug))
        {
            var estimatedMemoryBytes = CalculateCacheMemoryUsage();
            _logger.LogDebug(
                "Document URL cache estimated memory usage: {MemoryUsageMB:F2} MB ({MemoryUsageBytes:N0} bytes).",
                estimatedMemoryBytes / (1024.0 * 1024.0),
                estimatedMemoryBytes);
        }

        _isInitialized = true;
        scope.Complete();
    }

    private bool ShouldRebuildUrls()
    {
        var persistedValue = GetPersistedRebuildValue();
        var currentValue = GetCurrentRebuildValue();

        return string.Equals(persistedValue, currentValue) is false;
    }

    private string? GetPersistedRebuildValue() => _keyValueService.GetValue(RebuildKey);

    private string GetCurrentRebuildValue() => string.Join("|", _urlSegmentProviderCollection.Select(x => x.GetType().Name));

    /// <inheritdoc/>
    public async Task RebuildAllUrlsAsync()
    {
        using ICoreScope scope = _coreScopeProvider.CreateCoreScope();
        scope.ReadLock(Constants.Locks.ContentTree);

        IEnumerable<IContent> documents = _documentRepository.GetMany(Array.Empty<int>());

        await CreateOrUpdateUrlSegmentsAsync(documents);

        _keyValueService.SetValue(RebuildKey, GetCurrentRebuildValue());

        scope.Complete();
    }

    /// <summary>
    /// Converts a collection of <see cref="PublishedDocumentUrlSegment"/> to cache key-value pairs for caching purposes.
    /// </summary>
    /// <param name="publishedDocumentUrlSegments">The collection of <see cref="PublishedDocumentUrlSegment"/> retrieved from the database on startup.</param>
    /// <returns>The collection of cache key-value pairs.</returns>
    /// <remarks>Internal for the purpose of unit and benchmark testing.</remarks>
    internal static IEnumerable<(UrlCacheKey Key, UrlSegmentCache Cache)> ConvertToCacheModel(IEnumerable<PublishedDocumentUrlSegment> publishedDocumentUrlSegments)
    {
        // Group segments by document/language/draft and collect primary + alternates.
        var grouped = new Dictionary<(Guid DocumentKey, int? LanguageId, bool IsDraft), (string? Primary, List<string> Alternates)>();

        foreach (PublishedDocumentUrlSegment model in publishedDocumentUrlSegments)
        {
            // Group using composite key of document/language/draft.
            (Guid DocumentKey, int? LanguageId, bool IsDraft) key = (model.DocumentKey, model.LanguageId, model.IsDraft);

            if (grouped.TryGetValue(key, out (string? Primary, List<string> Alternates) segments) is false)
            {
                segments = (model.IsPrimary ? model.UrlSegment : null, new List<string>());
                grouped[key] = segments;
            }

            // Each segment is either added as the primrary setment or de-duplicated and added as an alternate.
            if (model.IsPrimary && segments.Primary is null)
            {
                grouped[key] = (model.UrlSegment, segments.Alternates);
            }
            else if (model.IsPrimary is false && segments.Alternates.Contains(model.UrlSegment) is false)
            {
                segments.Alternates.Add(model.UrlSegment);
            }
        }

        // Prepare output as a collection of key value pairs of the cache key (UrlCacheKey struct) and cache entry (UrlSegmentCache object).
        foreach (KeyValuePair<(Guid DocumentKey, int? LanguageId, bool IsDraft), (string? Primary, List<string> Alternates)> kvp in grouped)
        {
            (Guid documentKey, int? languageId, bool isDraft) = kvp.Key;
            (string? primary, List<string>? alternates) = kvp.Value;

            // Use first alternate as primary if no primary was marked.
            var primarySegment = primary ?? alternates.FirstOrDefault();
            if (primarySegment is null)
            {
                continue; // Skip entries with no segments
            }

            // Remove primary from alternates if it was there.
            if (primary is null && alternates.Count > 0)
            {
                alternates.RemoveAt(0);
            }

            var cacheKey = new UrlCacheKey(documentKey, languageId, isDraft);
            var cache = new UrlSegmentCache
            {
                PrimarySegment = primarySegment,
                AlternateSegments = alternates.Count > 0 ? alternates.ToArray() : null,
            };

            yield return (cacheKey, cache);
        }
    }

    private void RemoveFromCache(IScopeContext scopeContext, Guid documentKey, int? languageId, bool isDraft)
    {
        UrlCacheKey cacheKey = CreateCacheKey(documentKey, languageId, isDraft);

        scopeContext.Enlist($"RemoveFromCache_{documentKey}_{languageId}_{isDraft}", () =>
        {
            if (_documentUrlCache.TryRemove(cacheKey, out _) is false)
            {
                _logger.LogDebug("Could not remove the document url cache. But the important thing is that it is not there.");
                return false;
            }

            return true;
        });
    }

    private void UpdateCache(IScopeContext scopeContext, UrlCacheKey cacheKey, UrlSegmentCache cache)
    {
        scopeContext.Enlist($"UpdateCache_{cacheKey.DocumentKey}_{cacheKey.LanguageId}_{cacheKey.IsDraft}", () =>
        {
            _documentUrlCache.AddOrUpdate(cacheKey, cache, (_, _) => cache);
            return true;
        });
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static UrlCacheKey CreateCacheKey(Guid documentKey, int? languageId, bool isDraft) => new(documentKey, languageId, isDraft);

    /// <summary>
    /// Calculates the estimated memory usage of the document URL cache.
    /// </summary>
    /// <returns>Estimated memory usage in bytes.</returns>
    private long CalculateCacheMemoryUsage()
    {
        // ConcurrentDictionary overhead (approximate).
        const int DictionaryBaseOverhead = 128;

        // Per-entry overhead in ConcurrentDictionary (node object + references).
        const int PerEntryDictionaryOverhead = 56;

        // UrlCacheKey struct size (Guid 16 + int 4 + bool 1 + padding 3 = 24 bytes).
        const int CacheKeySize = 24;

        // UrlSegmentCache object overhead (object header + references).
        const int CacheValueObjectOverhead = 24;

        // String object overhead (object header + length field + null terminator padding).
        const int StringObjectOverhead = 26;

        // Bytes per character in .NET strings.
        const int BytesPerChar = 2;

        // Array object overhead.
        const int ArrayObjectOverhead = 24;

        long totalBytes = DictionaryBaseOverhead;

        foreach (KeyValuePair<UrlCacheKey, UrlSegmentCache> kvp in _documentUrlCache)
        {
            // Key size (struct, stored inline in dictionary node)
            totalBytes += CacheKeySize;

            // Per-entry dictionary overhead
            totalBytes += PerEntryDictionaryOverhead;

            // Cache value object overhead
            totalBytes += CacheValueObjectOverhead;

            // Primary segment string
            totalBytes += StringObjectOverhead + (kvp.Value.PrimarySegment.Length * BytesPerChar);

            // Alternate segments array and strings
            if (kvp.Value.AlternateSegments is not null)
            {
                totalBytes += ArrayObjectOverhead + (kvp.Value.AlternateSegments.Length * IntPtr.Size);
                foreach (var segment in kvp.Value.AlternateSegments)
                {
                    totalBytes += StringObjectOverhead + (segment.Length * BytesPerChar);
                }
            }
        }

        return totalBytes;
    }

    /// <inheritdoc/>
    public string? GetUrlSegment(Guid documentKey, string culture, bool isDraft)
    {
        ThrowIfNotInitialized();
        if (TryGetLanguageIdFromCulture(culture, out int languageId) is false)
        {
            return null;
        }

        // Try culture-specific lookup first
        UrlCacheKey cacheKey = CreateCacheKey(documentKey, languageId, isDraft);
        if (_documentUrlCache.TryGetValue(cacheKey, out UrlSegmentCache? cache))
        {
            return cache.PrimarySegment;
        }

        // Try invariant lookup (NULL languageId) - for invariant content that stores with NULL
        UrlCacheKey invariantKey = CreateCacheKey(documentKey, null, isDraft);
        return _documentUrlCache.TryGetValue(invariantKey, out cache) ? cache.PrimarySegment : null;
    }

    private bool TryGetLanguageIdFromCulture(string culture, out int languageId)
    {
        if (_cultureToLanguageIdMap.TryGetValue(culture, out languageId))
        {
            return true;
        }

        // Not found in map, try to retrieve from language service. Although we are already updating after initialization in CreateOrUpdateUrlSegmentsAsync,
        // this ensures there's no chance of missing existing languages if this method is called with a newly introduced culture.
        ILanguage? language = _languageService.GetAsync(culture).GetAwaiter().GetResult();
        if (language is not null)
        {
            _cultureToLanguageIdMap[culture] = language.Id;
            languageId = language.Id;
            return true;
        }

        return false;
    }

    /// <inheritdoc/>
    public IEnumerable<string> GetUrlSegments(Guid documentKey, string culture, bool isDraft)
    {
        ThrowIfNotInitialized();
        if (TryGetLanguageIdFromCulture(culture, out int languageId) is false)
        {
            return Enumerable.Empty<string>();
        }

        // Try culture-specific lookup first
        UrlCacheKey cacheKey = CreateCacheKey(documentKey, languageId, isDraft);
        if (_documentUrlCache.TryGetValue(cacheKey, out UrlSegmentCache? cache))
        {
            return cache.GetAllSegments();
        }

        // Try invariant lookup (NULL languageId) - for invariant content that stores with NULL
        UrlCacheKey invariantKey = CreateCacheKey(documentKey, null, isDraft);
        return _documentUrlCache.TryGetValue(invariantKey, out cache)
            ? cache.GetAllSegments()
            : Enumerable.Empty<string>();
    }

    private void ThrowIfNotInitialized()
    {
        if (_isInitialized is false)
        {
            throw new InvalidOperationException("The service needs to be initialized before it can be used.");
        }
    }

    /// <inheritdoc/>
    public async Task CreateOrUpdateUrlSegmentsAsync(Guid key)
    {
        IContent? content = _contentService.GetById(key);

        if (content is not null)
        {
            await CreateOrUpdateUrlSegmentsAsync(content.Yield());
        }
    }

    /// <inheritdoc/>
    public async Task CreateOrUpdateUrlSegmentsWithDescendantsAsync(Guid key)
    {
        var id = _idKeyMap.GetIdForKey(key, UmbracoObjectTypes.Document).Result;
        IContent? item = _contentService.GetById(id);
        if (item is null)
        {
            _logger.LogDebug("Skipping URL segment rebuild for document with key {DocumentKey} was not found.", key);
            return;
        }

        IEnumerable<IContent> descendants = _contentService.GetPagedDescendants(id, 0, int.MaxValue, out _);

        await CreateOrUpdateUrlSegmentsAsync(new List<IContent>(descendants)
        {
            item,
        });
    }

    /// <inheritdoc/>
    public async Task CreateOrUpdateUrlSegmentsAsync(IEnumerable<IContent> documentsEnumerable)
    {
        IEnumerable<IContent> documents = documentsEnumerable as IContent[] ?? documentsEnumerable.ToArray();
        if (documents.Any() is false)
        {
            return;
        }

        using ICoreScope scope = _coreScopeProvider.CreateCoreScope();

        var toSave = new List<PublishedDocumentUrlSegment>();

        IEnumerable<ILanguage> languages = await _languageService.GetAllAsync();

        // Update culture-to-languageId map with any new languages created after InitAsync.
        PopulateCultureToLanguageIdMap(languages);

        var languageDictionary = languages.ToDictionary(x => x.IsoCode);

        foreach (IContent document in documents)
        {
            if (_logger.IsEnabled(LogLevel.Trace))
            {
                _logger.LogTrace("Rebuilding urls for document with key {DocumentKey}", document.Key);
            }

            if (document.ContentType.VariesByCulture())
            {
                // Variant content: process per language
                foreach ((string culture, ILanguage language) in languageDictionary)
                {
                    HandleCaching(_coreScopeProvider.Context!, document, culture, language.Id, toSave);
                }
            }
            else
            {
                // Invariant content: process once with NULL languageId
                HandleCaching(_coreScopeProvider.Context!, document, null, null, toSave);
            }
        }

        if (toSave.Count > 0)
        {
            scope.WriteLock(Constants.Locks.DocumentUrls);
            _documentUrlRepository.Save(toSave);
        }

        scope.Complete();
    }

    private void PopulateCultureToLanguageIdMap(IEnumerable<ILanguage> languages)
    {
        foreach (ILanguage language in languages)
        {
            _cultureToLanguageIdMap[language.IsoCode] = language.Id;
        }
    }

    private void HandleCaching(IScopeContext scopeContext, IContent document, string? culture, int? languageId, List<PublishedDocumentUrlSegment> toSave)
    {
        foreach ((UrlCacheKey cacheKey, UrlSegmentCache? cache, bool shouldCache) in GenerateCacheEntries(document, culture, languageId))
        {
            if (shouldCache is false || cache is null)
            {
                RemoveFromCache(scopeContext, cacheKey.DocumentKey, cacheKey.LanguageId, cacheKey.IsDraft);
            }
            else
            {
                toSave.AddRange(ConvertToPersistedModel(cacheKey, cache));
                UpdateCache(scopeContext, cacheKey, cache);
            }
        }
    }

    private IEnumerable<(UrlCacheKey CacheKey, UrlSegmentCache? Cache, bool ShouldCache)> GenerateCacheEntries(IContent document, string? culture, int? languageId)
    {
        // Published version
        if (document.Trashed is false
            && (IsInvariantAndPublished(document) || IsVariantAndPublishedForCulture(document, culture)))
        {
            string[] publishedUrlSegments = document.GetUrlSegments(_shortStringHelper, _urlSegmentProviderCollection, culture).ToArray();
            if (publishedUrlSegments.Length == 0)
            {
                _logger.LogWarning("No published URL segments found for document {DocumentKey} in culture {Culture}", document.Key, culture ?? "{null}");
            }
            else
            {
                var cacheKey = new UrlCacheKey(document.Key, languageId, isDraft: false);
                var cache = new UrlSegmentCache
                {
                    PrimarySegment = publishedUrlSegments[0],
                    AlternateSegments = publishedUrlSegments.Length > 1 ? publishedUrlSegments[1..] : null
                };
                yield return (cacheKey, cache, true);
            }
        }
        else
        {
            var cacheKey = new UrlCacheKey(document.Key, languageId, isDraft: false);
            yield return (cacheKey, null, false);
        }

        // Draft version
        string[] draftUrlSegments = document.GetUrlSegments(_shortStringHelper, _urlSegmentProviderCollection, culture, false).ToArray();

        if (draftUrlSegments.Length == 0)
        {
            // Log at debug level because this is expected when a document is not published in a given language.
            _logger.LogDebug("No draft URL segments found for document {DocumentKey} in culture {Culture}", document.Key, culture ?? "{null}");
        }
        else
        {
            var cacheKey = new UrlCacheKey(document.Key, languageId, isDraft: true);
            var cache = new UrlSegmentCache
            {
                PrimarySegment = draftUrlSegments[0],
                AlternateSegments = draftUrlSegments.Length > 1 ? draftUrlSegments[1..] : null
            };
            yield return (cacheKey, cache, document.Trashed is false);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsInvariantAndPublished(IContent document)
        => document.ContentType.VariesByCulture() is false  // Is Invariant
           && document.Published; // Is Published

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsVariantAndPublishedForCulture(IContent document, string? culture) =>
        document.PublishCultureInfos?.Values.Any(x => x.Culture == culture) ?? false;

    private static IEnumerable<PublishedDocumentUrlSegment> ConvertToPersistedModel(UrlCacheKey cacheKey, UrlSegmentCache cache)
    {
        // Primary segment
        yield return new PublishedDocumentUrlSegment
        {
            DocumentKey = cacheKey.DocumentKey,
            LanguageId = cacheKey.LanguageId,
            UrlSegment = cache.PrimarySegment,
            IsDraft = cacheKey.IsDraft,
            IsPrimary = true,
        };

        // Alternate segments
        if (cache.AlternateSegments is not null)
        {
            foreach (var segment in cache.AlternateSegments)
            {
                yield return new PublishedDocumentUrlSegment
                {
                    DocumentKey = cacheKey.DocumentKey,
                    LanguageId = cacheKey.LanguageId,
                    UrlSegment = segment,
                    IsDraft = cacheKey.IsDraft,
                    IsPrimary = false,
                };
            }
        }
    }

    /// <inheritdoc/>
    public async Task DeleteUrlsFromCacheAsync(IEnumerable<Guid> documentKeysEnumerable)
    {
        using ICoreScope scope = _coreScopeProvider.CreateCoreScope();

        IEnumerable<ILanguage> languages = await _languageService.GetAllAsync();

        IEnumerable<Guid> documentKeys = documentKeysEnumerable as Guid[] ?? documentKeysEnumerable.ToArray();

        foreach (Guid documentKey in documentKeys)
        {
            // Remove invariant entries (NULL languageId)
            RemoveFromCache(_coreScopeProvider.Context!, documentKey, null, true);
            RemoveFromCache(_coreScopeProvider.Context!, documentKey, null, false);

            // Remove language-specific entries
            foreach (ILanguage language in languages)
            {
                RemoveFromCache(_coreScopeProvider.Context!, documentKey, language.Id, true);
                RemoveFromCache(_coreScopeProvider.Context!, documentKey, language.Id, false);
            }
        }

        scope.Complete();
    }

    /// <inheritdoc/>
    public Guid? GetDocumentKeyByUri(Uri uri, bool isDraft)
    {
        IEnumerable<Domain> domains = _domainCacheService.GetAll(false);
        DomainAndUri? domain = DomainUtilities.SelectDomain(domains, uri, defaultCulture: _defaultCultureAccessor.DefaultCulture);

        string route;
        if (domain is not null)
        {
            route = domain.ContentId + DomainUtilities.PathRelativeToDomain(domain.Uri, uri.GetAbsolutePathDecoded());
        }
        else
        {
            // If we have configured strict domain matching, and a domain has not been found for the request configured on an ancestor node,
            // do not route the content by URL.
            if (_webRoutingSettings.UseStrictDomainMatching)
            {
                return null;
            }

            // Default behaviour if strict domain matching is not enabled will be to route under the to the first root node found.
            route = uri.GetAbsolutePathDecoded();
        }

        return GetDocumentKeyByRoute(
            domain is null ? route : route[domain.ContentId.ToString().Length..],
            domain?.Culture,
            domain?.ContentId,
            isDraft);
    }

    /// <inheritdoc/>
    public Guid? GetDocumentKeyByRoute(string route, string? culture, int? documentStartNodeId, bool isDraft)
    {
        var urlSegments = route.Split(Constants.CharArrays.ForwardSlash, StringSplitOptions.RemoveEmptyEntries);

        // We need to translate legacy int ids to guid keys.
        Guid? runnerKey = GetStartNodeKey(documentStartNodeId);
        var hideTopLevelNodeFromPath = _globalSettings.HideTopLevelNodeFromPath;

        culture ??= _languageService.GetDefaultIsoCodeAsync().GetAwaiter().GetResult();

        if (!_globalSettings.ForceCombineUrlPathLeftToRight
            && CultureInfo.GetCultureInfo(culture).TextInfo.IsRightToLeft)
        {
            urlSegments = urlSegments.Reverse().ToArray();
        }

        // If a domain is assigned to this route, we need to follow the url segments
        if (runnerKey.HasValue)
        {
            // if the domain node is unpublished, we need to return null.
            if (isDraft is false && IsContentPublished(runnerKey.Value, culture) is false)
            {
                return null;
            }

            // If there is no url segments it means the domain root has been requested
            if (urlSegments.Length == 0)
            {
                return runnerKey.Value;
            }

            // Otherwise we have to find the child with that segment anc follow that
            foreach (var urlSegment in urlSegments)
            {
                // Get the children of the runnerKey and find the child (if any) with the correct url segment
                IEnumerable<Guid> childKeys = GetChildKeys(runnerKey.Value);

                runnerKey = GetChildWithUrlSegment(childKeys, urlSegment, culture, isDraft);


                if (runnerKey is null)
                {
                    break;
                }

                // If part of the path is unpublished, we need to break
                if (isDraft is false && IsContentPublished(runnerKey.Value, culture) is false)
                {
                    return null;
                }
            }

            return runnerKey;
        }

        // If there is no parts, it means it is a root (and no assigned domain)
        if (urlSegments.Length == 0)
        {
            // // if we do not hide the top level and no domain was found, it mean there is no content.
            // // TODO we can remove this to keep consistency with the old routing, but it seems incorrect to allow that.
            // if (hideTopLevelNodeFromPath is false)
            // {
            //     return null;
            // }

            return GetTopMostRootKey(isDraft, culture);
        }

        // Special case for all top level nodes except the first (that will have /)
        if (runnerKey is null && urlSegments.Length == 1 && hideTopLevelNodeFromPath is true)
        {
            IEnumerable<Guid> rootKeys = GetKeysInRoot(false, isDraft, culture);
            Guid? rootKeyWithUrlSegment = GetChildWithUrlSegment(rootKeys, urlSegments.First(), culture, isDraft);

            if (rootKeyWithUrlSegment is not null)
            {
                return rootKeyWithUrlSegment;
            }
        }

        // Otherwise we have to find the root items (or child of the roots when hideTopLevelNodeFromPath is true) and follow the url segments in them to get to correct document key
        for (var index = 0; index < urlSegments.Length; index++)
        {
            var urlSegment = urlSegments[index];
            IEnumerable<Guid> runnerKeys;
            if (index == 0)
            {
                runnerKeys = GetKeysInRoot(hideTopLevelNodeFromPath, isDraft, culture);
            }
            else
            {
                if (runnerKey is null)
                {
                    break;
                }

                runnerKeys = GetChildKeys(runnerKey.Value);
            }

            runnerKey = GetChildWithUrlSegment(runnerKeys, urlSegment, culture, isDraft);
        }

        if (isDraft is false && runnerKey.HasValue && IsContentPublished(runnerKey.Value, culture) is false)
        {
            return null;
        }

        return runnerKey;
    }

    private Guid? GetStartNodeKey(int? documentStartNodeId)
    {
        if (documentStartNodeId is null)
        {
            return null;
        }

        Attempt<Guid> attempt = _idKeyMap.GetKeyForId(documentStartNodeId.Value, UmbracoObjectTypes.Document);
        return attempt.Success ? attempt.Result : null;
    }

    private bool IsContentPublished(Guid contentKey, string culture) => _publishStatusQueryService.IsDocumentPublished(contentKey, culture);

    /// <summary>
    /// Gets the children based on the latest published version of the content. (No aware of things in this scope).
    /// </summary>
    /// <param name="documentKey">The key of the document to get children from.</param>
    /// <returns>The keys of all the children of the document.</returns>
    private IEnumerable<Guid> GetChildKeys(Guid documentKey)
    {
        if (_documentNavigationQueryService.TryGetChildrenKeys(documentKey, out IEnumerable<Guid> childrenKeys))
        {
            return childrenKeys;
        }

        return [];
    }

    private Guid? GetChildWithUrlSegment(IEnumerable<Guid> childKeys, string urlSegment, string culture, bool isDraft)
    {
        foreach (Guid childKey in childKeys)
        {
            IEnumerable<string> childUrlSegments = GetUrlSegments(childKey, culture, isDraft);

            if (childUrlSegments.Contains(urlSegment))
            {
                return childKey;
            }
        }

        return null;
    }

    private Guid? GetTopMostRootKey(bool isDraft, string culture) => GetRootKeys(isDraft, culture).Cast<Guid?>().FirstOrDefault();

    private IEnumerable<Guid> GetRootKeys(bool isDraft, string culture)
    {
        if (_documentNavigationQueryService.TryGetRootKeys(out IEnumerable<Guid> rootKeys))
        {
            foreach (Guid rootKey in rootKeys)
            {
                if (isDraft || IsContentPublished(rootKey, culture))
                {
                    yield return rootKey;
                }
            }
        }
    }

    private IEnumerable<Guid> GetKeysInRoot(bool considerFirstLevelAsRoot, bool isDraft, string culture)
    {
        if (_documentNavigationQueryService.TryGetRootKeys(out IEnumerable<Guid> rootKeysEnumerable) is false)
        {
            yield break;
        }

        IEnumerable<Guid> rootKeys = rootKeysEnumerable as Guid[] ?? rootKeysEnumerable.ToArray();

        if (considerFirstLevelAsRoot)
        {
            foreach (Guid rootKey in rootKeys)
            {
                if (isDraft is false && IsContentPublished(rootKey, culture) is false)
                {
                    continue;
                }

                IEnumerable<Guid> childKeys = GetChildKeys(rootKey);

                foreach (Guid childKey in childKeys)
                {
                    yield return childKey;
                }
            }
        }
        else
        {
            foreach (Guid rootKey in rootKeys)
            {
                yield return rootKey;
            }
        }
    }

    /// <inheritdoc/>
    public string GetLegacyRouteFormat(Guid documentKey, string? culture, bool isDraft)
    {
        Attempt<int> documentIdAttempt = _idKeyMap.GetIdForKey(documentKey, UmbracoObjectTypes.Document);

        if (documentIdAttempt.Success is false)
        {
            return "#";
        }

        if (_documentNavigationQueryService.TryGetAncestorsOrSelfKeys(documentKey, out IEnumerable<Guid> ancestorsOrSelfKeys) is false)
        {
            return "#";
        }

        if (isDraft is false && string.IsNullOrWhiteSpace(culture) is false && _publishStatusQueryService.IsDocumentPublished(documentKey, culture) is false)
        {
            return "#";
        }

        string cultureOrDefault = GetCultureOrDefault(culture);

        Guid[] ancestorsOrSelfKeysArray = ancestorsOrSelfKeys as Guid[] ?? ancestorsOrSelfKeys.ToArray();
        ILookup<Guid, Domain?> ancestorOrSelfKeyToDomains = ancestorsOrSelfKeysArray.ToLookup(x => x, ancestorKey =>
        {
            Attempt<int> idAttempt = _idKeyMap.GetIdForKey(ancestorKey, UmbracoObjectTypes.Document);

            if (idAttempt.Success is false)
            {
                return null;
            }

            IEnumerable<Domain> domains = _domainCacheService.GetAssigned(idAttempt.Result, false);

            // If no culture is specified, we assume invariant and return the first domain.
            // This is also only used to later to specify the node id in the route, so it does not matter what culture it is.
            return GetDomainForCultureOrInvariant(domains, culture);
        });

        var urlSegments = new List<string>();

        Domain? foundDomain = null;

        foreach (Guid ancestorOrSelfKey in ancestorsOrSelfKeysArray)
        {
            IEnumerable<Domain> domains = ancestorOrSelfKeyToDomains[ancestorOrSelfKey].WhereNotNull();
            if (domains.Any())
            {
                foundDomain = domains.First();// What todo here that is better?
                break;
            }

            if (TryGetPrimaryUrlSegment(ancestorOrSelfKey, cultureOrDefault, isDraft, out string? segment))
            {
                urlSegments.Add(segment);
            }

            if (foundDomain is not null)
            {
                break;
            }
        }

        bool leftToRight = ArePathsLeftToRight(cultureOrDefault);
        if (leftToRight)
        {
            urlSegments.Reverse();
        }

        if (foundDomain is not null)
        {
            // We found a domain, and not to construct the route in the funny legacy way
            return foundDomain.ContentId + "/" + string.Join("/", urlSegments);
        }

        var isRootFirstItem = GetTopMostRootKey(isDraft, cultureOrDefault) == ancestorsOrSelfKeysArray.Last();
        return GetFullUrl(isRootFirstItem, urlSegments, null, leftToRight);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private string GetCultureOrDefault(string? culture)
        => string.IsNullOrWhiteSpace(culture) is false
            ? culture
            : _languageService.GetDefaultIsoCodeAsync().GetAwaiter().GetResult();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool ArePathsLeftToRight(string cultureOrDefault)
        => _globalSettings.ForceCombineUrlPathLeftToRight ||
            CultureInfo.GetCultureInfo(cultureOrDefault).TextInfo.IsRightToLeft is false;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Domain? GetDomainForCultureOrInvariant(IEnumerable<Domain> domains, string? culture)
        => string.IsNullOrEmpty(culture)
            ? domains.FirstOrDefault()
            : domains.FirstOrDefault(x => x.Culture?.Equals(culture, StringComparison.InvariantCultureIgnoreCase) ?? false);

    private string GetFullUrl(bool isRootFirstItem, List<string> segments, Domain? foundDomain, bool leftToRight)
    {
        var urlSegments = new List<string>(segments);

        if (foundDomain is not null)
        {
            return foundDomain.Name.EnsureEndsWith("/") + string.Join('/', urlSegments);
        }

        var hideTopLevel = HideTopLevel(_globalSettings.HideTopLevelNodeFromPath, isRootFirstItem, urlSegments);
        if (leftToRight)
        {
            return '/' + string.Join('/', urlSegments.Skip(hideTopLevel ? 1 : 0));
        }

        if (hideTopLevel)
        {
            urlSegments.RemoveAt(urlSegments.Count - 1);
        }

        return '/' + string.Join('/', urlSegments);
    }

    private static bool HideTopLevel(bool hideTopLevelNodeFromPath, bool isRootFirstItem, List<string> urlSegments)
    {
        if (hideTopLevelNodeFromPath is false)
        {
            return false;
        }

        if (isRootFirstItem is false && urlSegments.Count == 1)
        {
            return false;
        }

        return true;
    }

    /// <inheritdoc/>
    public bool HasAny()
    {
        ThrowIfNotInitialized();
        return _documentUrlCache.Any();
    }

    private bool TryGetPrimaryUrlSegment(Guid documentKey, string culture, bool isDraft, [NotNullWhen(true)] out string? segment)
    {
        if (TryGetLanguageIdFromCulture(culture, out int languageId))
        {
            // Try culture-specific lookup first
            if (_documentUrlCache.TryGetValue(CreateCacheKey(documentKey, languageId, isDraft), out UrlSegmentCache? cache))
            {
                segment = cache.PrimarySegment;
                return true;
            }

            // Try invariant lookup (NULL languageId) - for invariant content that stores with NULL
            if (_documentUrlCache.TryGetValue(CreateCacheKey(documentKey, null, isDraft), out cache))
            {
                segment = cache.PrimarySegment;
                return true;
            }
        }

        segment = null;
        return false;
    }
}
