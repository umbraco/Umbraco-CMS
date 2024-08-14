using System.Collections.Concurrent;
using System.Globalization;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Media.EmbedProviders;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Routing;
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
    Task InitAsync(CancellationToken cancellationToken);

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

    Task<Guid?> GetDocumentKeyByRouteAsync(string route, string culture, int? documentStartNodeId);
}

public class DocumentUrlServiceInitializer : IHostedService, IHostedLifecycleService
{
    private readonly IDocumentUrlService _documentUrlService;
    private readonly IRuntimeState _runtimeState;

    public DocumentUrlServiceInitializer(IDocumentUrlService documentUrlService, IRuntimeState runtimeState)
    {
        _documentUrlService = documentUrlService;
        _runtimeState = runtimeState;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (_runtimeState.Level <= RuntimeLevel.Install)
        {
            return;
        }

        await _documentUrlService.InitAsync(cancellationToken);
    }

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
    private readonly ILogger<DocumentUrlService> _logger;
    private readonly IDocumentUrlRepository _documentUrlRepository;
    private readonly IDocumentRepository _documentRepository;
    private readonly ICoreScopeProvider _coreScopeProvider;
    private readonly GlobalSettings _globalSettings;
    private readonly UrlSegmentProviderCollection _urlSegmentProviderCollection;
    private readonly IContentService _contentService;
    private readonly IShortStringHelper _shortStringHelper;
    private readonly IDomainService _domainService;
    private readonly ILanguageService _languageService;

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
        IDomainService domainService,
        ILanguageService languageService)
    {
        _logger = logger;
        _documentUrlRepository = documentUrlRepository;
        _documentRepository = documentRepository;
        _coreScopeProvider = coreScopeProvider;
        _globalSettings = globalSettings.Value;
        _urlSegmentProviderCollection = urlSegmentProviderCollection;
        _contentService = contentService;
        _shortStringHelper = shortStringHelper;
        _domainService = domainService;
        _languageService = languageService;
    }

    public async Task InitAsync(CancellationToken cancellationToken)
    {
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
                var cacheKey = CreateCacheKey(publishedDocumentUrlSegment.DocumentKey, isoCode, false/*TODO*/);
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
    }

    public Task<bool> ShouldRebuildUrlsAsync()
    {
        // TODO check that the url providers has not chanaged
        // TODO check that the ... has not changed

        return Task.FromResult(false);
    }

    public string? GetUrlSegment(Guid documentKey, string culture, bool isDraft)
    {
        ThrowIfNotInitialized();
        var cacheKey = CreateCacheKey(documentKey, culture, isDraft);
        //TODO handle draft?

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
            DocumentKey = document.Key, LanguageId = language.Id, UrlSegment = urlSegment
        };

        return model;
    }

    public Task DeleteUrlAsync(Guid documentKey, string culture, string segment, bool isDraft) => throw new NotImplementedException();

    public Task DeleteUrlAsync(Guid documentKey) => throw new NotImplementedException();
    public async Task<Guid?> GetDocumentKeyByRouteAsync(string route, string culture, int? documentStartNodeId) /*TODO draft mode?*/
    {
        var urlSegments = route.Split(Constants.CharArrays.ForwardSlash, StringSplitOptions.RemoveEmptyEntries);

        // We need to translate legacy int ids to guid keys.
        var runnerKey = GetStartNodeKey(documentStartNodeId);
        var hideTopLevelNodeFromPath = _globalSettings.HideTopLevelNodeFromPath;

        if ((!_globalSettings.ForceCombineUrlPathLeftToRight
             && CultureInfo.GetCultureInfo(culture).TextInfo.IsRightToLeft))
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

                runnerKey = GetChildWithUrlSegment(childKeys, urlSegment, culture);

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
            // TODO we can remove this to keep consistency witht the old routing, but it seems incorrect to allow that.
            if (hideTopLevelNodeFromPath is false)
            {
                return null;
            }

            return GetTopMostRootKey();
        }

        // Otherwise we have to find the root items (or child of the first root when hideTopLevelNodeFromPath is true) and follow the url segments in them to get to correct document key

        for (var index = 0; index < urlSegments.Length; index++)
        {
            if (runnerKey is null)
            {
                break;
            }

            var urlSegment = urlSegments[index];
            IEnumerable<Guid> runnerKeys = index == 0
                ? GetKeysInRoot(hideTopLevelNodeFromPath)
                : GetChildKeys(runnerKey.Value);
            runnerKey = GetChildWithUrlSegment(runnerKeys, urlSegment, culture);
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

    private IEnumerable<Guid> GetKeysInRoot(bool replaceFirstRootItemWithItsChildren)
    {
        //TODO replace with something more performand
        IEnumerable<Guid> rootKeys = _contentService.GetRootContent().Select(x=>x.Key);

        var replace = replaceFirstRootItemWithItsChildren;
        foreach (Guid rootKey in rootKeys)
        {
            if (replace)
            {
                IEnumerable<Guid> childKeys = GetChildKeys(rootKey);

                foreach (Guid childKey in childKeys)
                {
                    yield return childKey;
                }

                replace = false;
            }

            yield return rootKey;
        }
    }

    private Guid? GetChildWithUrlSegment(IEnumerable<Guid> childKeys, string urlSegment, string culture)
    {
        foreach (Guid childKey in childKeys)
        {
            var childUrlSegment = GetUrlSegment(childKey, culture, false /*TODO figure out about this*/);

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

        // TODO find performand whey to do this
        return _contentService.GetById(documentStartNodeId.Value)?.Key;
    }

}
