using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services.Navigation;
using Umbraco.Cms.Infrastructure.HybridCache.Persistence;

namespace Umbraco.Cms.Infrastructure.HybridCache.SeedKeyProviders.Element;

/// <summary>
/// Provides seed keys for elements whose content types match the configured <see cref="CacheSettings.ContentTypeKeys"/>.
/// </summary>
internal sealed class ElementContentTypeSeedKeyProvider : IElementSeedKeyProvider
{
    private readonly ICoreScopeProvider _scopeProvider;
    private readonly IDatabaseCacheRepository _databaseCacheRepository;
    private readonly IElementPublishStatusQueryService _publishStatusService;
    private readonly CacheSettings _cacheSettings;

    /// <summary>
    /// Initializes a new instance of the <see cref="ElementContentTypeSeedKeyProvider"/> class.
    /// </summary>
    /// <param name="scopeProvider">The scope provider for database operations.</param>
    /// <param name="databaseCacheRepository">The database cache repository.</param>
    /// <param name="cacheSettings">The cache settings containing the content type keys to seed.</param>
    /// <param name="publishStatusService">The publish status service for filtering unpublished elements.</param>
    public ElementContentTypeSeedKeyProvider(
        ICoreScopeProvider scopeProvider,
        IDatabaseCacheRepository databaseCacheRepository,
        IOptions<CacheSettings> cacheSettings,
        IElementPublishStatusQueryService publishStatusService)
    {
        _scopeProvider = scopeProvider;
        _databaseCacheRepository = databaseCacheRepository;
        _publishStatusService = publishStatusService;
        _cacheSettings = cacheSettings.Value;
    }

    /// <inheritdoc/>
    public ISet<Guid> GetSeedKeys()
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope();
        var elementKeys = _databaseCacheRepository
            .GetElementKeysByContentTypeKeys(_cacheSettings.ContentTypeKeys, published: true)
            .Where(key => _publishStatusService.IsPublishedInAnyCulture(key))
            .ToHashSet();
        scope.Complete();

        return elementKeys;
    }
}
