using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;

namespace Umbraco.Cms.Core.Services.Navigation;

/// <summary>
/// Implements <see cref="IElementPublishStatusQueryService"/> and <see cref="IElementPublishStatusManagementService"/>
/// verifying and managing the published status of elements.
/// </summary>
public class ElementPublishStatusService :
    PublishStatusService,
    IElementPublishStatusQueryService,
    IElementPublishStatusManagementService
{
    private readonly IPublishStatusRepository _publishStatusRepository;
    private readonly ICoreScopeProvider _coreScopeProvider;
    private readonly ILanguageService _languageService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ElementPublishStatusService"/> class.
    /// </summary>
    /// <param name="logger">The logger for diagnostic output.</param>
    /// <param name="publishStatusRepository">The repository for accessing element publish status data.</param>
    /// <param name="coreScopeProvider">The provider for creating database scopes.</param>
    /// <param name="languageService">The service for retrieving language information.</param>
    public ElementPublishStatusService(
        ILogger<ElementPublishStatusService> logger,
        IPublishStatusRepository publishStatusRepository,
        ICoreScopeProvider coreScopeProvider,
        ILanguageService languageService)
        : base(UmbracoObjectTypes.Element, logger)
    {
        _publishStatusRepository = publishStatusRepository;
        _coreScopeProvider = coreScopeProvider;
        _languageService = languageService;
    }

    /// <inheritdoc/>
    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        IDictionary<Guid, ISet<string>> publishStatus;
        using (ICoreScope scope = _coreScopeProvider.CreateCoreScope())
        {
            publishStatus = await _publishStatusRepository.GetAllElementPublishStatusAsync(cancellationToken);
            scope.Complete();
        }

        PopulateCache(publishStatus);
        DefaultCulture = await _languageService.GetDefaultIsoCodeAsync();
    }

    /// <inheritdoc/>
    public new bool IsPublished(Guid elementKey, string culture)
        => base.IsPublished(elementKey, culture);

    /// <inheritdoc/>
    public new bool IsPublishedInAnyCulture(Guid elementKey)
        => base.IsPublishedInAnyCulture(elementKey);

    /// <inheritdoc/>
    public async Task AddOrUpdateStatusAsync(Guid elementKey, CancellationToken cancellationToken)
    {
        using ICoreScope scope = _coreScopeProvider.CreateCoreScope();
        ISet<string> publishedCultures = await _publishStatusRepository.GetElementPublishStatusAsync(elementKey, cancellationToken);
        SetStatus(elementKey, publishedCultures);
        scope.Complete();
    }

    /// <inheritdoc/>
    public Task RemoveAsync(Guid elementKey, CancellationToken cancellationToken)
    {
        RemoveStatus(elementKey);
        return Task.CompletedTask;
    }
}
