using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Infrastructure.HybridCache;

/// <summary>
///     Implements <see cref="IDomainCache" /> for NuCache.
/// </summary>
public class DomainCache : IDomainCache
{
    private readonly IDomainCacheService _domainCacheService;
    private readonly IDefaultCultureAccessor _defaultCultureAccessor;
    private readonly IRuntimeState _runtimeState;

#pragma warning disable IDE0032 // Use auto property - does not apply here: _defaultCulture conditionally caches the
                                // result of the computed DefaultCulture getter rather than acting as its backing
                                // field, so it cannot be folded into an auto-property.
    private string? _defaultCulture;
#pragma warning restore IDE0032 // Use auto property

    /// <summary>
    ///     Initializes a new instance of the <see cref="DomainCache" /> class.
    /// </summary>
    [Obsolete("Please use the constructor taking all parameters. Scheduled for removal in Umbraco 19.")]
    public DomainCache(IDefaultCultureAccessor defaultCultureAccessor, IDomainCacheService domainCacheService)
        : this(
            defaultCultureAccessor,
            domainCacheService,
            StaticServiceProvider.Instance.GetRequiredService<IRuntimeState>())
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="DomainCache" /> class.
    /// </summary>
    public DomainCache(
        IDefaultCultureAccessor defaultCultureAccessor,
        IDomainCacheService domainCacheService,
        IRuntimeState runtimeState)
    {
        _defaultCultureAccessor = defaultCultureAccessor;
        _domainCacheService = domainCacheService;
        _runtimeState = runtimeState;
    }

    /// <inheritdoc />
    /// <remarks>
    ///     Only retained once the runtime reaches <see cref="RuntimeLevel.Run" />. This is a singleton that can be
    ///     constructed earlier, where <see cref="IDefaultCultureAccessor" /> reports a configured fallback, and
    ///     retaining that left content unroutable until restart (https://github.com/umbraco/Umbraco-CMS/issues/22581).
    /// </remarks>
    public string DefaultCulture
    {
        get
        {
            if (_defaultCulture is not null)
            {
                return _defaultCulture;
            }

            var defaultCulture = _defaultCultureAccessor.DefaultCulture;

            if (_runtimeState.Level == RuntimeLevel.Run && string.IsNullOrEmpty(defaultCulture) is false)
            {
                _defaultCulture = defaultCulture;
            }

            return defaultCulture;
        }
    }

    /// <inheritdoc />
    public IEnumerable<Domain> GetAll(bool includeWildcards) => _domainCacheService.GetAll(includeWildcards);

    /// <inheritdoc />
    public IEnumerable<Domain> GetAssigned(int documentId, bool includeWildcards = false) => _domainCacheService.GetAssigned(documentId, includeWildcards);

    /// <inheritdoc />
    public bool HasAssigned(int documentId, bool includeWildcards = false) => _domainCacheService.HasAssigned(documentId, includeWildcards);
}
