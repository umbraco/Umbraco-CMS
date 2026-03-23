using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Collections;
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
internal sealed class LanguageRepository : AsyncEntityRepositoryBase<Guid, ILanguage>, ILanguageRepository
{
    // Lock on _codeKeyMap for all map operations — the repository is a singleton.
    // Maps are rebuilt on PerformGetAllAsync and allow fast ISO code / Key lookups without deep-cloning.
    private readonly Dictionary<string, Guid> _codeKeyMap = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<Guid, string> _keyCodeMap = new();

    // Internal bridge map: ISO code to int ID. Only used for FK columns that still reference language by int.
    private readonly Dictionary<string, int> _codeIdMap = new(StringComparer.OrdinalIgnoreCase);

    private CancellationToken CancellationToken => CancellationToken.None;

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

    private AsyncFullDataSetRepositoryCachePolicy<ILanguage, Guid>? TypedCachePolicy =>
        CachePolicy as AsyncFullDataSetRepositoryCachePolicy<ILanguage, Guid>;

    protected override Guid GetEntityKey(ILanguage entity) => entity.Key;

    public async Task<ILanguage?> GetByIsoCodeAsync(string isoCode)
    {
        await EnsureCacheIsPopulatedAsync();

        Guid? key = await GetKeyByIsoCodeAsync(isoCode, false);
        return key.HasValue ? await GetAsync(key.Value, CancellationToken) : null;
    }

    public async Task<Guid?> GetKeyByIsoCodeAsync(string? isoCode, bool throwOnNotFound = true)
    {
        if (isoCode == null)
        {
            return null;
        }

        await EnsureMapsPopulatedAsync();

        lock (_codeKeyMap)
        {
            if (_codeKeyMap.TryGetValue(isoCode, out Guid key))
            {
                return key;
            }
        }

        if (throwOnNotFound)
        {
            throw new ArgumentException($"Code {isoCode} does not correspond to an existing language.", nameof(isoCode));
        }

        return null;
    }

    public async Task<string?> GetIsoCodeByKeyAsync(Guid? key, bool throwOnNotFound = true)
    {
        if (key == null)
        {
            return null;
        }

        await EnsureMapsPopulatedAsync();

        lock (_codeKeyMap)
        {
            if (_keyCodeMap.TryGetValue(key.Value, out var isoCode))
            {
                return isoCode;
            }
        }

        if (throwOnNotFound)
        {
            throw new ArgumentException($"Key {key} does not correspond to an existing language.", nameof(key));
        }

        return null;
    }

