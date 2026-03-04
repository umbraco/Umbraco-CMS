using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Infrastructure.Cache;
using Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore;
using Umbraco.Cms.Infrastructure.Persistence.EFCore;
using Umbraco.Cms.Infrastructure.Persistence.EFCore.Scoping;
using Umbraco.Cms.Infrastructure.Persistence.Factories;
using Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement.EFCore;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

/// <summary>
///     Represents a repository for doing CRUD operations for <see cref="Language" />
/// </summary>
internal sealed class LanguageRepository : AsyncEntityRepositoryBase<int, ILanguage>, ILanguageRepository
{
    // We need to lock this dictionary every time we do an operation on it as the languageRepository is registered as a unique implementation
    // It is used to quickly get isoCodes by Id, or the reverse by avoiding (deep)cloning dtos
    // It is rebuild on PerformGetAll
    private readonly Dictionary<string, int> _codeIdMap = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<int, string> _idCodeMap = new();

    private CancellationToken cancellationToken => CancellationToken.None;

    public LanguageRepository(
        IEFCoreScopeAccessor<UmbracoDbContext> scopeAccessor,
        AppCaches cache,
        ILogger<LanguageRepository> logger,
        IRepositoryCacheVersionService repositoryCacheVersionService,
        ICacheSyncService cacheSyncService)
        : base(
            scopeAccessor,
            cache,
            logger,
            repositoryCacheVersionService,
            cacheSyncService)
    {
    }

    private AsyncFullDataSetRepositoryCachePolicy<ILanguage, int>? TypedCachePolicy =>
        CachePolicy as AsyncFullDataSetRepositoryCachePolicy<ILanguage, int>;

    public async Task<ILanguage?> GetByIsoCodeAsync(string isoCode)
    {
        await EnsureCacheIsPopulatedAsync();

        var id = await GetIdByIsoCodeAsync(isoCode, false);
        return id.HasValue ? await GetAsync(id.Value, cancellationToken) : null;
    }

    public async Task<int?> GetIdByIsoCodeAsync(string? isoCode, bool throwOnNotFound = true)
    {
        if (isoCode == null)
        {
            return null;
        }

        await EnsureCacheIsPopulatedAsync();

        lock (_codeIdMap)
        {
            if (_codeIdMap.TryGetValue(isoCode, out var id))
            {
                return id;
            }
        }

        if (throwOnNotFound)
        {
            throw new ArgumentException($"Code {isoCode} does not correspond to an existing language.", nameof(isoCode));
        }

        return null;
    }

    public async Task<string?> GetIsoCodeByIdAsync(int? id, bool throwOnNotFound = true)
    {
        if (id == null)
        {
            return null;
        }

        await EnsureCacheIsPopulatedAsync();

        lock (_codeIdMap)
        {
            if (_idCodeMap.TryGetValue(id.Value, out var isoCode))
            {
                return isoCode;
            }
        }

        if (throwOnNotFound)
        {
            throw new ArgumentException($"Id {id} does not correspond to an existing language.", nameof(id));
        }

        return null;
    }

    // multi implementation of GetIsoCodeById
    public async Task<string[]> GetIsoCodesByIdsAsync(ICollection<int> ids, bool throwOnNotFound = true)
    {
        var isoCodes = new string[ids.Count];

        if (ids.Any() == false)
        {
            return isoCodes;
        }

        await EnsureCacheIsPopulatedAsync();


        lock (_codeIdMap)
        {
            for (var i = 0; i < ids.Count; i++)
            {
                var id = ids.ElementAt(i);
                if (_idCodeMap.TryGetValue(id, out var isoCode))
                {
                    isoCodes[i] = isoCode;
                }
                else if (throwOnNotFound)
                {
                    throw new ArgumentException($"Id {id} does not correspond to an existing language.", nameof(id));
                }
            }
        }

        return isoCodes;
    }

    public async Task<string> GetDefaultIsoCodeAsync()
    {
        ILanguage defaultLanguage = await GetDefaultAsync();
        return defaultLanguage.IsoCode;
    }

    public async Task<int?> GetDefaultIdAsync()
    {
        ILanguage defaultLanguage = await GetDefaultAsync();
        return defaultLanguage.Id;
    }

    protected override IAsyncRepositoryCachePolicy<ILanguage, int> CreateCachePolicy() =>
        new AsyncFullDataSetRepositoryCachePolicy<ILanguage, int>(GlobalIsolatedCache, ScopeAccessor,
            RepositoryCacheVersionService, CacheSyncService, GetEntityId, /*expires:*/ false);

