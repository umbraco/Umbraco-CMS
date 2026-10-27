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
        => AncestorsPublished(contentKey, culture: null);

    /// <inheritdoc/>
    public bool HasPublishedAncestorPath(Guid contentKey, string culture)
        => AncestorsPublished(contentKey, culture);

    /// <inheritdoc/>
    public IEnumerable<Guid> WhereAncestorPathPublished(IEnumerable<Guid> contentKeys, string? culture)
    {
        // Only the ancestors we walk through are memoised - they are shared across many candidates -
        // while the candidate keys themselves are not, keeping the dictionary off the usually far more
        // numerous leaves. Both buffers are reused across the whole set.
        var memo = new Dictionary<Guid, bool>();
        var walked = new List<Guid>();

        foreach (Guid contentKey in contentKeys)
        {
            if (AncestorsPublished(contentKey, culture, memo, walked))
            {
                yield return contentKey;
            }
        }
    }

    /// <summary>
    /// Determines whether all ancestors of <paramref name="contentKey"/> are published in the requested culture.
    /// </summary>
    /// <param name="contentKey">The document key whose ancestor path to verify.</param>
    /// <param name="culture">The culture to check, or <c>null</c> to accept any published culture.</param>
    /// <param name="memo">An optional cache of already resolved ancestors, shared across a batch of candidates.</param>
    /// <param name="walked">An optional buffer holding the chain walked for the current candidate, used to populate <paramref name="memo"/>.</param>
    /// <remarks>
    /// The chain is walked iteratively rather than recursively, so that deep content structures cannot overflow the stack.
    /// </remarks>
    private bool AncestorsPublished(Guid contentKey, string? culture, Dictionary<Guid, bool>? memo = null, List<Guid>? walked = null)
    {
        walked?.Clear();

        Guid current = contentKey;
        bool result;

        while (true)
        {
            if (memo is not null && memo.TryGetValue(current, out var cached))
            {
                result = cached;
                break;
            }

            walked?.Add(current);

            if (_documentNavigationQueryService.TryGetParentKey(current, out Guid? parentKey) is false)
            {
                // Node not (yet) in navigation - notifications are not ordered, so a node can reach the
                // publish-status cache before the navigation cache. Treat it as having no published path;
                // it will be re-evaluated once it is actually requested.
                result = false;
                break;
            }

            if (parentKey is null)
            {
                // Root: no ancestors, so the ancestor path is vacuously published.
                result = true;
                break;
            }

            var parentPublished = culture is null
                ? IsPublishedInAnyCulture(parentKey.Value)
                : IsPublished(parentKey.Value, culture);
            if (parentPublished is false)
            {
                result = false;
                break;
            }

            current = parentKey.Value;
        }

        // The walk stops at the first unpublished (or unresolvable) ancestor, so the entire walked chain
        // shares the result. The candidate key itself - the first entry - is deliberately not memoised.
        if (memo is not null && walked is not null)
        {
            for (var i = 1; i < walked.Count; i++)
            {
                memo[walked[i]] = result;
            }
        }

        return result;
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

    /// <inheritdoc/>
    public async Task RefreshCultureAsync(string culture, CancellationToken cancellationToken)
    {
        HashSet<Guid> publishedKeys;
        using (ICoreScope scope = _coreScopeProvider.CreateCoreScope())
        {
            publishedKeys = (await _publishStatusRepository.GetPublishedKeysAsync(culture, cancellationToken)).ToHashSet();
            scope.Complete();
        }

        RefreshCulture(culture, publishedKeys);
    }

    /// <inheritdoc/>
    public Task RefreshDefaultCultureAsync(CancellationToken cancellationToken)
    {
        InvalidateDefaultCulture();
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    protected override string? ReadDefaultCulture()
        => _languageService.GetDefaultIsoCodeAsync().GetAwaiter().GetResult();
}
