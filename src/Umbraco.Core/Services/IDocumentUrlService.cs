using System.Globalization;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Media.EmbedProviders;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Services;

public interface IDocumentUrlService
{
    /// <summary>
    /// Rebuilds all urls for all documents in all combinations.
    /// </summary>
    Task RebuildAllUrlsAsync();

    /// <summary>
    /// Gets a bool indicating whether the current urls are valid with the current configuration or not.
    /// </summary>
    Task<bool> ShouldRebuildUrlsAsync();

    /// <summary>
    /// Gets the Url from a document key, culture and segment. Preview urls are returned if isPreview is true.
    /// </summary>
    /// <param name="documentKey">The key of the document.</param>
    /// <param name="culture">The culture code.</param>
    /// <param name="isDraft">Whether to get the url of the draft or published document.</param>
    /// <returns>The url of the document.</returns>
    Task<string> GetUrlSegmentAsync(Guid documentKey, string culture, bool isDraft);

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

    Task<Guid> GetDocumentKeyByRouteAsync(string route);
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
    private readonly IMemoryCache _memoryCache;

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
        ILanguageService languageService,
        IMemoryCache memoryCache)
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
        _memoryCache = memoryCache;
    }

    public async Task RebuildAllUrlsAsync()
    {
        using ICoreScope scope = _coreScopeProvider.CreateCoreScope();
        scope.ReadLock(Constants.Locks.ContentTree);

        //TODO we only need keys here and published cultures? or what for drafts?
        var documents = _documentRepository.GetMany(Array.Empty<Guid>()).Where(x=>x.Trashed is false);

        await CreateOrUpdateUrlSegmentsAsync(documents);
    }

    public Task<bool> ShouldRebuildUrlsAsync() => throw new NotImplementedException();

    public Task<string> GetUrlSegmentAsync(Guid documentKey, string culture, bool isDraft)
    {
        //TODO handle draft?

        _memoryCache.Get()
        throw new NotImplementedException();
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

    private async Task<Route?> GetRouteAsync(Guid documentKey, bool isDraft, string? culture, string? segment)
        {

            //First, we need to walk up from the note to the ancesctor with a domain or is the cotent root, and collect all the url segments.
            var urlSegments = new List<string>();


            // IPublishedContent? content = node;
            IContent? document = _contentService.GetById(documentKey);

            if (document is null || document.Trashed)
            {
                return null;
            }

            var urlSegment = GetUrlSegment(document, culture, segment);
            var hasDomains = await HasDomainAssignedAsync(document.Key, culture);

            // content is null at root
            Guid? runnerKey = documentKey;
            while (hasDomains == false && runnerKey.HasValue)
            {
                // no segment indicates this is not published when this is a variant
                if (urlSegment.IsNullOrWhiteSpace())
                {
                    return null;
                }

                urlSegments.Add(urlSegment);

                // move to parent node
                runnerKey = document is not null ? GetParentKeyFromDocumentKey(document) : null;
                if (runnerKey.HasValue)
                {
                    document = _contentService.GetById(runnerKey.Value);
                    if (document is not null)
                    {
                        urlSegment = GetUrlSegment(document, culture, segment);
                    }
                }

                hasDomains = document is not null && await HasDomainAssignedAsync(document.Key, culture);
            }

            // at this point this will be the urlSegment of the root, no segment indicates this is not published when this is a variant
            if (urlSegment.IsNullOrWhiteSpace())
            {
                return null;
            }

            var hideTopLevelNode = _globalSettings.HideTopLevelNodeFromPath;

            // no domain, respect HideTopLevelNodeFromPath for legacy purposes
            if (hasDomains == false && hideTopLevelNode)
            {
                ApplyHideTopLevelNodeFromPath(document!, urlSegments, isDraft); // todo should this have been the original document or the root?
            }

            // assemble the route- We only have to reverse for left to right languages
            if (_globalSettings.ForceCombineUrlPathLeftToRight || !CultureInfo.GetCultureInfo(culture ?? _globalSettings.DefaultUILanguage).TextInfo.IsRightToLeft)
            {
                urlSegments.Reverse();
            }

            var path = "/" + string.Join("/", urlSegments); // will be "/" or "/foo" or "/foo/bar" etc

            // prefix the root node id containing the domain if it exists (this is a standard way of creating route paths)
            // and is done so that we know the ID of the domain node for the path
            var route = (document?.Id.ToString(CultureInfo.InvariantCulture) ?? string.Empty) + path;

            return new Route(document?.Key, path);
        }

    /// <summary>
    /// Removes the first segment of the urlSegments, if the document is the original root (the item with / as url).
    /// </summary>
    private void ApplyHideTopLevelNodeFromPath(IContentBase document, List<string> urlSegments, bool isDraft)
    {
        // TODO weird fact that we need to test: If the root items is reordered, the urls will change, so the first item will get the /, and thereby the entire subtrees of the moved and old parent have to be rebuild to match old behavior.

        // TODO how can I determine what is the original root, if all urls are begin rebuild? Do we need to store this in key/value?. We could do a migration to add that from the existing logic.
        var parentKey = GetParentKeyFromDocumentKey(document);

        // in theory if hideTopLevelNodeFromPath is true, then there should be only one
        // top-level node, or else domains should be assigned. but for backward compatibility
        // we add this check - we look for the document matching "/" and if it's not us, then
        // we do not hide the top level path
        // it has to be taken care of in GetByRoute too so if
        // "/foo" fails (looking for "/*/foo") we try also "/foo".
        // this does not make much sense anyway esp. if both "/foo/" and "/bar/foo" exist, but
        // that's the way it works pre-4.10 and we try to be backward compat for the time being
        if(parentKey.HasValue)
        {
            if (IsFirstRoot(parentKey.Value))
            {
                urlSegments.RemoveAt(urlSegments.Count - 1);
            }
        }
        else
        {
            urlSegments.RemoveAt(urlSegments.Count - 1);
        }
    }

    private async Task<bool> HasDomainAssignedAsync(Guid documentKey, string? culture, bool includeWildcards = false)
    {
        var assigned = await _domainService.GetAssignedDomainsAsync(documentKey, includeWildcards);

        // It's super important that we always compare cultures with ignore case, since we can't be sure of the casing!
        // Comparing with string.IsNullOrEmpty since both empty string and null signifies invariant.
        return string.IsNullOrEmpty(culture)
            ? assigned.Any()
            : assigned.Any(x => x.LanguageIsoCode?.Equals(culture, StringComparison.InvariantCultureIgnoreCase) ?? false);
    }

    //TODO replace implementation with navigational service when available
    private bool IsFirstRoot(Guid parentKey) => _contentService.GetRootContent().FirstOrDefault()?.Key == parentKey;




    private string? GetUrlSegment(IContentBase document, string? culture, string? segment) => document.GetUrlSegment(_shortStringHelper, _urlSegmentProviderCollection, culture); //TODO can we use segment?

    //TODO replace implementation with navigational service when available
    private Guid? GetParentKeyFromDocumentKey(IContentBase document)
    {
        if (document.ParentId == 0)
        {
            return null;
        }

        Attempt<Guid> attempt = StaticServiceProvider.Instance.GetRequiredService<IIdKeyMap>()
            .GetKeyForId(document.ParentId, UmbracoObjectTypes.Document);

        return attempt.Success ? attempt.Result : null;
    }
}

public sealed class Route
{
    public Route(Guid? relativeRouteKey, string path)
    {
        RelativeRouteKey = relativeRouteKey;
        Path = path;
    }

    /// <summary>
    /// The key of the document that has a domain assigned. Null if no domain is assigned.
    /// </summary>
    public Guid? RelativeRouteKey { get; }

    /// <summary>
    /// The full path of the route.
    /// </summary>
    public string Path { get; }


}
