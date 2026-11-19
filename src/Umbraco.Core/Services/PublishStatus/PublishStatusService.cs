using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;

namespace Umbraco.Cms.Core.Services.Navigation;

/// <summary>
/// Implements <see cref="IPublishStatusManagementService" /> and <see cref="IPublishStatusQueryService" /> verifying the published
/// status of documents.
/// </summary>
public class PublishStatusService : IPublishStatusManagementService, IPublishStatusQueryService
{
    private readonly ILogger<PublishStatusService> _logger;
    private readonly IPublishStatusRepository _publishStatusRepository;
    private readonly ICoreScopeProvider _coreScopeProvider;
    private readonly ILanguageService _languageService;
    private readonly IDocumentNavigationQueryService _documentNavigationQueryService;

    private ConcurrentDictionary<Guid, ISet<string>> _publishedCultures = new();
    private volatile bool _defaultCultureIsStale;

    /// <summary>
    /// Gets or sets the default culture ISO code used when no culture is specified.
    /// </summary>
    private string? DefaultCulture { get; set; }

/// <summary>
/// Initializes a new instance of the <see cref="PublishStatusService"/> class.
/// </summary>
/// <param name="logger">The logger for diagnostic output.</param>
/// <param name="publishStatusRepository">The repository for accessing publish status data.</param>
/// <param name="coreScopeProvider">The provider for creating database scopes.</param>
/// <param name="languageService">The service for retrieving language information.</param>
/// <param name="documentNavigationQueryService">The service for querying document navigation structure.</param>
public PublishStatusService(
    ILogger<PublishStatusService> logger,
    IPublishStatusRepository publishStatusRepository,
    ICoreScopeProvider coreScopeProvider,
    ILanguageService languageService,
    IDocumentNavigationQueryService documentNavigationQueryService)
{
    _logger = logger;
    _publishStatusRepository = publishStatusRepository;
    _coreScopeProvider = coreScopeProvider;
    _languageService = languageService;
    _documentNavigationQueryService = documentNavigationQueryService;
}

    /// <inheritdoc/>
    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        // On subscriber/CD servers in a load-balanced setup, cache refresh notifications can trigger
        // re-initialization while other threads are reading _publishedCultures. The previous approach
        // called _publishedCultures.Clear() then gradually re-added entries, creating a window where
        // concurrent readers (e.g. HasPublishedAncestorPath) would see an empty dictionary and
        // incorrectly conclude that content is unpublished.
        //
        // Instead, we build a completely new dictionary from the DB, then swap it in with
        // Interlocked.Exchange. Readers that already hold a reference to the old dictionary continue
        // to use it safely; new readers pick up the fully populated replacement. There is no window
        // where the dictionary is empty.
        //
        // Verified by: PublishStatusServiceTests.Concurrent_Initialize_Never_Transiently_Loses_Published_Status
        IDictionary<Guid, ISet<string>> publishStatus;
        using (ICoreScope scope = _coreScopeProvider.CreateCoreScope())
        {
            publishStatus = await _publishStatusRepository.GetAllPublishStatusAsync(cancellationToken);
            scope.Complete();
        }

        var newDictionary = new ConcurrentDictionary<Guid, ISet<string>>(publishStatus);
        _defaultCultureIsStale = false;
        DefaultCulture = await _languageService.GetDefaultIsoCodeAsync();

