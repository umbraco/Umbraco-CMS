using System.Collections.Concurrent;
using System.Globalization;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Services;

public interface IDocumentUrlService
{

    /// <summary>
    /// Initializes the service and ensure the content in the database is correct with the current configuration.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task InitAsync(bool forceEmpty, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the Url from a document key, culture and segment. Preview urls are returned if isPreview is true.
    /// </summary>
    /// <param name="documentKey">The key of the document.</param>
    /// <param name="culture">The culture code.</param>
    /// <param name="isDraft">Whether to get the url of the draft or published document.</param>
    /// <returns>The url of the document.</returns>
    string? GetUrlSegment(Guid documentKey, string culture, bool isDraft);

    /// <summary>
    /// Creates or updates a url for a document key, culture and segment and draft information.
    /// </summary>
    /// <param name="documentKey">The key of the document.</param>
    /// <param name="culture">The culture code.</param>
    /// <param name="segment">The segment.</param>
    /// <param name="isDraft">Whether to set the url of the draft or published document.</param>
    /// <param name="urlSegment">The new url segment.</param>
    Task CreateOrUpdateUrlSegmentAsync(Guid documentKey, string culture, string segment, bool isDraft, string urlSegment);

    Task CreateOrUpdateUrlSegmentsAsync(IEnumerable<IContent> documents);

    /// <summary>
    /// Delete a specific url for a document key, culture and segment and draft information.
    /// </summary>
    /// <param name="documentKey">The key of the document.</param>
    /// <param name="culture">The culture code.</param>
    /// <param name="segment">The segment.</param>
    /// <param name="isDraft">Whether to delete the url of the draft or published document.</param>
    Task DeleteUrlAsync(Guid documentKey, string culture, string segment, bool isDraft);

    /// <summary>
    /// Delete all url for a document key, culture and segment and draft information.
    /// </summary>
    /// <param name="documentKey">The key of the document.</param>
    /// <param name="culture">The culture code.</param>
    /// <param name="segment">The segment.</param>
    /// <param name="isDraft">Whether to delete the url of the draft or published document.</param>
    Task DeleteUrlAsync(Guid documentKey);

    Task DeleteUrlsAsync(IEnumerable<IContent> documents);

    Guid? GetDocumentKeyByRoute(string route, string culture, int? documentStartNodeId, bool isDraft);
}

public class DocumentUrlServiceInitializer : IHostedLifecycleService
{
    private readonly IDocumentUrlService _documentUrlService;
    private readonly IRuntimeState _runtimeState;

    public DocumentUrlServiceInitializer(IDocumentUrlService documentUrlService, IRuntimeState runtimeState)
    {
        _documentUrlService = documentUrlService;
        _runtimeState = runtimeState;
    }

    public async Task StartAsync(CancellationToken cancellationToken) => await _documentUrlService.InitAsync(
        _runtimeState.Level <= RuntimeLevel.Install,
        cancellationToken);

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task StartingAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task StartedAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task StoppingAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task StoppedAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}

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
                var cacheKey = CreateCacheKey(publishedDocumentUrlSegment.DocumentKey, isoCode, publishedDocumentUrlSegment.IsDraft);
                if (_cache.TryAdd(cacheKey, publishedDocumentUrlSegment) is false)
                {
                    throw new InvalidOperationException("Could not initialize the document url cache.");
                }
            }
        }
        _isInitialized = true;
        scope.Complete();
    }

    public async Task RebuildAllUrlsAsync()
    {
        using ICoreScope scope = _coreScopeProvider.CreateCoreScope();
        scope.ReadLock(Constants.Locks.ContentTree);

        //TODO we only need keys here and published cultures? or what for drafts?
        var documents = _documentRepository.GetMany(Array.Empty<Guid>()).Where(x=>x.Trashed is false);

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

    public Task CreateOrUpdateUrlSegmentAsync(Guid documentKey, string culture, string segment, bool isDraft, string url) => throw new NotImplementedException();
    public async Task CreateOrUpdateUrlSegmentsAsync(IEnumerable<IContent> documents)
    {
        using ICoreScope scope = _coreScopeProvider.CreateCoreScope();

        var publishedDocumentUrlSegments = new List<PublishedDocumentUrlSegment>();
        var allCultures = documents.SelectMany(x => x.AvailableCultures ).Distinct();

        var languages = await _languageService.GetMultipleAsync(allCultures);
        var languageDictionary = languages.ToDictionary(x=>x.IsoCode);

        //TODO handle drafts
        foreach (IContent document in documents)
        {

            if (document.AvailableCultures.Any())
            {
                foreach (var culture in document.AvailableCultures)
                {
                    var language = languageDictionary[culture];

                    var model = GenerateModel(document, culture, language);

                    if (model is not null)
                    {
                        publishedDocumentUrlSegments.Add(model);
                    }
                }
            }
            else
            {
                var language = await _languageService.GetDefaultLanguageAsync();
                var model = GenerateModel(document, null, language!);

                if (model is not null)
                {
                    publishedDocumentUrlSegments.Add(model);
                }
            }
        }

        //TODO uådate caches


        //TODO rebuild.. What if any is removed?
        _documentUrlRepository.Save(publishedDocumentUrlSegments);

        scope.Complete();
    }

    private PublishedDocumentUrlSegment? GenerateModel(IContent document, string? culture, ILanguage language)
    {
        var urlSegment = document.GetUrlSegment(_shortStringHelper, _urlSegmentProviderCollection, culture);

        if(urlSegment.IsNullOrWhiteSpace())
        {
            _logger.LogWarning("No url segment found for document {DocumentKey} in culture {Culture}", document.Key, culture ?? "{null}");
            return null;
        }

        var model = new PublishedDocumentUrlSegment()
        {
            DocumentKey = document.Key, LanguageId = language.Id, UrlSegment = urlSegment, IsDraft = false
        };

        return model;
    }

    //TODO also call this when trashed!
    public Task DeleteUrlAsync(Guid documentKey, string culture, string segment, bool isDraft) => throw new NotImplementedException();
    public async Task DeleteUrlAsync(Guid documentKey) => throw new NotImplementedException();

    public async Task DeleteUrlsAsync(IEnumerable<IContent> documents)
    {
        await CreateOrUpdateUrlSegmentsAsync(documents);
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
