using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
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
    private const string RebuildKey = "UmbracoUrlGeneration";

    private readonly ILogger<DocumentUrlService> _logger;
    private readonly IDocumentUrlRepository _documentUrlRepository;
    private readonly IDocumentRepository _documentRepository;
    private readonly ICoreScopeProvider _coreScopeProvider;
    private readonly GlobalSettings _globalSettings;
    private readonly UrlSegmentProviderCollection _urlSegmentProviderCollection;
    private readonly IContentService _contentService;
    private readonly IShortStringHelper _shortStringHelper;
    private readonly ILanguageService _languageService;
    private readonly IKeyValueService _keyValueService;
    private readonly IIdKeyMap _idKeyMap;
    private readonly IDocumentNavigationQueryService _documentNavigationQueryService;
    private readonly IPublishStatusQueryService _publishStatusQueryService;
    private readonly IDomainCacheService _domainCacheService;

    private readonly ConcurrentDictionary<string, PublishedDocumentUrlSegments> _cache = new();
    private bool _isInitialized;

    private class PublishedDocumentUrlSegments
    {
        public required Guid DocumentKey { get; set; }

        public required int LanguageId { get; set; }

        public required IList<UrlSegment> UrlSegments { get; set; }

        public required bool IsDraft { get; set; }

        public class UrlSegment
        {
            public UrlSegment(string segment, bool isPrimary)
            {
                Segment = segment;
                IsPrimary = isPrimary;
            }

            public string Segment { get; }

            public bool IsPrimary { get; }
        }
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
        UrlSegmentProviderCollection urlSegmentProviderCollection,
        IContentService contentService,
        IShortStringHelper shortStringHelper,
        ILanguageService languageService,
        IKeyValueService keyValueService,
        IIdKeyMap idKeyMap,
        IDocumentNavigationQueryService documentNavigationQueryService,
        IPublishStatusQueryService publishStatusQueryService,
        IDomainCacheService domainCacheService)
    {
        _logger = logger;
        _documentUrlRepository = documentUrlRepository;
        _documentRepository = documentRepository;
        _coreScopeProvider = coreScopeProvider;
        _globalSettings = globalSettings.Value;
        _urlSegmentProviderCollection = urlSegmentProviderCollection;
        _contentService = contentService;
        _shortStringHelper = shortStringHelper;
        _languageService = languageService;
        _keyValueService = keyValueService;
        _idKeyMap = idKeyMap;
        _documentNavigationQueryService = documentNavigationQueryService;
        _publishStatusQueryService = publishStatusQueryService;
        _domainCacheService = domainCacheService;
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
            _logger.LogInformation("Rebuilding all URLs.");
            await RebuildAllUrlsAsync();
        }

        IEnumerable<PublishedDocumentUrlSegment> publishedDocumentUrlSegments = _documentUrlRepository.GetAll();

        IEnumerable<ILanguage> languages = await _languageService.GetAllAsync();
        var languageIdToIsoCode = languages.ToDictionary(x => x.Id, x => x.IsoCode);
        foreach (PublishedDocumentUrlSegments publishedDocumentUrlSegment in ConvertToCacheModel(publishedDocumentUrlSegments))
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            if (languageIdToIsoCode.TryGetValue(publishedDocumentUrlSegment.LanguageId, out var isoCode))
            {
                UpdateCache(_coreScopeProvider.Context!, publishedDocumentUrlSegment, isoCode);
            }
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

    private static IEnumerable<PublishedDocumentUrlSegments> ConvertToCacheModel(IEnumerable<PublishedDocumentUrlSegment> publishedDocumentUrlSegments)
    {
        var cacheModels = new List<PublishedDocumentUrlSegments>();
        foreach (PublishedDocumentUrlSegment model in publishedDocumentUrlSegments)
        {
            PublishedDocumentUrlSegments? existingCacheModel = cacheModels
                .SingleOrDefault(x => x.DocumentKey == model.DocumentKey && x.LanguageId == model.LanguageId && x.IsDraft == model.IsDraft);
            if (existingCacheModel is null)
            {
                cacheModels.Add(new PublishedDocumentUrlSegments
                {
                    DocumentKey = model.DocumentKey,
                    LanguageId = model.LanguageId,
                    UrlSegments = [new PublishedDocumentUrlSegments.UrlSegment(model.UrlSegment, model.IsPrimary)],
                    IsDraft = model.IsDraft
                });
            }
            else
            {
                IList<PublishedDocumentUrlSegments.UrlSegment> urlSegments = existingCacheModel.UrlSegments;
                if (urlSegments.FirstOrDefault(x => x.Segment == model.UrlSegment) is null)
                {
                    urlSegments.Add(new PublishedDocumentUrlSegments.UrlSegment(model.UrlSegment, model.IsPrimary));
                }

                existingCacheModel.UrlSegments = urlSegments;
            }
        }

        return cacheModels;
    }

    private void RemoveFromCache(IScopeContext scopeContext, Guid documentKey, string isoCode, bool isDraft)
    {
        var cacheKey = CreateCacheKey(documentKey, isoCode, isDraft);

        scopeContext.Enlist("RemoveFromCache_" + cacheKey, () =>
        {
            if (_cache.TryRemove(cacheKey, out _) is false)
            {
                _logger.LogDebug("Could not remove the document url cache. But the important thing is that it is not there.");
                return false;
            }

            return true;
        });
    }

    private void UpdateCache(IScopeContext scopeContext, PublishedDocumentUrlSegments publishedDocumentUrlSegments, string isoCode)
    {
        var cacheKey = CreateCacheKey(publishedDocumentUrlSegments.DocumentKey, isoCode, publishedDocumentUrlSegments.IsDraft);

        scopeContext.Enlist("UpdateCache_" + cacheKey, () =>
        {
            _cache.TryGetValue(cacheKey, out PublishedDocumentUrlSegments? existingValue);

            if (existingValue is null)
            {
                if (_cache.TryAdd(cacheKey, publishedDocumentUrlSegments) is false)
                {
                    _logger.LogError("Could not add to the document url cache.");
                    return false;
                }
            }
            else
            {
                if (_cache.TryUpdate(cacheKey, publishedDocumentUrlSegments, existingValue) is false)
                {
                    _logger.LogError("Could not update the document url cache.");
                    return false;
                }
            }

            return true;
        });
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string CreateCacheKey(Guid documentKey, string culture, bool isDraft) => $"{documentKey}|{culture}|{isDraft}".ToLowerInvariant();

    /// <inheritdoc/>
    public string? GetUrlSegment(Guid documentKey, string culture, bool isDraft)
    {
        ThrowIfNotInitialized();
        var cacheKey = CreateCacheKey(documentKey, culture, isDraft);

        _cache.TryGetValue(cacheKey, out PublishedDocumentUrlSegments? urlSegment);

        return urlSegment?.UrlSegments.FirstOrDefault(x => x.IsPrimary)?.Segment;
    }

    /// <inheritdoc/>
    public IEnumerable<string> GetUrlSegments(Guid documentKey, string culture, bool isDraft)
    {
        ThrowIfNotInitialized();
        var cacheKey = CreateCacheKey(documentKey, culture, isDraft);

        _cache.TryGetValue(cacheKey, out PublishedDocumentUrlSegments? urlSegments);

        return urlSegments?.UrlSegments.Select(x => x.Segment) ?? Enumerable.Empty<string>();
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
        IContent item = _contentService.GetById(id)!;
        IEnumerable<IContent> descendants = _contentService.GetPagedDescendants(id, 0, int.MaxValue, out _);

        await CreateOrUpdateUrlSegmentsAsync(new List<IContent>(descendants)
        {
            item
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
        var languageDictionary = languages.ToDictionary(x => x.IsoCode);

        foreach (IContent document in documents)
        {
            if (_logger.IsEnabled(LogLevel.Trace))
            {
                _logger.LogTrace("Rebuilding urls for document with key {DocumentKey}", document.Key);
            }

            foreach ((string culture, ILanguage language) in languageDictionary)
            {
                HandleCaching(_coreScopeProvider.Context!, document, document.ContentType.VariesByCulture() ? culture : null, language, toSave);
            }
        }

        if (toSave.Count > 0)
        {
            _documentUrlRepository.Save(toSave);
        }

        scope.Complete();
    }

    private void HandleCaching(IScopeContext scopeContext, IContent document, string? culture, ILanguage language, List<PublishedDocumentUrlSegment> toSave)
    {
        IEnumerable<(PublishedDocumentUrlSegments model, bool shouldCache)> modelsAndStatus = GenerateModels(document, culture, language);

        foreach ((PublishedDocumentUrlSegments model, bool shouldCache) in modelsAndStatus)
        {
            if (shouldCache is false)
            {
                RemoveFromCache(scopeContext, model.DocumentKey, language.IsoCode, model.IsDraft);
            }
            else
            {
                toSave.AddRange(ConvertToPersistedModel(model));
                UpdateCache(scopeContext, model, language.IsoCode);
            }
        }
    }

    private IEnumerable<(PublishedDocumentUrlSegments model, bool shouldCache)> GenerateModels(IContent document, string? culture, ILanguage language)
    {
        if (document.Trashed is false
            && (IsInvariantAndPublished(document) || IsVariantAndPublishedForCulture(document, culture)))
        {
            IEnumerable<string> publishedUrlSegments = document.GetUrlSegments(_shortStringHelper, _urlSegmentProviderCollection, culture);
            if (publishedUrlSegments.Any() is false)
            {
                _logger.LogWarning("No published URL segments found for document {DocumentKey} in culture {Culture}", document.Key, culture ?? "{null}");
            }
            else
            {
                yield return (new PublishedDocumentUrlSegments
                {
                    DocumentKey = document.Key,
                    LanguageId = language.Id,
                    UrlSegments = publishedUrlSegments
                        .Select((x, i) => new PublishedDocumentUrlSegments.UrlSegment(x, i == 0))
                        .ToList(),
                    IsDraft = false
                }, true);
            }
        }
        else
        {
            yield return (new PublishedDocumentUrlSegments
            {
                DocumentKey = document.Key,
                LanguageId = language.Id,
                UrlSegments = [],
                IsDraft = false
            }, false);
        }

        IEnumerable<string> draftUrlSegments = document.GetUrlSegments(_shortStringHelper, _urlSegmentProviderCollection, culture, false);

        if (draftUrlSegments.Any() is false)
        {
            _logger.LogWarning("No draft URL segments found for document {DocumentKey} in culture {Culture}", document.Key, culture ?? "{null}");
        }
        else
        {
            yield return (new PublishedDocumentUrlSegments
            {
                DocumentKey = document.Key,
                LanguageId = language.Id,
                UrlSegments = draftUrlSegments
                    .Select((x, i) => new PublishedDocumentUrlSegments.UrlSegment(x, i == 0))
                    .ToList(),
                IsDraft = true
            }, document.Trashed is false);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsInvariantAndPublished(IContent document)
        => document.ContentType.VariesByCulture() is false  // Is Invariant
           && document.Published; // Is Published

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsVariantAndPublishedForCulture(IContent document, string? culture) =>
        document.PublishCultureInfos?.Values.Any(x => x.Culture == culture) ?? false;

    private IEnumerable<PublishedDocumentUrlSegment> ConvertToPersistedModel(PublishedDocumentUrlSegments model)
    {
        foreach (PublishedDocumentUrlSegments.UrlSegment urlSegment in model.UrlSegments)
        {
            yield return new PublishedDocumentUrlSegment
            {
                DocumentKey = model.DocumentKey,
                LanguageId = model.LanguageId,
                UrlSegment = urlSegment.Segment,
                IsDraft = model.IsDraft,
                IsPrimary = urlSegment.IsPrimary
            };
        }
    }

    /// <inheritdoc/>
    public async Task DeleteUrlsFromCacheAsync(IEnumerable<Guid> documentKeysEnumerable)
    {
        using ICoreScope scope = _coreScopeProvider.CreateCoreScope();

        IEnumerable<ILanguage> languages = await _languageService.GetAllAsync();

        IEnumerable<Guid> documentKeys = documentKeysEnumerable as Guid[] ?? documentKeysEnumerable.ToArray();

        foreach (ILanguage language in languages)
        {
            foreach (Guid documentKey in documentKeys)
            {
                RemoveFromCache(_coreScopeProvider.Context!, documentKey, language.IsoCode, true);
                RemoveFromCache(_coreScopeProvider.Context!, documentKey, language.IsoCode, false);
            }
        }

        scope.Complete();
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

        return Enumerable.Empty<Guid>();
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

        var cultureOrDefault = string.IsNullOrWhiteSpace(culture) is false ? culture : _languageService.GetDefaultIsoCodeAsync().GetAwaiter().GetResult();

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
            return string.IsNullOrEmpty(culture)
                ? domains.FirstOrDefault()
                : domains.FirstOrDefault(x => x.Culture?.Equals(culture, StringComparison.InvariantCultureIgnoreCase) ?? false);
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

            if (TryGetPrimaryUrlSegment(ancestorOrSelfKey, cultureOrDefault, isDraft, out string ? segment))
            {
                urlSegments.Add(segment);
            }

            if (foundDomain is not null)
            {
                break;
            }
        }

        var leftToRight = _globalSettings.ForceCombineUrlPathLeftToRight
                          || CultureInfo.GetCultureInfo(cultureOrDefault).TextInfo.IsRightToLeft is false;
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

    private bool HideTopLevel(bool hideTopLevelNodeFromPath, bool isRootFirstItem, List<string> urlSegments)
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
        return _cache.Any();
    }

    /// <inheritdoc/>
    [Obsolete("This method is obsolete and will be removed in future versions. Use IPublishedUrlInfoProvider.GetAllAsync instead.")]
    public async Task<IEnumerable<UrlInfo>> ListUrlsAsync(Guid contentKey)
    {
        var result = new List<UrlInfo>();

        Attempt<int> documentIdAttempt = _idKeyMap.GetIdForKey(contentKey, UmbracoObjectTypes.Document);

        if (documentIdAttempt.Success is false)
        {
            return result;
        }

        IEnumerable<Guid> ancestorsOrSelfKeys = contentKey.Yield()
            .Concat(_contentService.GetAncestors(documentIdAttempt.Result).Select(x => x.Key).Reverse());

        IEnumerable<ILanguage> languages = await _languageService.GetAllAsync();
        var cultures = languages.ToDictionary(x=>x.IsoCode);

        Guid[] ancestorsOrSelfKeysArray = ancestorsOrSelfKeys as Guid[] ?? ancestorsOrSelfKeys.ToArray();
        Dictionary<Guid, Task<ILookup<string, Domain>>> ancestorOrSelfKeyToDomains = ancestorsOrSelfKeysArray
            .ToDictionary(
                x => x,
                ancestorKey =>
                {
                    Attempt<int> idAttempt = _idKeyMap.GetIdForKey(ancestorKey, UmbracoObjectTypes.Document);

                    if (idAttempt.Success is false)
                    {
                        return Task.FromResult((ILookup<string, Domain>)null!);
                    }

                    IEnumerable<Domain> domains = _domainCacheService.GetAssigned(idAttempt.Result, false);
                    return Task.FromResult(domains.ToLookup(x => x.Culture!));
                })!;

        foreach ((string culture, ILanguage language) in cultures)
        {
            var urlSegments = new List<string>();
            var foundDomains = new List<Domain?>();

            var hasUrlInCulture = true;
            foreach (Guid ancestorOrSelfKey in ancestorsOrSelfKeysArray)
            {
                ILookup<string, Domain> domainLookup = await ancestorOrSelfKeyToDomains[ancestorOrSelfKey];
                if (domainLookup.Any())
                {
                    IEnumerable<Domain> domains = domainLookup[culture];
                    foreach (Domain domain in domains)
                    {
                        Attempt<Guid> domainKeyAttempt =
                            _idKeyMap.GetKeyForId(domain.ContentId, UmbracoObjectTypes.Document);
                        if (domainKeyAttempt.Success)
                        {
                            if (_publishStatusQueryService.IsDocumentPublished(domainKeyAttempt.Result, culture))
                            {
                                foundDomains.Add(domain);
                            }
                        }
                    }

                    if (foundDomains.Any())
                    {
                        break;
                    }
                }

                if (TryGetPrimaryUrlSegment(ancestorOrSelfKey, culture, false, out string? segment))
                {
                    urlSegments.Add(segment);
                }
                else
                {
                    hasUrlInCulture = false;
                }
            }

            // If we did not find a domain and this is not the default language, then the content is not routable
            if (foundDomains.Any() is false && language.IsDefault is false)
            {
                continue;
            }


            var isRootFirstItem = GetTopMostRootKey(false, culture) == ancestorsOrSelfKeysArray.Last();

            var leftToRight = _globalSettings.ForceCombineUrlPathLeftToRight
                              || CultureInfo.GetCultureInfo(culture).TextInfo.IsRightToLeft is false;
            if (leftToRight)
            {
                urlSegments.Reverse();
            }

            // If no domain was found, we need to add a null domain to the list to make sure we check for no domains.
            if (foundDomains.Any() is false)
            {
                foundDomains.Add(null);
            }

            foreach (Domain? foundDomain in foundDomains)
            {
                var foundUrl = GetFullUrl(isRootFirstItem, urlSegments, foundDomain, leftToRight);

                if (foundDomain is not null)
                {
                    // if we found a domain, it should be safe to show url
                    result.Add(new UrlInfo(
                        text: foundUrl,
                        isUrl: hasUrlInCulture,
                        culture: culture));
                }
                else
                {
                    // otherwise we need to ensure that no other page has the same url
                    // e.g. a site with two roots that both have a child with the same name
                    Guid? documentKeyByRoute = GetDocumentKeyByRoute(foundUrl, culture, foundDomain?.ContentId, false);
                    if (contentKey.Equals(documentKeyByRoute))
                    {
                        result.Add(new UrlInfo(
                            text: foundUrl,
                            isUrl: hasUrlInCulture,
                            culture: culture));
                    }
                    else
                    {
                        result.Add(new UrlInfo(
                            text: "Conflict: Other page has the same url",
                            isUrl: false,
                            culture: culture));
                    }
                }
            }
        }

        return result;
    }

    private bool TryGetPrimaryUrlSegment(Guid documentKey, string culture, bool isDraft, [NotNullWhen(true)] out string? segment)
    {
        if (_cache.TryGetValue(
            CreateCacheKey(documentKey, culture, isDraft),
            out PublishedDocumentUrlSegments? publishedDocumentUrlSegments))
        {
            PublishedDocumentUrlSegments.UrlSegment? primaryUrlSegment = publishedDocumentUrlSegments.UrlSegments.FirstOrDefault(x => x.IsPrimary);
            if (primaryUrlSegment is not null)
            {
                segment = primaryUrlSegment.Segment;
                return true;
            }
        }

        segment = null;
        return false;
    }
}
