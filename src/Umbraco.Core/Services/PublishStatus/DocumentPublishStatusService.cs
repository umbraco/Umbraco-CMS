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
        => WhereAncestorPathPublished([contentKey], culture: null).Any();

    /// <inheritdoc/>
    public bool HasPublishedAncestorPath(Guid contentKey, string culture)
        => WhereAncestorPathPublished([contentKey], culture).Any();

    /// <inheritdoc/>
    public IEnumerable<Guid> WhereAncestorPathPublished(IEnumerable<Guid> contentKeys, string? culture)
    {
        var memo = new Dictionary<Guid, bool>();

        // "Are ALL ancestors of key published (in the requested culture)?" Only the ancestors we walk
        // through are memoised - they are shared across many candidates - while the candidate keys
        // themselves are not, keeping the dictionary off the usually far more numerous leaves. Hence
        // memoise is false for the top-level candidate call and true for the recursive ancestor calls.
        bool AncestorsPublished(Guid key, bool memoise)
        {
            if (memo.TryGetValue(key, out var cached))
            {
                return cached;
            }

            bool result;
            if (_documentNavigationQueryService.TryGetParentKey(key, out Guid? parentKey) is false)
            {
                // Node not (yet) in navigation - notifications are not ordered, so a node can reach the
                // publish-status cache before the navigation cache. Treat it as having no published path;
                // it will be re-evaluated once it is actually requested.
                result = false;
            }
            else if (parentKey is null)
            {
                // Root: no ancestors, so the ancestor path is vacuously published.
                result = true;
            }
            else
            {
                bool parentPublished = culture is null
                    ? IsPublishedInAnyCulture(parentKey.Value)
                    : IsPublished(parentKey.Value, culture);
                result = parentPublished && AncestorsPublished(parentKey.Value, memoise: true);
            }

            if (memoise)
            {
                memo[key] = result;
            }

            return result;
        }

        foreach (Guid key in contentKeys)
        {
            if (AncestorsPublished(key, memoise: false))
            {
                yield return key;
            }
        }
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