    public async Task<string[]> GetIsoCodesByKeysAsync(ICollection<Guid> keys, bool throwOnNotFound = true)
    {
        var isoCodes = new string[keys.Count];

        if (keys.Any() == false)
        {
            return isoCodes;
        }

        await EnsureMapsPopulatedAsync();

        lock (_codeKeyMap)
        {
            for (var i = 0; i < keys.Count; i++)
            {
                Guid key = keys.ElementAt(i);
                if (_keyCodeMap.TryGetValue(key, out var isoCode))
                {
                    isoCodes[i] = isoCode;
                }
                else if (throwOnNotFound)
                {
                    throw new ArgumentException($"Key {key} does not correspond to an existing language.", nameof(key));
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

    public async Task<Guid?> GetDefaultKeyAsync()
    {
        ILanguage defaultLanguage = await GetDefaultAsync();
        return defaultLanguage.Key;
    }

    protected override IAsyncRepositoryCachePolicy<ILanguage, Guid> CreateCachePolicy() =>
        new AsyncFullDataSetRepositoryCachePolicy<ILanguage, Guid>(GlobalIsolatedCache, ScopeAccessor,
            RepositoryCacheVersionService, CacheSyncService, GetEntityKey, /*expires:*/ false);

    /// <summary>
    /// Converts a DTO to a domain entity. Uses the navigation property for fallback resolution,
    /// so the DTO must be loaded with <c>.Include(x => x.FallbackLanguage)</c>.
    /// </summary>
    private static ILanguage ConvertFromDto(LanguageDto dto) =>
        LanguageFactory.BuildEntity(dto, dto.FallbackLanguage?.IsoCode);

    // do NOT leak that language, it's not deep-cloned!
    private async Task<ILanguage> GetDefaultAsync()
    {
        // get all cached
        // Try to get all cached non-cloned if using the correct cache policy (not the case in unit tests)
        List<ILanguage> languages = TypedCachePolicy is not null
            ? (await TypedCachePolicy.GetAllCachedAsync(PerformGetAllAsync)).ToList()
            : (await CachePolicy.GetAllAsync(PerformGetAllAsync)).ToList();

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
            if (first == null || l.Key < first.Key)
            {
                first = l;
            }
        }

        return first!;
    }

    #region Overrides of RepositoryBase<int,Language>

    protected override async Task<ILanguage?> PerformGetAsync(Guid key) =>
        (await PerformGetManyAsync([key]))?.FirstOrDefault();

    protected override async Task<IEnumerable<ILanguage>?> PerformGetAllAsync() =>
        await AmbientScope.ExecuteWithContextAsync(async db =>
        {
            List<LanguageDto> dtos = await db.Language
                .Include(x => x.FallbackLanguage)
                .OrderBy(x => x.Id)
                .ToListAsync();

            // Rebuild all maps from the full dataset
            lock (_codeKeyMap)
            {
                _codeKeyMap.Clear();
                _keyCodeMap.Clear();
                _codeIdMap.Clear();
                foreach (LanguageDto dto in dtos)
                {
                    if (dto.IsoCode != null)
                    {
                        _codeKeyMap[dto.IsoCode] = dto.LanguageKey;
                        _keyCodeMap[dto.LanguageKey] = dto.IsoCode;
                        _codeIdMap[dto.IsoCode] = dto.Id;
                    }
                }
            }

            return dtos
                .Select(ConvertFromDto)
                .WhereNotNull()
                .AsEnumerable();
        });

    protected override async Task<IEnumerable<ILanguage>?> PerformGetManyAsync(Guid[]? keys)
    {
        if (keys is null)
        {
            return null;
        }

        return await AmbientScope.ExecuteWithContextAsync(async db =>
        {
            List<LanguageDto> dtos = await db.Language
                .Include(x => x.FallbackLanguage)
                .Where(x => keys.Contains(x.LanguageKey))
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
            entity.Key = dto.LanguageKey;
            entity.ResetDirtyProperties();

            lock (_codeKeyMap)
            {
                _codeKeyMap[entity.IsoCode] = entity.Key;
                _keyCodeMap[entity.Key] = entity.IsoCode;
                _codeIdMap[entity.IsoCode] = entity.Id;
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
                var defaultKey = await db.Language
                    .Where(x => x.IsDefault)
                    .Select(x => x.LanguageKey)
                    .FirstOrDefaultAsync();

                if (entity.Key == defaultKey)
                {
                    throw new InvalidOperationException(
                        $"Cannot save the default language ({entity.IsoCode}) as non-default. Make another language the default language instead.");
                }
            }

            if (entity.IsPropertyDirty(nameof(ILanguage.IsoCode)))
            {
                var countOfSameCode = await db.Language
                    .CountAsync(x => x.IsoCode == entity.IsoCode && x.LanguageKey != entity.Key);

                if (countOfSameCode > 0)
                {
                    throw new InvalidOperationException(
                        $"Cannot update the language to a new culture: {entity.IsoCode} since that culture is already assigned to another language entity.");
                }
            }

            // fallback cycles are detected at service level

            // update — LanguageKey is immutable, not included in SetProperty
            LanguageDto dto = LanguageFactory.BuildDto(entity, GetFallbackLanguageId(entity));
            await db.UpsertAsync(dto, () =>
                db.Language
                    .Where(x => x.LanguageKey == dto.LanguageKey)
                    .ExecuteUpdateAsync(setter => setter
                        .SetProperty(x => x.IsoCode, dto.IsoCode)
                        .SetProperty(x => x.CultureName, dto.CultureName)
                        .SetProperty(x => x.IsDefault, dto.IsDefault)
                        .SetProperty(x => x.IsMandatory, dto.IsMandatory)
                        .SetProperty(x => x.FallbackLanguageId, dto.FallbackLanguageId)));

            entity.ResetDirtyProperties();

            lock (_codeKeyMap)
            {
                _codeKeyMap.RemoveAll(kvp => kvp.Value == entity.Key);
                _codeKeyMap[entity.IsoCode] = entity.Key;
                _keyCodeMap[entity.Key] = entity.IsoCode;
                _codeIdMap.RemoveAll(kvp => kvp.Value == entity.Id);
                _codeIdMap[entity.IsoCode] = entity.Id;
            }
        });

    /// <inheritdoc/>
    protected override async Task PersistDeletedItemAsync(ILanguage entity) =>
        await AmbientScope.ExecuteWithContextAsync<LanguageDto>(async db =>
        {
            var defaultKey = await db.Language
                .Where(x => x.IsDefault)
                .Select(x => x.LanguageKey)
                .FirstOrDefaultAsync();

            if (entity.Key == defaultKey)
            {
                throw new InvalidOperationException($"Cannot delete the default language ({entity.IsoCode}).");
            }

            // We need to remove any references to the language if it's being used as a fall-back from other ones
            // FallbackLanguageId is an int FK — must use entity.Id here
            await db.Language
                .Where(x => x.FallbackLanguageId == entity.Id)
                .ExecuteUpdateAsync(setter => setter
                    .SetProperty(x => x.FallbackLanguageId, (int?)null));

            // DeleteReferences uses int ID for raw SQL against tables with int languageId FKs
            await DeleteReferences(entity.Id, db);

            // delete the language itself
            await db.Language
                .Where(x => x.LanguageKey == entity.Key)
                .ExecuteDeleteAsync();

            lock (_codeKeyMap)
            {
                _codeKeyMap.RemoveAll(kvp => kvp.Value == entity.Key);
                _keyCodeMap.Remove(entity.Key);
                _codeIdMap.RemoveAll(kvp => kvp.Value == entity.Id);
            }
        });

    /// <summary>
    /// Ensures the in-memory maps are populated.
    /// If the maps already contain data, returns immediately without requiring an EF Core scope.
    /// This allows NPoco code paths to perform ISO code/Key lookups
    /// without needing an ambient EF Core scope.
    /// </summary>
    private async Task EnsureMapsPopulatedAsync()
    {
        lock (_codeKeyMap)
        {
            if (_codeKeyMap.Count > 0)
            {
                return;
            }
        }

        // Try to populate maps from cached entities.
        if (TryPopulateMapsFromCache())
        {
            return;
        }

        // Fall back to loading from DB.
        // PerformGetAllAsync uses ExecuteWithContextAsync which creates a scope if needed.
        await EnsureCacheIsPopulatedAsync();
    }

    /// <summary>
    /// Attempts to rebuild the in-memory maps from cached <see cref="ILanguage"/> entities
    /// without requiring a database round-trip or an ambient EF Core scope.
    /// </summary>
    private bool TryPopulateMapsFromCache()
    {
        DeepCloneableList<ILanguage>? cached = GlobalIsolatedCache
            .GetCacheItem<DeepCloneableList<ILanguage>>(RepositoryCacheKeys.GetKey<ILanguage>());

        if (cached is null || cached.Count == 0)
        {
            return false;
        }

        lock (_codeKeyMap)
        {
            if (_codeKeyMap.Count > 0)
            {
                return true;
            }

            foreach (ILanguage language in cached)
            {
                if (language.IsoCode is not null)
                {
                    _codeKeyMap[language.IsoCode] = language.Key;
                    _keyCodeMap[language.Key] = language.IsoCode;
                    _codeIdMap[language.IsoCode] = language.Id;
                }
            }
        }

        return _codeKeyMap.Count > 0;
    }

    private async Task EnsureCacheIsPopulatedAsync()
    {
        // ensure cache is populated, in a non-expensive way
        if (TypedCachePolicy != null)
        {
            await TypedCachePolicy.GetAllCachedAsync(PerformGetAllAsync);
        }
        else
        {
            await PerformGetAllAsync(); // We don't have a typed cache (i.e. unit tests) but need to populate maps
        }
    }

    /// <summary>
    /// Resolves the int ID of a fallback language from its ISO code.
    /// Uses the internal <c>_codeIdMap</c> which is populated on cache load.
    /// This is needed because the FallbackLanguageId column is still an int FK.
    /// </summary>
    private int? GetFallbackLanguageId(ILanguage entity)
    {
        if (entity.FallbackIsoCode.IsNullOrWhiteSpace() == false &&
            _codeIdMap.TryGetValue(entity.FallbackIsoCode, out var languageId))
        {
            return languageId;
        }

        return null;
    }

    /// <summary>
    /// Clean up all referencing tables before deleting the language.
    /// These tables have FKs to the Language table without cascade delete.
    /// When these tables get migrated to EF Core, they should use OnDelete(DeleteBehavior.Cascade) instead.
    ///
    /// This solution is temporary, and should be removed when this repository is no longer called from NPoco contexts.
    /// </summary>
    private async Task DeleteReferences(int id, UmbracoDbContext db)
    {
        async Task DeleteByLanguageId(string tableName) =>
            await db.Database.ExecuteSqlRawAsync($"DELETE FROM \"{tableName}\" WHERE \"languageId\" = {{0}}", id);

        await DeleteByLanguageId(Constants.DatabaseSchema.Tables.DictionaryValue);
        await DeleteByLanguageId(Constants.DatabaseSchema.Tables.PropertyData);
        await DeleteByLanguageId(Constants.DatabaseSchema.Tables.ContentVersionCultureVariation);
        await DeleteByLanguageId(Constants.DatabaseSchema.Tables.DocumentCultureVariation);
        await DeleteByLanguageId(Constants.DatabaseSchema.Tables.ElementCultureVariation);
        await DeleteByLanguageId(Constants.DatabaseSchema.Tables.ContentSchedule);
        await db.Database.ExecuteSqlRawAsync($"DELETE FROM \"{Constants.DatabaseSchema.Tables.TagRelationship}\" WHERE \"tagId\" IN (SELECT id FROM \"{Constants.DatabaseSchema.Tables.Tag}\" WHERE \"languageId\" = {{0}})", id);
        await DeleteByLanguageId(Constants.DatabaseSchema.Tables.Tag);
        await DeleteByLanguageId(Constants.DatabaseSchema.Tables.DocumentUrl);
        await DeleteByLanguageId(Constants.DatabaseSchema.Tables.DocumentUrlAlias);
        await DeleteByLanguageId(Constants.DatabaseSchema.Tables.UserGroup2Language);
    }

    #endregion
}