        Interlocked.Exchange(ref _publishedCultures, newDictionary);
    }

    /// <inheritdoc/>
    public async Task RefreshCultureAsync(string culture, CancellationToken cancellationToken)
    {
        HashSet<Guid> publishedKeys;
        using (ICoreScope scope = _coreScopeProvider.CreateCoreScope())
        {
            publishedKeys = (await _publishStatusRepository.GetPublishedKeysAsync(culture, cancellationToken)).ToHashSet();
            scope.Complete();
        }

        // The cached sets are read concurrently, so they are replaced rather than modified in place.
        ConcurrentDictionary<Guid, ISet<string>> publishedCultures = Volatile.Read(ref _publishedCultures);
        foreach (Guid documentKey in publishedKeys)
        {
            publishedCultures.AddOrUpdate(
                documentKey,
                _ => new HashSet<string> { culture },
                (_, existing) => existing.Contains(culture, StringComparer.InvariantCultureIgnoreCase)
                    ? existing
                    : new HashSet<string>(existing) { culture });
        }

        foreach ((Guid documentKey, ISet<string> existing) in publishedCultures)
        {
            if (publishedKeys.Contains(documentKey) || existing.Contains(culture, StringComparer.InvariantCultureIgnoreCase) is false)
            {
                continue;
            }

            var remaining = existing
                .Where(x => string.Equals(x, culture, StringComparison.InvariantCultureIgnoreCase) is false)
                .ToHashSet();

            // A failed swap means the entry was reloaded from the database after the language change, so it is
            // already correct and is left as it is.
            if (remaining.Count > 0)
            {
                publishedCultures.TryUpdate(documentKey, remaining, existing);
            }
            else
            {
                publishedCultures.TryRemove(new KeyValuePair<Guid, ISet<string>>(documentKey, existing));
            }
        }
    }

    /// <inheritdoc/>
    /// <remarks>
    /// The default language is re-read on the next query that needs it, rather than here, so a language change does
    /// not reload the languages on every server unless a query without a culture is made.
    /// </remarks>
    public Task RefreshDefaultCultureAsync(CancellationToken cancellationToken)
    {
        _defaultCultureIsStale = true;
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public bool IsDocumentPublished(Guid documentKey, string culture)
    {
        if (string.IsNullOrEmpty(culture) && GetDefaultCulture() is { } defaultCulture)
        {
            culture = defaultCulture;
        }

        if (_publishedCultures.TryGetValue(documentKey, out ISet<string>? publishedCultures))
        {
            // If "*" is provided as the culture, we consider this as "published in any culture". This aligns
            // with behaviour in Umbraco 13.
            return culture == Constants.System.InvariantCulture || publishedCultures.Contains(culture, StringComparer.InvariantCultureIgnoreCase);
        }

        _logger.LogDebug("Document {DocumentKey} not found in the publish status cache", documentKey);
        return false;
    }

    private string? GetDefaultCulture()
    {
        if (_defaultCultureIsStale)
        {
            _defaultCultureIsStale = false;
            DefaultCulture = _languageService.GetDefaultIsoCodeAsync().GetAwaiter().GetResult();
        }

        return DefaultCulture;
    }

    /// <inheritdoc />
    public bool IsDocumentPublishedInAnyCulture(Guid documentKey)
    {
        if (_publishedCultures.TryGetValue(documentKey, out ISet<string>? publishedCultures))
        {
            return publishedCultures.Count > 0;
        }

        _logger.LogDebug("Document {DocumentKey} not found in the publish status cache", documentKey);
        return false;
    }

    /// <inheritdoc/>
    public bool HasPublishedAncestorPath(Guid contentKey)
        => HasPublishedAncestorPathInternal(contentKey, null);

    /// <inheritdoc/>
    public bool HasPublishedAncestorPath(Guid contentKey, string culture)
        => HasPublishedAncestorPathInternal(contentKey, culture);

    private bool HasPublishedAncestorPathInternal(Guid contentKey, string? culture)
    {
        var success = _documentNavigationQueryService.TryGetAncestorsKeys(contentKey, out IEnumerable<Guid> keys);
        if (success is false)
        {
            // This might happen is certain cases, since notifications are not ordered, for instance, if you save and publish a content node in the same scope.
            // In this case we'll try and update the node in the cache even though it hasn't been updated in the document navigation cache yet.
            // It's okay to just return false here, since the node will be loaded later when it's actually requested.
            return false;
        }

        foreach (Guid key in keys)
        {
            var isPublished = culture is null
                ? IsDocumentPublishedInAnyCulture(key)
                : IsDocumentPublished(key, culture);

            if (isPublished is false)
            {
                return false;
            }
        }

        return true;
    }

    /// <inheritdoc/>
    public async Task AddOrUpdateStatusAsync(Guid documentKey, CancellationToken cancellationToken)
    {
        using ICoreScope scope = _coreScopeProvider.CreateCoreScope();
        ISet<string> publishedCultures = await _publishStatusRepository.GetPublishStatusAsync(documentKey, cancellationToken);
        UpdatePublishedCultures(documentKey, publishedCultures);
        scope.Complete();
    }

    /// <inheritdoc />
    public void AddOrUpdateStatus(Guid documentKey)
    {
        using ICoreScope scope = _coreScopeProvider.CreateCoreScope();
        ISet<string> publishedCultures = _publishStatusRepository.GetPublishStatus(documentKey);
        UpdatePublishedCultures(documentKey, publishedCultures);
        scope.Complete();
    }

    /// <inheritdoc/>
    public Task RemoveAsync(Guid documentKey, CancellationToken cancellationToken)
    {
        RemovePublishedCultures(documentKey);
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public async Task AddOrUpdateStatusWithDescendantsAsync(Guid rootDocumentKey, CancellationToken cancellationToken)
    {
        IDictionary<Guid, ISet<string>> publishStatus;
        using (ICoreScope scope = _coreScopeProvider.CreateCoreScope())
        {
            publishStatus = await _publishStatusRepository.GetDescendantsOrSelfPublishStatusAsync(rootDocumentKey, cancellationToken);
            scope.Complete();
        }

        UpdatePublishedCultures(publishStatus);
    }

    /// <inheritdoc />
    public void AddOrUpdateStatusWithDescendants(Guid rootDocumentKey)
    {
        IDictionary<Guid, ISet<string>> publishStatus;
        using (ICoreScope scope = _coreScopeProvider.CreateCoreScope())
        {
            publishStatus = _publishStatusRepository.GetDescendantsOrSelfPublishStatus(rootDocumentKey);
            scope.Complete();
        }

        UpdatePublishedCultures(publishStatus);
    }

    private void UpdatePublishedCultures(IDictionary<Guid, ISet<string>> publishStatus)
    {
        foreach ((Guid documentKey, ISet<string> publishedCultures) in publishStatus)
        {
            UpdatePublishedCultures(documentKey, publishedCultures);
        }
    }

    private void UpdatePublishedCultures(Guid documentKey, ISet<string> publishedCultures)
    {
        if (publishedCultures.Count > 0)
        {
            _publishedCultures[documentKey] = publishedCultures;
        }
        else
        {
            RemovePublishedCultures(documentKey);
        }
    }

    private void RemovePublishedCultures(Guid documentKey)
        => _publishedCultures.TryRemove(documentKey, out _);
}
