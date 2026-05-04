using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;

namespace Umbraco.Cms.Core.Services.Navigation;

/// <summary>
/// Implements <see cref="IDocumentPublishStatusQueryService"/> and <see cref="IDocumentPublishStatusManagementService"/>
/// verifying and managing the published status of documents.
/// </summary>
public class DocumentPublishStatusService :
    PublishStatusService,
    IDocumentPublishStatusQueryService,
    IDocumentPublishStatusManagementService,
#pragma warning disable CS0618 // Type or member is obsolete
    IPublishStatusManagementService,
    IPublishStatusQueryService
#pragma warning restore CS0618 // Type or member is obsolete
{
    private readonly IPublishStatusRepository _publishStatusRepository;
    private readonly ICoreScopeProvider _coreScopeProvider;
    private readonly ILanguageService _languageService;
    private readonly IDocumentNavigationQueryService _documentNavigationQueryService;

    /// <summary>
    /// Initializes a new instance of the <see cref="DocumentPublishStatusService"/> class.
    /// </summary>
    /// <param name="logger">The logger for diagnostic output.</param>
    /// <param name="publishStatusRepository">The repository for accessing document publish status data.</param>
    /// <param name="coreScopeProvider">The provider for creating database scopes.</param>
    /// <param name="languageService">The service for retrieving language information.</param>
    /// <param name="documentNavigationQueryService">The service for querying document navigation structure.</param>
    public DocumentPublishStatusService(
        ILogger<DocumentPublishStatusService> logger,
        IPublishStatusRepository publishStatusRepository,
        ICoreScopeProvider coreScopeProvider,
        ILanguageService languageService,
        IDocumentNavigationQueryService documentNavigationQueryService)
        : base(UmbracoObjectTypes.Document, logger)
    {
        _publishStatusRepository = publishStatusRepository;
        _coreScopeProvider = coreScopeProvider;
        _languageService = languageService;
        _documentNavigationQueryService = documentNavigationQueryService;
    }

    /// <inheritdoc/>
    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        IDictionary<Guid, ISet<string>> publishStatus;
        using (ICoreScope scope = _coreScopeProvider.CreateCoreScope())
        {
            publishStatus = await _publishStatusRepository.GetAllPublishStatusAsync(cancellationToken);
            scope.Complete();
        }

        PopulateCache(publishStatus);
        DefaultCulture = await _languageService.GetDefaultIsoCodeAsync();
    }

    /// <inheritdoc/>
    public new bool IsPublished(Guid documentKey, string culture)
        => base.IsPublished(documentKey, culture);

    /// <inheritdoc/>
    public new bool IsPublishedInAnyCulture(Guid documentKey)
        => base.IsPublishedInAnyCulture(documentKey);

    /// <inheritdoc/>
    public bool HasPublishedAncestorPath(Guid contentKey)
        => HasPublishedAncestorPathInternal(contentKey, culture: null);

    /// <inheritdoc/>
    public bool HasPublishedAncestorPath(Guid contentKey, string culture)
        => HasPublishedAncestorPathInternal(contentKey, culture);

    private bool HasPublishedAncestorPathInternal(Guid contentKey, string? culture)
    {
        var success = _documentNavigationQueryService.TryGetAncestorsKeys(contentKey, out IEnumerable<Guid> keys);
        if (success is false)
        {
            // This might happen in certain cases, since notifications are not ordered, for instance, if you save and publish a content node in the same scope.
            // In this case we'll try and update the node in the cache even though it hasn't been updated in the document navigation cache yet.
            // It's okay to just return false here, since the node will be loaded later when it's actually requested.
            return false;
        }

        foreach (Guid key in keys)
        {
            var isPublished = culture is null
                ? IsPublishedInAnyCulture(key)
                : IsPublished(key, culture);

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
        SetStatus(documentKey, publishedCultures);
        scope.Complete();
    }

    /// <inheritdoc/>
    public Task RemoveAsync(Guid documentKey, CancellationToken cancellationToken)
    {
        RemoveStatus(documentKey);
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
            SetStatus(documentKey, publishedCultures);
        }
    }
}
