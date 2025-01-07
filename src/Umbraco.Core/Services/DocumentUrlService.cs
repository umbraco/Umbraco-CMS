using System.Collections.Concurrent;
using System.Globalization;
using System.Runtime.CompilerServices;
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

    private readonly ConcurrentDictionary<string, PublishedDocumentUrlSegment> _cache = new();
    private bool _isInitialized;

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

    public async Task InitAsync(bool forceEmpty, CancellationToken cancellationToken)
    {
        if (forceEmpty)
        {
            // We have this use case when umbraco is installing, we know there is no routes. And we can execute the normal logic because the connection string is missing.
            _isInitialized = true;
            return;
        }

        using ICoreScope scope = _coreScopeProvider.CreateCoreScope();
        if (await ShouldRebuildUrlsAsync())
        {
			_logger.LogInformation("Rebuilding all urls.");
            await RebuildAllUrlsAsync();
        }

        IEnumerable<PublishedDocumentUrlSegment> publishedDocumentUrlSegments = _documentUrlRepository.GetAll();

        IEnumerable<ILanguage> languages = await _languageService.GetAllAsync();
        var languageIdToIsoCode = languages.ToDictionary(x => x.Id, x => x.IsoCode);
        foreach (PublishedDocumentUrlSegment publishedDocumentUrlSegment in publishedDocumentUrlSegments)
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

    private void UpdateCache(IScopeContext scopeContext, PublishedDocumentUrlSegment publishedDocumentUrlSegment, string isoCode)
    {
        var cacheKey = CreateCacheKey(publishedDocumentUrlSegment.DocumentKey, isoCode, publishedDocumentUrlSegment.IsDraft);

        scopeContext.Enlist("UpdateCache_" + cacheKey, () =>
        {
            _cache.TryGetValue(cacheKey, out PublishedDocumentUrlSegment? existingValue);

            if (existingValue is null)
            {
                if (_cache.TryAdd(cacheKey, publishedDocumentUrlSegment) is false)
                {
                    _logger.LogError("Could not add the document url cache.");
                    return false;
                }
            }
            else
            {
                if (_cache.TryUpdate(cacheKey, publishedDocumentUrlSegment, existingValue) is false)
                {
                    _logger.LogError("Could not update the document url cache.");
                    return false;
                }
            }

            return true;
        });


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

    public async Task RebuildAllUrlsAsync()
    {
        using ICoreScope scope = _coreScopeProvider.CreateCoreScope();
        scope.ReadLock(Constants.Locks.ContentTree);

        IEnumerable<IContent> documents = _documentRepository.GetMany(Array.Empty<int>());

        await CreateOrUpdateUrlSegmentsAsync(documents);

        _keyValueService.SetValue(RebuildKey, GetCurrentRebuildValue());

        scope.Complete();
    }

    private Task<bool> ShouldRebuildUrlsAsync()
    {
        var persistedValue = GetPersistedRebuildValue();
        var currentValue = GetCurrentRebuildValue();

        return Task.FromResult(string.Equals(persistedValue, currentValue) is false);
    }

    private string GetCurrentRebuildValue() => string.Join("|", _urlSegmentProviderCollection.Select(x => x.GetType().Name));

    private string? GetPersistedRebuildValue() => _keyValueService.GetValue(RebuildKey);

    public string? GetUrlSegment(Guid documentKey, string culture, bool isDraft)
    {
        ThrowIfNotInitialized();
        var cacheKey = CreateCacheKey(documentKey, culture, isDraft);

        _cache.TryGetValue(cacheKey, out PublishedDocumentUrlSegment? urlSegment);

        return urlSegment?.UrlSegment;
    }

    private void ThrowIfNotInitialized()
    {
        if (_isInitialized is false)
        {
            throw new InvalidOperationException("The service needs to be initialized before it can be used.");
        }
    }

    public async Task CreateOrUpdateUrlSegmentsAsync(IEnumerable<IContent> documentsEnumerable)
    {
        IEnumerable<IContent> documents = documentsEnumerable as IContent[] ?? documentsEnumerable.ToArray();
        if(documents.Any() is false)
        {
            return;
        }

        using ICoreScope scope = _coreScopeProvider.CreateCoreScope();

        var toSave = new List<PublishedDocumentUrlSegment>();

        IEnumerable<ILanguage> languages = await _languageService.GetAllAsync();
        var languageDictionary = languages.ToDictionary(x=>x.IsoCode);

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

        if(toSave.Any())
        {
            _documentUrlRepository.Save(toSave);
        }

        scope.Complete();
    }

    private void HandleCaching(IScopeContext scopeContext, IContent document, string? culture, ILanguage language, List<PublishedDocumentUrlSegment> toSave)
    {
        IEnumerable<(PublishedDocumentUrlSegment model, bool shouldCache)> modelsAndStatus = GenerateModels(document, culture, language);

        foreach ((PublishedDocumentUrlSegment model, bool shouldCache) in modelsAndStatus)
        {
            if (shouldCache is false)
            {
                RemoveFromCache(scopeContext, model.DocumentKey, language.IsoCode, model.IsDraft);
            }
            else
            {
                toSave.Add(model);
                UpdateCache(scopeContext, model, language.IsoCode);
            }
        }
    }

    private IEnumerable<(PublishedDocumentUrlSegment model, bool shouldCache)> GenerateModels(IContent document, string? culture, ILanguage language)
    {
        if (document.Trashed is false
            && (IsInvariantAndPublished(document) || IsVariantAndPublishedForCulture(document, culture)))
        {
            var publishedUrlSegment =
                document.GetUrlSegment(_shortStringHelper, _urlSegmentProviderCollection, culture);
            if (publishedUrlSegment.IsNullOrWhiteSpace())
            {
                _logger.LogWarning("No published url segment found for document {DocumentKey} in culture {Culture}", document.Key, culture ?? "{null}");
            }
            else
            {
                yield return (new PublishedDocumentUrlSegment()
                {
                    DocumentKey = document.Key,
                    LanguageId = language.Id,
                    UrlSegment = publishedUrlSegment,
                    IsDraft = false
                }, true);
            }
        }
        else
        {
            yield return (new PublishedDocumentUrlSegment()
            {
                DocumentKey = document.Key,
                LanguageId = language.Id,
                UrlSegment = string.Empty,
                IsDraft = false
            }, false);
        }

        var draftUrlSegment = document.GetUrlSegment(_shortStringHelper, _urlSegmentProviderCollection, culture, false);

        if(draftUrlSegment.IsNullOrWhiteSpace())
        {
            _logger.LogWarning("No draft url segment found for document {DocumentKey} in culture {Culture}", document.Key, culture ?? "{null}");
        }
        else
        {
            yield return (new PublishedDocumentUrlSegment()
            {
                DocumentKey = document.Key, LanguageId = language.Id, UrlSegment = draftUrlSegment, IsDraft = true
            }, document.Trashed is false);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsVariantAndPublishedForCulture(IContent document, string? culture) =>
        document.PublishCultureInfos?.Values.Any(x => x.Culture == culture) ?? false;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsInvariantAndPublished(IContent document)
        => document.ContentType.VariesByCulture() is false  // Is Invariant
           && document.Published; // Is Published

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
                //Get the children of the runnerKey and find the child (if any) with the correct url segment
                IEnumerable<Guid> childKeys = GetChildKeys(runnerKey.Value);

                runnerKey = GetChildWithUrlSegment(childKeys, urlSegment, culture, isDraft);


                if (runnerKey is null)
                {
                    break;
                }
                //if part of the path is unpublished, we need to break
                if (isDraft is false && IsContentPublished(runnerKey.Value, culture) is false)
                {
                    return null;
                }
            }

            return runnerKey;
        }
        // If there is no parts, it means it is a root (and no assigned domain)
        if(urlSegments.Length == 0)
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

    private bool IsContentPublished(Guid contentKey, string culture) => _publishStatusQueryService.IsDocumentPublished(contentKey, culture);

    public string GetLegacyRouteFormat(Guid documentKey, string? culture, bool isDraft)
    {
        Attempt<int> documentIdAttempt = _idKeyMap.GetIdForKey(documentKey, UmbracoObjectTypes.Document);

        if(documentIdAttempt.Success is false)
        {
            return "#";
        }

        if (_documentNavigationQueryService.TryGetAncestorsOrSelfKeys(documentKey, out IEnumerable<Guid> ancestorsOrSelfKeys) is false)
        {
            return "#";
        }

        if(isDraft is false && string.IsNullOrWhiteSpace(culture) is false && _publishStatusQueryService.IsDocumentPublished(documentKey, culture) is false)
        {
            return "#";
        }

        var cultureOrDefault = string.IsNullOrWhiteSpace(culture) is false ? culture : _languageService.GetDefaultIsoCodeAsync().GetAwaiter().GetResult();

        Guid[] ancestorsOrSelfKeysArray = ancestorsOrSelfKeys as Guid[] ?? ancestorsOrSelfKeys.ToArray();
        ILookup<Guid, Domain?> ancestorOrSelfKeyToDomains = ancestorsOrSelfKeysArray.ToLookup(x => x, ancestorKey =>
        {
            Attempt<int> idAttempt = _idKeyMap.GetIdForKey(ancestorKey, UmbracoObjectTypes.Document);

            if(idAttempt.Success is false)
            {
                return null;
            }

            IEnumerable<Domain> domains = _domainCacheService.GetAssigned(idAttempt.Result, false);
            return domains.FirstOrDefault(x=>x.Culture == cultureOrDefault);
        });

        var urlSegments = new List<string>();

        Domain? foundDomain = null;

        foreach (Guid ancestorOrSelfKey in ancestorsOrSelfKeysArray)
        {
            var domains = ancestorOrSelfKeyToDomains[ancestorOrSelfKey].WhereNotNull();
            if (domains.Any())
            {
                foundDomain = domains.First();// What todo here that is better?
                break;
            }

            if (_cache.TryGetValue(CreateCacheKey(ancestorOrSelfKey, cultureOrDefault, isDraft), out PublishedDocumentUrlSegment? publishedDocumentUrlSegment))
            {
                urlSegments.Add(publishedDocumentUrlSegment.UrlSegment);
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
            //we found a domain, and not to construct the route in the funny legacy way
            return foundDomain.ContentId + "/" + string.Join("/", urlSegments);
        }

        var isRootFirstItem = GetTopMostRootKey(isDraft, cultureOrDefault) == ancestorsOrSelfKeysArray.Last();
        return GetFullUrl(isRootFirstItem, urlSegments, null, leftToRight);
    }

    public bool HasAny()
    {
        ThrowIfNotInitialized();
        return _cache.Any();
    }


    public async Task<IEnumerable<UrlInfo>> ListUrlsAsync(Guid contentKey)
    {
        var result = new List<UrlInfo>();

        Attempt<int> documentIdAttempt = _idKeyMap.GetIdForKey(contentKey, UmbracoObjectTypes.Document);

        if(documentIdAttempt.Success is false)
        {
            return result;
        }

        IEnumerable<Guid> ancestorsOrSelfKeys = contentKey.Yield()
            .Concat(_contentService.GetAncestors(documentIdAttempt.Result).Select(x => x.Key).Reverse());

        IEnumerable<ILanguage> languages = await _languageService.GetAllAsync();
        var cultures = languages.ToDictionary(x=>x.IsoCode);

        Guid[] ancestorsOrSelfKeysArray = ancestorsOrSelfKeys as Guid[] ?? ancestorsOrSelfKeys.ToArray();
        Dictionary<Guid, Task<ILookup<string, Domain>>> ancestorOrSelfKeyToDomains = ancestorsOrSelfKeysArray.ToDictionary(x => x, async ancestorKey =>
        {
            Attempt<int> idAttempt = _idKeyMap.GetIdForKey(ancestorKey, UmbracoObjectTypes.Document);

            if(idAttempt.Success is false)
            {
                return null;
            }
            IEnumerable<Domain> domains = _domainCacheService.GetAssigned(idAttempt.Result, false);
            return domains.ToLookup(x => x.Culture!);
        })!;

        foreach ((string culture, ILanguage language) in cultures)
        {
            var urlSegments = new List<string>();
            List<Domain?> foundDomains = new List<Domain?>();

            var hasUrlInCulture = true;
            foreach (Guid ancestorOrSelfKey in ancestorsOrSelfKeysArray)
            {
                var domainLookup = await ancestorOrSelfKeyToDomains[ancestorOrSelfKey];
                if (domainLookup.Any())
                {
                    var domains = domainLookup[culture];
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

                if (_cache.TryGetValue(
                        CreateCacheKey(ancestorOrSelfKey, culture, false),
                        out PublishedDocumentUrlSegment? publishedDocumentUrlSegment))
                {
                    urlSegments.Add(publishedDocumentUrlSegment.UrlSegment);
                }
                else
                {
                    hasUrlInCulture = false;
                }
            }

            //If we did not find a domain and this is not the default language, then the content is not routable
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
                        culture: culture
                    ));
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
                            culture: culture
                        ));
                    }
                    else
                    {
                        result.Add(new UrlInfo(
                            text: "Conflict: Other page has the same url",
                            isUrl: false,
                            culture: culture
                        ));
                    }
                }



            }
        }

        return result;
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

        if(isRootFirstItem is false && urlSegments.Count == 1)
        {
            return false;
        }

        return true;
    }

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

    public async Task CreateOrUpdateUrlSegmentsAsync(Guid key)
    {
        IContent? content = _contentService.GetById(key);

        if (content is not null)
        {
            await CreateOrUpdateUrlSegmentsAsync(content.Yield());
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
            yield return rootKeys.First();

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

    private Guid? GetChildWithUrlSegment(IEnumerable<Guid> childKeys, string urlSegment, string culture, bool isDraft)
    {
        foreach (Guid childKey in childKeys)
        {
            var childUrlSegment = GetUrlSegment(childKey, culture, isDraft);

            if (string.Equals(childUrlSegment, urlSegment))
            {
                return childKey;
            }
        }

        return null;
    }

    /// <summary>
    /// Gets the children based on the latest published version of the content. (No aware of things in this scope).
    /// </summary>
    /// <param name="documentKey">The key of the document to get children from.</param>
    /// <returns>The keys of all the children of the document.</returns>
    private IEnumerable<Guid> GetChildKeys(Guid documentKey)
    {
        if(_documentNavigationQueryService.TryGetChildrenKeys(documentKey, out IEnumerable<Guid> childrenKeys))
        {
            return childrenKeys;
        }

        return Enumerable.Empty<Guid>();
    }

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


    /// <summary>
    /// Gets the top most root key.
    /// </summary>
    /// <returns>The top most root key.</returns>
    private Guid? GetTopMostRootKey(bool isDraft, string culture)
    {
        return GetRootKeys(isDraft, culture).Cast<Guid?>().FirstOrDefault();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string CreateCacheKey(Guid documentKey, string culture, bool isDraft) => $"{documentKey}|{culture}|{isDraft}".ToLowerInvariant();

    private Guid? GetStartNodeKey(int? documentStartNodeId)
    {
        if (documentStartNodeId is null)
        {
            return null;
        }

        Attempt<Guid> attempt = _idKeyMap.GetKeyForId(documentStartNodeId.Value, UmbracoObjectTypes.Document);
        return attempt.Success ? attempt.Result : null;
    }

}