    private ILanguage ConvertFromDto(LanguageDto dto)
    {
        lock (_codeIdMap)
        {
            string? fallbackIsoCode = null;
            if (dto.FallbackLanguageId.HasValue && _idCodeMap.TryGetValue(dto.FallbackLanguageId.Value, out fallbackIsoCode) == false)
            {
                throw new ArgumentException($"The ISO code map did not contain ISO code for fallback language ID: {dto.FallbackLanguageId}. Please reload the caches.");
            }

            return LanguageFactory.BuildEntity(dto, fallbackIsoCode);
        }
    }

    // do NOT leak that language, it's not deep-cloned!
    private async Task<ILanguage> GetDefaultAsync()
    {
        // get all cached
        var languages =
            (await TypedCachePolicy
                     ?.GetAllCachedAsync(PerformGetAllAsync)
                 ! // Try to get all cached non-cloned if using the correct cache policy (not the case in unit tests)
             ?? await CachePolicy.GetAllAsync(PerformGetAllAsync)).ToList();

        ILanguage? language = languages.FirstOrDefault(x => x.IsDefault);
        if (language != null)
        {
            return language;
        }

        // this is an anomaly, the service/repo should ensure it cannot happen
        Logger.LogWarning(
            "There is no default language. Fix this anomaly by editing the language table in database and setting one language as the default language.");

        // still, don't kill the site, and return "something"
        ILanguage? first = null;
        foreach (ILanguage l in languages)
        {
            if (first == null || l.Id < first.Id)
            {
                first = l;
            }
        }

        return first!;
    }

    #region Overrides of RepositoryBase<int,Language>

    protected override async Task<ILanguage?> PerformGetAsync(int id) =>
        (await PerformGetManyAsync([id]))?.FirstOrDefault();

    protected override async Task<IEnumerable<ILanguage>?> PerformGetAllAsync() =>
        await AmbientScope.ExecuteWithContextAsync(async db =>
        {
            List<LanguageDto> dtos = await db.Language
                .OrderBy(x => x.Id)
                .ToListAsync();

            return dtos
                .Select(ConvertFromDto)
                .WhereNotNull()
                .AsEnumerable();
        });

    protected override async Task<IEnumerable<ILanguage>?> PerformGetManyAsync(int[]? ids)
    {
        if (ids is null)
        {
            return null;
        }

        return await AmbientScope.ExecuteWithContextAsync(async db =>
        {
            List<LanguageDto> dtos = await db.Language
                .Where(x => ids.Contains(x.Id))
                .OrderBy(x => x.Id)
                .ToListAsync();

            return dtos
                .Select(ConvertFromDto)
                .WhereNotNull()
                .AsEnumerable();
        });
    }

    #endregion

    #region Unit of Work Implementation

    /// <inheritdoc/>
    protected override async Task PersistNewItemAsync(ILanguage entity) =>
        await AmbientScope.ExecuteWithContextAsync<LanguageDto>(async db =>
        {
            // validate iso code and culture name
            if (entity.IsoCode.IsNullOrWhiteSpace() || entity.CultureName.IsNullOrWhiteSpace())
            {
                throw new InvalidOperationException("Cannot save a language without an ISO code and a culture name.");
            }

            await EnsureCacheIsPopulatedAsync();

            entity.AddingEntity();

            // deal with entity becoming the new default entity
            if (entity.IsDefault)
            {
                // set all other entities to non-default
                // safe (no race cond) because the service locks languages
                await db.Language
                    .ExecuteUpdateAsync(
                        setter => setter
                        .SetProperty(x => x.IsDefault, false));
            }

            // fallback cycles are detected at service level

            // insert
            LanguageDto dto = LanguageFactory.BuildDto(entity, GetFallbackLanguageId(entity));
            await db.Language.AddAsync(dto);
            await db.SaveChangesAsync();

            entity.Id = dto.Id;
            entity.ResetDirtyProperties();

            // yes, we want to lock _codeIdMap
            lock (_codeIdMap)
            {
                _codeIdMap[entity.IsoCode] = entity.Id;
                _idCodeMap[entity.Id] = entity.IsoCode;
            }
        });

