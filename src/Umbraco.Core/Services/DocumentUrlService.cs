using System.Collections.Concurrent;
using System.Globalization;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Caching.Memory;
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
    private readonly IDomainService _domainService;

    private readonly ConcurrentDictionary<string, PublishedDocumentUrlSegment> _cache = new();
    private bool _isInitialized = false;

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
        IDomainService domainService)
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
        _domainService = domainService;
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
            PublishedDocumentUrlSegment? existingValue = null;
            _cache.TryGetValue(cacheKey, out existingValue);

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

    private void RemoveFromCache(IScopeContext scopeContext, Guid documentKey, string isoCode)
    {
        var cacheKeyDraft = CreateCacheKey(documentKey, isoCode, true);

        scopeContext.Enlist("RemoveFromCache_" + cacheKeyDraft, () =>
        {
            if (_cache.TryRemove(cacheKeyDraft, out _) is false)
            {
                _logger.LogDebug("Could not remove the document url cache. But the important thing is that it is not there.");
                return false;
            }

            return true;
        });

        var cacheKeyPublished = CreateCacheKey(documentKey, isoCode, false);

        scopeContext.Enlist("RemoveFromCache_" + cacheKeyPublished, () =>
        {
            if (_cache.TryRemove(cacheKeyPublished, out _) is false)
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

        IEnumerable<IContent> documents = _documentRepository.GetMany(Array.Empty<Guid>());

        await CreateOrUpdateUrlSegmentsAsync(documents);

        _keyValueService.SetValue(RebuildKey, GetCurrentRebuildValue());

        scope.Complete();
    }

    public Task<bool> ShouldRebuildUrlsAsync()
    {
        var persistedValue = GetPersistedRebuildValue();
        var currentValue = GetCurrentRebuildValue();

        return Task.FromResult(string.Equals(persistedValue, currentValue) is false);
    }

    private string GetCurrentRebuildValue()
    {
        return string.Join("|", _urlSegmentProviderCollection.Select(x => x.GetType().Name));
    }

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

    public async Task CreateOrUpdateUrlSegmentsAsync(IEnumerable<IContent> documents)
    {
        if(documents.Any() is false)
        {
            return;
        }

        using ICoreScope scope = _coreScopeProvider.CreateCoreScope();

        var toSave = new List<PublishedDocumentUrlSegment>();
        var toDelete = new List<Guid>();
        var allCultures = documents.SelectMany(x => x.AvailableCultures ).Distinct();

        var languages = await _languageService.GetAllAsync();
        var languageDictionary = languages.ToDictionary(x=>x.IsoCode);

        foreach (IContent document in documents)
        {
            if (_logger.IsEnabled(LogLevel.Trace))
            {
                _logger.LogTrace("Rebuilding urls for document with key {DocumentKey}", document.Key);
            }

            foreach ((string culture, ILanguage language) in languageDictionary)
            {
                HandleCaching(_coreScopeProvider.Context!, document, document.ContentType.VariesByCulture() ? culture : null, language, toDelete, toSave);
            }
        }

        if(toSave.Any())
        {
            _documentUrlRepository.Save(toSave);
        }

        if(toDelete.Any())
        {
            _documentUrlRepository.DeleteByDocumentKey(toDelete);
        }

        scope.Complete();
    }

    private void HandleCaching(IScopeContext scopeContext, IContent document, string? culture, ILanguage language, List<Guid> toDelete, List<PublishedDocumentUrlSegment> toSave)
    {
        var models = GenerateModels(document, culture, language);

        foreach (PublishedDocumentUrlSegment model in models)
        {
            if (document.Published is false && model.IsDraft is false)
            {
                continue;
            }

            if (document.Trashed)
            {
                toDelete.Add(model.DocumentKey);
                RemoveFromCache(scopeContext, model.DocumentKey, language.IsoCode);
            }
            else
            {
                toSave.Add(model);
                UpdateCache(scopeContext, model, language.IsoCode);
            }
        }
    }

    private IEnumerable<PublishedDocumentUrlSegment> GenerateModels(IContent document, string? culture, ILanguage language)
    {
        var publishedUrlSegment = document.GetUrlSegment(_shortStringHelper, _urlSegmentProviderCollection, culture, true);
        if(publishedUrlSegment.IsNullOrWhiteSpace())
        {
            _logger.LogWarning("No published url segment found for document {DocumentKey} in culture {Culture}", document.Key, culture ?? "{null}");
        }
        else
        {
            yield return new PublishedDocumentUrlSegment()
            {
                DocumentKey = document.Key, LanguageId = language.Id, UrlSegment = publishedUrlSegment, IsDraft = false
            };
        }

        var draftUrlSegment = document.GetUrlSegment(_shortStringHelper, _urlSegmentProviderCollection, culture, false);

        if(draftUrlSegment.IsNullOrWhiteSpace())
        {
            _logger.LogWarning("No draft url segment found for document {DocumentKey} in culture {Culture}", document.Key, culture ?? "{null}");
        }
        else
        {
            yield return new PublishedDocumentUrlSegment()
            {
                DocumentKey = document.Key, LanguageId = language.Id, UrlSegment = draftUrlSegment, IsDraft = true
            };
        }
    }

    public async Task DeleteUrlsFromCacheAsync(IEnumerable<Guid> documentKeys)
    {
        using ICoreScope scope = _coreScopeProvider.CreateCoreScope();

        IEnumerable<ILanguage> languages = await _languageService.GetAllAsync();

        foreach (ILanguage language in languages)
        {
            foreach (Guid documentKey in documentKeys)
            {
                RemoveFromCache(_coreScopeProvider.Context!, documentKey, language.IsoCode);
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
            // If there is no url segments it means the domain root has been requested
            if (urlSegments.Length == 0)
            {
                return runnerKey.Value;
            }

            // Otherwise we have to find the child with that segment anc follow that
            foreach (var urlSegment in urlSegments)
            {
                //Get the children of the runnerKey and find the child (if any) with the correct url segment
                var childKeys = GetChildKeys(runnerKey.Value);

                runnerKey = GetChildWithUrlSegment(childKeys, urlSegment, culture, isDraft);

                if (runnerKey is null)
                {
                    break;
                }
            }

            return runnerKey;
        }
        // If there is no parts, it means it is a root (and no assigned domain)
        if(urlSegments.Length == 0)
        {
            // // if we do not hide the top level and no domain was found, it maens there is no content.
            // // TODO we can remove this to keep consistency with the old routing, but it seems incorrect to allow that.
            // if (hideTopLevelNodeFromPath is false)
            // {
            //     return null;
            // }

            return GetTopMostRootKey();
        }

        // Otherwise we have to find the root items (or child of the first root when hideTopLevelNodeFromPath is true) and follow the url segments in them to get to correct document key
        for (var index = 0; index < urlSegments.Length; index++)
        {
            var urlSegment = urlSegments[index];
            IEnumerable<Guid> runnerKeys;
            if (index == 0)
            {
                runnerKeys = GetKeysInRoot(hideTopLevelNodeFromPath);
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

        return runnerKey;
    }

    public string GetLegacyRouteFormat(Guid docuemntKey, string? culture, bool isDraft)
    {
        var documentIdAttempt = _idKeyMap.GetIdForKey(docuemntKey, UmbracoObjectTypes.Document);

        if(documentIdAttempt.Success is false)
        {
            return "#";
        }

        if (_documentNavigationQueryService.TryGetAncestorsOrSelfKeys(docuemntKey,
                out IEnumerable<Guid> ancestorsOrSelfKeys) is false)
        {
            return "#";
        }

        var cultureOrDefault = culture ?? _languageService.GetDefaultIsoCodeAsync().GetAwaiter().GetResult();

        Guid[] ancestorsOrSelfKeysArray = ancestorsOrSelfKeys as Guid[] ?? ancestorsOrSelfKeys.ToArray();
        IDictionary<Guid, IDomain?> ancestorOrSelfKeyToDomains = ancestorsOrSelfKeysArray.ToDictionary(x => x, ancestorKey =>
        {
            IEnumerable<IDomain> domains = _domainService.GetAssignedDomainsAsync(ancestorKey, false).GetAwaiter().GetResult();
            return domains.FirstOrDefault(x=>x.LanguageIsoCode == cultureOrDefault);
        });

        var urlSegments = new List<string>();

        IDomain? foundDomain = null;

        foreach (Guid ancestorOrSelfKey in ancestorsOrSelfKeysArray)
        {
            if (ancestorOrSelfKeyToDomains.TryGetValue(ancestorOrSelfKey, out IDomain? domain))
            {
                if (domain is not null)
                {
                    foundDomain = domain;
                    break;
                }
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

        if (foundDomain is not null)
        {
            //we found a domain, and not to construct the route in the funny legacy way
            return foundDomain.RootContentId + "/" + string.Join("/", urlSegments);
        }

        var isRootFirstItem = GetTopMostRootKey() == ancestorsOrSelfKeysArray.Last();
        return GetFullUrl(isRootFirstItem, urlSegments, null);
    }

    public bool HasAny()
    {
        ThrowIfNotInitialized();
        return _cache.Any();
    }


    public async Task<IEnumerable<UrlInfo>> ListUrlsAsync(Guid contentKey)
    {
        var result = new List<UrlInfo>();

        var documentIdAttempt = _idKeyMap.GetIdForKey(contentKey, UmbracoObjectTypes.Document);

        if(documentIdAttempt.Success is false)
        {
            return result;
        }

        IEnumerable<Guid> ancestorsOrSelfKeys = contentKey.Yield()
            .Concat(_contentService.GetAncestors(documentIdAttempt.Result).Select(x => x.Key).Reverse());

        IEnumerable<ILanguage> languages = await _languageService.GetAllAsync();
        IEnumerable<string> cultures = languages.Select(x=>x.IsoCode);


        Guid[] ancestorsOrSelfKeysArray = ancestorsOrSelfKeys as Guid[] ?? ancestorsOrSelfKeys.ToArray();
        Dictionary<Guid, Task<Dictionary<string, IDomain>>> ancestorOrSelfKeyToDomains = ancestorsOrSelfKeysArray.ToDictionary(x => x, async ancestorKey =>
        {
            IEnumerable<IDomain> domains = await _domainService.GetAssignedDomainsAsync(ancestorKey, false);
            return domains.ToDictionary(x => x.LanguageIsoCode!);
        });

        foreach (var culture in cultures)
        {
           var urlSegments = new List<string>();
           IDomain? foundDomain = null;

           var hasUrlInCulture = true;
           foreach (Guid ancestorOrSelfKey in ancestorsOrSelfKeysArray)
           {
                if (ancestorOrSelfKeyToDomains.TryGetValue(ancestorOrSelfKey, out Task<Dictionary<string, IDomain>>? domainDictionaryTask))
                {
                    var domainDictionary = await domainDictionaryTask;
                    if (domainDictionary.TryGetValue(culture, out IDomain? domain))
                    {
                        foundDomain = domain;
                        break;
                    }
                }

                if (_cache.TryGetValue(CreateCacheKey(ancestorOrSelfKey, culture, false), out PublishedDocumentUrlSegment? publishedDocumentUrlSegment))
                {
                    urlSegments.Add(publishedDocumentUrlSegment.UrlSegment);
                }
                else
                {
                    hasUrlInCulture = false;
                }
            }

            var isRootFirstItem = GetTopMostRootKey() == ancestorsOrSelfKeysArray.Last();
            result.Add(new UrlInfo(
                text: GetFullUrl(isRootFirstItem, urlSegments, foundDomain),
                isUrl: hasUrlInCulture,
                culture: culture
            ));

        }

        return result;
    }

    private string GetFullUrl(bool isRootFirstItem, List<string> reversedUrlSegments, IDomain? foundDomain)
    {
        var urlSegments = new List<string>(reversedUrlSegments);
        urlSegments.Reverse();

        if (foundDomain is not null)
        {
            return foundDomain.DomainName.EnsureEndsWith("/") + string.Join('/', urlSegments);
        }

        return '/' + string.Join('/', urlSegments.Skip(_globalSettings.HideTopLevelNodeFromPath && isRootFirstItem ? 1 : 0));
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


    //TODO test cases:
    // - Find the root, when a domain is set
    // - Find a nested child, when a domain is set

    // - Find the root when no domain is set and hideTopLevelNodeFromPath is true
    // - Find a nested child of item in the root top when no domain is set and hideTopLevelNodeFromPath is true
    // - Find a nested child of item in the root bottom when no domain is set and hideTopLevelNodeFromPath is true
    // - Find the root when no domain is set and hideTopLevelNodeFromPath is false
    // - Find a nested child of item in the root top when no domain is set and hideTopLevelNodeFromPath is false
    // - Find a nested child of item in the root bottom when no domain is set and hideTopLevelNodeFromPath is false

    // - All of the above when having Constants.Conventions.Content.UrlName set to a value

    private IEnumerable<Guid> GetKeysInRoot(bool addFirstLevelChildren)
    {
        //TODO replace with something more performand - Should be possible with navigationservice..
        IEnumerable<Guid> rootKeys = _contentService.GetRootContent().Select(x=>x.Key).ToArray();

        foreach (Guid rootKey in rootKeys)
        {
            yield return rootKey;
        }

        if (addFirstLevelChildren)
        {
            foreach (Guid rootKey in rootKeys)
            {
                IEnumerable<Guid> childKeys = GetChildKeys(rootKey);

                foreach (Guid childKey in childKeys)
                {
                    yield return childKey;
                }
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

    /// <summary>
    /// Gets the top most root key.
    /// </summary>
    /// <returns>The top most root key.</returns>
    private Guid? GetTopMostRootKey()
    {
        if (_documentNavigationQueryService.TryGetRootKeys(out IEnumerable<Guid> rootKeys))
        {
            return rootKeys.FirstOrDefault();
        }
        return null;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string CreateCacheKey(Guid documentKey, string culture, bool isDraft) => $"{documentKey}|{culture}|{isDraft}";

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
