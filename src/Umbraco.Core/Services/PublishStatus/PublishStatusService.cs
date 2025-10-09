using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.DependencyInjection;
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

    private readonly IDictionary<Guid, ISet<string>> _publishedCultures = new Dictionary<Guid, ISet<string>>();

    private string? DefaultCulture { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PublishStatusService"/> class.
    /// </summary>
    [Obsolete("Use non-obsolete constructor. This will be removed in Umbraco 17.")]
    public PublishStatusService(
        ILogger<PublishStatusService> logger,
        IPublishStatusRepository publishStatusRepository,
        ICoreScopeProvider coreScopeProvider)
        : this(
            logger,
            publishStatusRepository,
            coreScopeProvider,
            StaticServiceProvider.Instance.GetRequiredService<ILanguageService>(),
            StaticServiceProvider.Instance.GetRequiredService<IDocumentNavigationQueryService>())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PublishStatusService"/> class.
    /// </summary>
    [Obsolete("Use non-obsolete constructor. This will be removed in Umbraco 17.")]
    public PublishStatusService(
        ILogger<PublishStatusService> logger,
        IPublishStatusRepository publishStatusRepository,
        ICoreScopeProvider coreScopeProvider,
        ILanguageService languageService)
        : this(
            logger,
            publishStatusRepository,
            coreScopeProvider,
            languageService,
            StaticServiceProvider.Instance.GetRequiredService<IDocumentNavigationQueryService>())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PublishStatusService"/> class.
    /// </summary>
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
        _publishedCultures.Clear();
        IDictionary<Guid, ISet<string>> publishStatus;
        using (ICoreScope scope = _coreScopeProvider.CreateCoreScope())
        {
            publishStatus = await _publishStatusRepository.GetAllPublishStatusAsync(cancellationToken);
            scope.Complete();
        }

        foreach ((Guid documentKey, ISet<string> publishedCultures) in publishStatus)
        {
            if (_publishedCultures.TryAdd(documentKey, publishedCultures) is false)
            {
                _logger.LogWarning("Failed to add published cultures for document {DocumentKey}", documentKey);
            }
        }

        DefaultCulture = await _languageService.GetDefaultIsoCodeAsync();
    }

    /// <inheritdoc/>
    public bool IsDocumentPublished(Guid documentKey, string culture)
    {
        if (string.IsNullOrEmpty(culture) && DefaultCulture is not null)
        {
            culture = DefaultCulture;
        }

        if (_publishedCultures.TryGetValue(documentKey, out ISet<string>? publishedCultures))
        {
            return publishedCultures.Contains(culture, StringComparer.InvariantCultureIgnoreCase);
        }

        _logger.LogDebug("Document {DocumentKey} not found in the publish status cache", documentKey);
        return false;
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

            if (IsDocumentPublishedInAnyCulture(key) is false)
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
        _publishedCultures[documentKey] = publishedCultures;
        scope.Complete();
    }

    /// <inheritdoc/>
    public Task RemoveAsync(Guid documentKey, CancellationToken cancellationToken)
    {
        _publishedCultures.Remove(documentKey);
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

        foreach ((Guid documentKey, ISet<string> publishedCultures) in publishStatus)
        {
            _publishedCultures[documentKey] = publishedCultures;
        }
    }
}