    /// <inheritdoc/>
    protected override async Task PersistUpdatedItemAsync(ILanguage entity) =>
        await AmbientScope.ExecuteWithContextAsync<LanguageDto>(async db =>
        {
            // validate iso code and culture name
            if (entity.IsoCode.IsNullOrWhiteSpace() || entity.CultureName.IsNullOrWhiteSpace())
            {
                throw new InvalidOperationException("Cannot save a language without an ISO code and a culture name.");
            }

            await EnsureCacheIsPopulatedAsync();

            entity.UpdatingEntity();

            if (entity.IsDefault)
            {
                // deal with entity becoming the new default entity

                // set all other entities to non-default
                // safe (no race cond) because the service locks languages
                await db.Language
                    .ExecuteUpdateAsync(
                        setter => setter
                            .SetProperty(x => x.IsDefault, false));
            }
            else
            {
                var defaultId = await db.Language
                    .Where(x => x.IsDefault)
                    .Select(x => x.Id)
                    .FirstOrDefaultAsync();

                if (entity.Id == defaultId)
                {
                    throw new InvalidOperationException(
                        $"Cannot save the default language ({entity.IsoCode}) as non-default. Make another language the default language instead.");
                }
            }

            if (entity.IsPropertyDirty(nameof(ILanguage.IsoCode)))
            {
                var countOfSameCode = await db.Language
                    .CountAsync(x => x.IsoCode == entity.IsoCode && x.Id != entity.Id);

                if (countOfSameCode > 0)
                {
                    throw new InvalidOperationException(
                        $"Cannot update the language to a new culture: {entity.IsoCode} since that culture is already assigned to another language entity.");
                }
            }

            // fallback cycles are detected at service level

            // update
            LanguageDto dto = LanguageFactory.BuildDto(entity, GetFallbackLanguageId(entity));
            await db.UpsertAsync(dto, () =>
                db.Language
                    .Where(x => x.Id == dto.Id)
                    .ExecuteUpdateAsync(setter => setter
                        .SetProperty(x => x.IsoCode, dto.IsoCode)
                        .SetProperty(x => x.CultureName, dto.CultureName)
                        .SetProperty(x => x.IsDefault, dto.IsDefault)
                        .SetProperty(x => x.IsMandatory, dto.IsMandatory)
                        .SetProperty(x => x.FallbackLanguageId, dto.FallbackLanguageId)));

            entity.ResetDirtyProperties();

            // yes, we want to lock _codeIdMap
            lock (_codeIdMap)
            {
                _codeIdMap.RemoveAll(kvp => kvp.Value == entity.Id);
                _codeIdMap[entity.IsoCode] = entity.Id;
                _idCodeMap[entity.Id] = entity.IsoCode;
            }
        });

    /// <inheritdoc/>
    protected override async Task PersistDeletedItemAsync(ILanguage entity) =>
        await AmbientScope.ExecuteWithContextAsync<LanguageDto>(async db =>
        {
            var defaultId = await db.Language
                .Where(x => x.IsDefault)
                .Select(x => x.Id)
                .FirstOrDefaultAsync();

            if (entity.Id == defaultId)
            {
                throw new InvalidOperationException($"Cannot delete the default language ({entity.IsoCode}).");
            }

            // We need to remove any references to the language if it's being used as a fall-back from other ones
            await db.Language
                .Where(x => x.FallbackLanguageId == entity.Id)
                .ExecuteUpdateAsync(setter => setter
                    .SetProperty(x => x.FallbackLanguageId, (int?)null));

            // delete
            await base.PersistDeletedItemAsync(entity);

            // yes, we want to lock _codeIdMap
            lock (_codeIdMap)
            {
                _codeIdMap.RemoveAll(kvp => kvp.Value == entity.Id);
                _idCodeMap.Remove(entity.Id);
            }
        });

    private async Task EnsureCacheIsPopulatedAsync()
    {
        // ensure cache is populated, in a non-expensive way
        if (TypedCachePolicy != null)
        {
            await TypedCachePolicy.GetAllCachedAsync(PerformGetAllAsync);
        }
        else
        {
            await PerformGetAllAsync(); // We don't have a typed cache (i.e. unit tests) but need to populate the _codeIdMap
        }
    }

    private int? GetFallbackLanguageId(ILanguage entity)
    {
        int? fallbackLanguageId = null;
        if (entity.FallbackIsoCode.IsNullOrWhiteSpace() == false &&
            _codeIdMap.TryGetValue(entity.FallbackIsoCode, out var languageId))
        {
            fallbackLanguageId = languageId;
        }

        return fallbackLanguageId;
    }

    #endregion
}
