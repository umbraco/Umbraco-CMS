using System.Collections.Concurrent;
using System.Globalization;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
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
        IIdKeyMap idKeyMap)
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
                UpdateCache(publishedDocumentUrlSegment, isoCode);
            }
        }
        _isInitialized = true;
        scope.Complete();
    }

    private void UpdateCache(PublishedDocumentUrlSegment publishedDocumentUrlSegment, string isoCode)
    {
        var cacheKey = CreateCacheKey(publishedDocumentUrlSegment.DocumentKey, isoCode, publishedDocumentUrlSegment.IsDraft);

        PublishedDocumentUrlSegment? existingValue = null;
        _cache.TryGetValue(cacheKey, out existingValue);

        if (existingValue is null)
        {
            if (_cache.TryAdd(cacheKey, publishedDocumentUrlSegment) is false)
            {
                throw new InvalidOperationException("Could not add the document url cache.");
            }
        }
        else
        {
            if (_cache.TryUpdate(cacheKey, publishedDocumentUrlSegment, existingValue) is false)
            {
                throw new InvalidOperationException("Could not update the document url cache.");
            }
        }
    }

    private void RemoveFromCache(PublishedDocumentUrlSegment publishedDocumentUrlSegment, string isoCode)
    {
        var cacheKey = CreateCacheKey(publishedDocumentUrlSegment.DocumentKey, isoCode, publishedDocumentUrlSegment.IsDraft);
        _cache.TryRemove(cacheKey, out _); // If it do not exists, we are still happy
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
        // TODO check that the url providers has not changed
        // TODO check that the ... has not changed

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
        using ICoreScope scope = _coreScopeProvider.CreateCoreScope();

        var toSave = new List<PublishedDocumentUrlSegment>();
        var toDelete = new List<Guid>();
        var allCultures = documents.SelectMany(x => x.AvailableCultures ).Distinct();

        var languages = await _languageService.GetMultipleAsync(allCultures);
        var languageDictionary = languages.ToDictionary(x=>x.IsoCode);

        foreach (IContent document in documents)
        {

            if (document.AvailableCultures.Any())
            {
                foreach (var culture in document.AvailableCultures)
                {
                    var language = languageDictionary[culture];

                    HandleCaching(document, culture, language, toDelete, toSave);
                }
            }
            else
            {
                var language = await _languageService.GetDefaultLanguageAsync();

                HandleCaching(document, null, language!, toDelete, toSave);
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

        // TODO handle when scope is rolled back, we need to rebuilt cache, to ensure the is no inconsistency.
        scope.Complete();
    }

    private void HandleCaching(IContent document, string? culture, ILanguage language, List<Guid> toDelete, List<PublishedDocumentUrlSegment> toSave)
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
                RemoveFromCache(model, language.IsoCode);
            }
            else
            {
                toSave.Add(model);
                UpdateCache(model, language.IsoCode);
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
    
    public async Task DeleteUrlsAsync(IEnumerable<IContent> documents)
    {
        using ICoreScope scope = _coreScopeProvider.CreateCoreScope();

        var allCultures = documents.SelectMany(x => x.AvailableCultures ).Distinct();

        var languages = await _languageService.GetMultipleAsync(allCultures);
        var languageDictionary = languages.ToDictionary(x=>x.IsoCode);

        foreach (IContent document in documents)
        {
            if (document.AvailableCultures.Any())
            {
                foreach (var culture in document.AvailableCultures)
                {
                    var language = languageDictionary[culture];

                    var models = GenerateModels(document, culture, language);
                    foreach (PublishedDocumentUrlSegment model in models)
                    {
                        RemoveFromCache(model, culture);
                    }
                }
            }
            else
            {
                var language = await _languageService.GetDefaultLanguageAsync();
                var models = GenerateModels(document, null, language!);

                foreach (PublishedDocumentUrlSegment model in models)
                {
                    RemoveFromCache(model, language!.IsoCode);
                }
            }
        }

        _documentUrlRepository.DeleteByDocumentKey(documents.Select(x=>x.Key));

        // TODO handle when scope is rolled back, we need to rebuilt cache, to ensure the is no inconsistency.
        scope.Complete();
    }

    public Guid? GetDocumentKeyByRoute(string route, string culture, int? documentStartNodeId, bool isDraft)
    {
        var urlSegments = route.Split(Constants.CharArrays.ForwardSlash, StringSplitOptions.RemoveEmptyEntries);

        // We need to translate legacy int ids to guid keys.
        Guid? runnerKey = GetStartNodeKey(documentStartNodeId);
        var hideTopLevelNodeFromPath = _globalSettings.HideTopLevelNodeFromPath;

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
            // if we do not hide the top level and no domain was found, it maens there is no content.
            // TODO we can remove this to keep consistency with the old routing, but it seems incorrect to allow that.
            if (hideTopLevelNodeFromPath is false)
            {
                return null;
            }

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

    // TODO replace with a navigational service when available
    private IEnumerable<Guid> GetChildKeys(Guid documentKey)
    {
        var id = _contentService.GetById(documentKey)?.Id;

        if (id.HasValue is false)
        {
            return Enumerable.Empty<Guid>();
        }

        return _contentService.GetPagedChildren(id.Value, 0, int.MaxValue, out _).Select(x => x.Key);
    }

    // TODO replace with a navigational service when available
    private Guid? GetTopMostRootKey()
    {
        return _contentService.GetRootContent().FirstOrDefault()?.Key;
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
