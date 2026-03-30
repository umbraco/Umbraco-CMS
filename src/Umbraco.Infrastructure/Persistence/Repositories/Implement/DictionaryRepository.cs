using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore;
using Umbraco.Cms.Infrastructure.Persistence.EFCore;
using Umbraco.Cms.Infrastructure.Persistence.EFCore.Scoping;
using Umbraco.Cms.Infrastructure.Persistence.Factories;
using Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement.EFCore;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

/// <summary>
///     Represents a repository for doing CRUD operations for <see cref="DictionaryItem" />
/// </summary>
internal sealed class DictionaryRepository : AsyncEntityRepositoryBase<Guid, IDictionaryItem>, IDictionaryRepository
{
    private readonly ILanguageRepository _languageRepository;
    private readonly IOptionsMonitor<DictionarySettings> _dictionarySettings;

    /// <summary>
    /// Initializes a new instance of the <see cref="DictionaryRepository"/> class.
    /// </summary>
    /// <param name="scopeAccessor">Provides access to the database scope for context management.</param>
    /// <param name="cache">The application-level cache manager.</param>
    /// <param name="logger">The logger used for logging repository operations.</param>
    /// <param name="languageRepository">Repository for managing language entities.</param>
    /// <param name="repositoryCacheVersionService">Service for managing repository cache versions.</param>
    /// <param name="cacheSyncService">Service for synchronizing cache across distributed environments.</param>
    /// <param name="dictionarySettings">Monitors and provides access to dictionary-related settings.</param>
    public DictionaryRepository(
        IEFCoreScopeAccessor<UmbracoDbContext> scopeAccessor,
        AppCaches cache,
        ILogger<DictionaryRepository> logger,
        ILanguageRepository languageRepository,
        IRepositoryCacheVersionService repositoryCacheVersionService,
        ICacheSyncService cacheSyncService,
        IOptionsMonitor<DictionarySettings> dictionarySettings)
        : base(scopeAccessor, cache, logger, repositoryCacheVersionService, cacheSyncService)
    {
        _languageRepository = languageRepository;
        _dictionarySettings = dictionarySettings;
    }

    protected override Guid GetEntityKey(IDictionaryItem entity) => entity.Key;

    #region Interface methods

    /// <summary>
    /// Gets a dictionary item by its string key.
    /// </summary>
    /// <param name="key">The string key of the dictionary item.</param>
    /// <returns>The dictionary item if found; otherwise, <c>null</c>.</returns>
    public async Task<IDictionaryItem?> GetByItemKeyAsync(string key)
    {
        IDictionary<int, ILanguage> languagesById = await GetLanguagesByIdAsync();

        return await AmbientScope.ExecuteWithContextAsync(async db =>
        {
            DictionaryDto? dto = await db.DictionaryEntries
                .Include(x => x.LanguageTextDtos)
                .Where(x => x.Key == key)
                .FirstOrDefaultAsync();

            return dto == null ? null : ConvertFromDto(dto, languagesById);
        });
    }

    /// <summary>
    /// Gets multiple dictionary items by their string keys.
    /// </summary>
    /// <param name="keys">The string keys of the dictionary items.</param>
    /// <returns>A collection of dictionary items matching the specified keys.</returns>
    public async Task<IEnumerable<IDictionaryItem>> GetManyByItemKeysAsync(params string[] keys)
    {
        if (keys.Length == 0)
        {
            return Array.Empty<IDictionaryItem>();
        }

        IDictionary<int, ILanguage> languagesById = await GetLanguagesByIdAsync();

        return await AmbientScope.ExecuteWithContextAsync(async db =>
        {
            List<DictionaryDto> dtos = await db.DictionaryEntries
                .Include(x => x.LanguageTextDtos)
                .Where(x => keys.Contains(x.Key))
                .ToListAsync();

            return dtos.Select(dto => ConvertFromDto(dto, languagesById)).ToList();
        });
    }

    /// <summary>
    /// Gets a dictionary mapping of dictionary item keys to their unique identifiers.
    /// </summary>
    /// <returns>A dictionary where the key is the dictionary item key and the value is the corresponding unique identifier.</returns>
    public async Task<Dictionary<string, Guid>> GetDictionaryItemKeyMapAsync() =>
        await AmbientScope.ExecuteWithContextAsync(async db =>
            await db.DictionaryEntries
                .Select(x => new { x.Key, x.UniqueId })
                .ToDictionaryAsync(x => x.Key, x => x.UniqueId));

    /// <summary>
    /// Retrieves all descendant dictionary items of a specified parent item, optionally filtered by a search string.
    /// </summary>
    /// <param name="parentId">The unique identifier of the parent dictionary item from which to retrieve descendants.
    /// If <c>null</c>, all dictionary items in the repository are returned.</param>
    /// <param name="filter">An optional string to filter the dictionary items by key or value.</param>
    /// <returns>A collection of descendant dictionary items ordered by key.</returns>
    public async Task<IEnumerable<IDictionaryItem>> GetDictionaryItemDescendantsAsync(Guid? parentId, string? filter = null)
    {
        IDictionary<int, ILanguage> languagesById = await GetLanguagesByIdAsync();

        if (!parentId.HasValue)
        {
            // Return all dictionary items (with optional filter)
            return await AmbientScope.ExecuteWithContextAsync(async db =>
            {
                IQueryable<DictionaryDto> query = db.DictionaryEntries
                    .Include(x => x.LanguageTextDtos);

                query = ApplyFilter(query, filter);

                List<DictionaryDto> dtos = await query.ToListAsync();

                return dtos
                    .Select(dto => ConvertFromDto(dto, languagesById))
                    .OrderBy(x => x.ItemKey)
                    .ToList();
            });
        }

        // Recursive descent: load children at each level
        return await AmbientScope.ExecuteWithContextAsync(async db =>
        {
            var allDescendants = new List<IDictionaryItem>();
            var currentParentIds = new[] { parentId.Value };

            while (currentParentIds.Length > 0)
            {
                var nextLevelItems = new List<IDictionaryItem>();
                var nextLevelParentIds = new List<Guid>();

                // Process in groups to avoid exceeding SQL parameter limits
                foreach (IEnumerable<Guid> group in currentParentIds.InGroupsOf(Constants.Sql.MaxParameterCount))
                {
                    Guid[] groupArray = group.ToArray();

                    IQueryable<DictionaryDto> query = db.DictionaryEntries
                        .Include(x => x.LanguageTextDtos)
                        .Where(x => x.Parent != null && groupArray.Contains(x.Parent.Value));

                    query = ApplyFilter(query, filter);

                    List<DictionaryDto> dtos = await query.ToListAsync();

                    foreach (DictionaryDto dto in dtos)
                    {
                        nextLevelItems.Add(ConvertFromDto(dto, languagesById));
                        nextLevelParentIds.Add(dto.UniqueId);
                    }
                }

                allDescendants.AddRange(nextLevelItems);
                currentParentIds = nextLevelParentIds.ToArray();
            }

            return allDescendants.OrderBy(x => x.ItemKey).ToList();
        });
    }

    #endregion

    #region Overrides of AsyncEntityRepositoryBase

    protected override async Task<IDictionaryItem?> PerformGetAsync(Guid key)
    {
        IDictionary<int, ILanguage> languagesById = await GetLanguagesByIdAsync();

        return await AmbientScope.ExecuteWithContextAsync(async db =>
        {
            DictionaryDto? dto = await db.DictionaryEntries
                .Include(x => x.LanguageTextDtos)
                .Where(x => x.UniqueId == key)
                .FirstOrDefaultAsync();

            if (dto == null)
            {
                return null;
            }

            IDictionaryItem entity = ConvertFromDto(dto, languagesById);
            ((EntityBase)entity).ResetDirtyProperties(false);
            return entity;
        });
    }

    protected override async Task<IEnumerable<IDictionaryItem>?> PerformGetAllAsync()
    {
        IDictionary<int, ILanguage> languagesById = await GetLanguagesByIdAsync();

        return await AmbientScope.ExecuteWithContextAsync(async db =>
        {
            List<DictionaryDto> dtos = await db.DictionaryEntries
                .Include(x => x.LanguageTextDtos)
                .OrderBy(x => x.UniqueId)
                .ToListAsync();

            return dtos
                .Select(dto => ConvertFromDto(dto, languagesById))
                .AsEnumerable();
        });
    }

    protected override async Task<IEnumerable<IDictionaryItem>?> PerformGetManyAsync(Guid[]? keys)
    {
        if (keys is null)
        {
            return null;
        }

        IDictionary<int, ILanguage> languagesById = await GetLanguagesByIdAsync();

        return await AmbientScope.ExecuteWithContextAsync(async db =>
        {
            List<DictionaryDto> dtos = await db.DictionaryEntries
                .Include(x => x.LanguageTextDtos)
                .Where(x => keys.Contains(x.UniqueId))
                .OrderBy(x => x.UniqueId)
                .ToListAsync();

            return dtos
                .Select(dto => ConvertFromDto(dto, languagesById))
                .AsEnumerable();
        });
    }

    protected override async Task<bool> PerformExistsAsync(Guid key) =>
        await AmbientScope.ExecuteWithContextAsync(async db =>
            await db.DictionaryEntries.AnyAsync(x => x.UniqueId == key));

    #endregion

    #region Unit of Work Implementation

    protected override async Task PersistNewItemAsync(IDictionaryItem entity) =>
        await AmbientScope.ExecuteWithContextAsync<DictionaryDto>(async db =>
        {
            var dictionaryItem = (DictionaryItem)entity;

            dictionaryItem.AddingEntity();

            foreach (IDictionaryTranslation translation in dictionaryItem.Translations)
            {
                translation.Value = translation.Value.ToValidXmlString();
            }

            DictionaryDto dto = DictionaryItemFactory.BuildDto(dictionaryItem);

            await db.DictionaryEntries.AddAsync(dto);
            await db.SaveChangesAsync();

            dictionaryItem.Id = dto.PrimaryKey;

            IDictionary<string, ILanguage> languagesByIsoCode = await GetLanguagesByIsoCodeAsync();

            foreach (IDictionaryTranslation translation in dictionaryItem.Translations)
            {
                LanguageTextDto textDto = DictionaryTranslationFactory.BuildDto(translation, dictionaryItem.Key, languagesByIsoCode);

                await db.Set<LanguageTextDto>().AddAsync(textDto);
                await db.SaveChangesAsync();

                translation.Id = textDto.PrimaryKey;
                translation.Key = dictionaryItem.Key;
            }

            dictionaryItem.ResetDirtyProperties();
        });

    protected override async Task PersistUpdatedItemAsync(IDictionaryItem entity) =>
        await AmbientScope.ExecuteWithContextAsync<DictionaryDto>(async db =>
        {
            entity.UpdatingEntity();

            foreach (IDictionaryTranslation translation in entity.Translations)
            {
                translation.Value = translation.Value.ToValidXmlString();
            }

            // Update the dictionary entry itself
            DictionaryDto dto = DictionaryItemFactory.BuildDto(entity);
            await db.UpsertAsync(dto, () =>
                db.DictionaryEntries
                    .Where(x => x.UniqueId == dto.UniqueId)
                    .ExecuteUpdateAsync(setter => setter
                        .SetProperty(x => x.Key, dto.Key)
                        .SetProperty(x => x.Parent, dto.Parent)));

            IDictionary<string, ILanguage> languagesByIsoCode = await GetLanguagesByIsoCodeAsync();

            foreach (IDictionaryTranslation translation in entity.Translations)
            {
                LanguageTextDto textDto = DictionaryTranslationFactory.BuildDto(translation, entity.Key, languagesByIsoCode);

                if (translation.HasIdentity)
                {
                    // Update existing translation
                    await db.UpsertAsync(textDto, () =>
                        db.Set<LanguageTextDto>()
                            .Where(x => x.PrimaryKey == textDto.PrimaryKey)
                            .ExecuteUpdateAsync(setter => setter
                                .SetProperty(x => x.LanguageId, textDto.LanguageId)
                                .SetProperty(x => x.UniqueId, textDto.UniqueId)
                                .SetProperty(x => x.Value, textDto.Value)));
                }
                else
                {
                    // Insert new translation
                    await db.Set<LanguageTextDto>().AddAsync(textDto);
                    await db.SaveChangesAsync();
                    translation.Id = textDto.PrimaryKey;
                    translation.Key = entity.Key;
                }
            }

            entity.ResetDirtyProperties();

            // Clear the cache entries that exist by item key
            IsolatedCache.Clear(RepositoryCacheKeys.GetKey<IDictionaryItem, string>(entity.ItemKey));
        });

    protected override async Task PersistDeletedItemAsync(IDictionaryItem entity)
    {
        await AmbientScope.ExecuteWithContextAsync(async db =>
        {
            await RecursiveDeleteAsync(entity.Key, db);
            await DeleteEntityAsync(entity.Key, db);
            return true;
        });

        // Clear the cache entries that exist by item key
        IsolatedCache.Clear(RepositoryCacheKeys.GetKey<IDictionaryItem, string>(entity.ItemKey));

        entity.DeleteDate = DateTime.UtcNow;
    }

    #endregion

    #region Cache Policy

    protected override IAsyncRepositoryCachePolicy<IDictionaryItem, Guid> CreateCachePolicy()
        => new AsyncDefaultRepositoryCachePolicy<IDictionaryItem, Guid>(
            GlobalIsolatedCache,
            ScopeAccessor,
            DefaultOptions,
            RepositoryCacheVersionService,
            CacheSyncService);

    #endregion

    #region Private helpers

    /// <summary>
    /// Converts an EF Core DictionaryDto to an IDictionaryItem entity, including its translations.
    /// </summary>
    private static IDictionaryItem ConvertFromDto(DictionaryDto dto, IDictionary<int, ILanguage> languagesById)
    {
        IDictionaryItem entity = DictionaryItemFactory.BuildEntity(dto);

        entity.Translations = dto.LanguageTextDtos
            .Where(x => x.LanguageId > 0)
            .Select(x => languagesById.TryGetValue(x.LanguageId, out ILanguage? language)
                ? DictionaryTranslationFactory.BuildEntity(x, dto.UniqueId, language)
                : null)
            .WhereNotNull()
            .ToList();

        return entity;
    }

    /// <summary>
    /// Applies the filter to the EF Core query based on configuration settings.
    /// </summary>
    private IQueryable<DictionaryDto> ApplyFilter(IQueryable<DictionaryDto> query, string? filter)
    {
        if (filter.IsNullOrWhiteSpace())
        {
            return query;
        }

        if (_dictionarySettings.CurrentValue.EnableValueSearch)
        {
            // Search in both keys and values
            return query.Where(x =>
                x.Key.StartsWith(filter) ||
                x.LanguageTextDtos.Any(lt => EF.Functions.Like(lt.Value, "%" + filter + "%")));
        }

        // Search only in keys
        return query.Where(x => x.Key.StartsWith(filter));
    }

    /// <summary>
    /// Recursively deletes all children of the specified parent dictionary entry.
    /// </summary>
    private async Task RecursiveDeleteAsync(Guid parentId, UmbracoDbContext db)
    {
        List<DictionaryDto> children = await db.DictionaryEntries
            .Where(x => x.Parent == parentId)
            .ToListAsync();

        foreach (DictionaryDto child in children)
        {
            await RecursiveDeleteAsync(child.UniqueId, db);
            await DeleteEntityAsync(child.UniqueId, db);

            // Clear the cache entries that exist by item key
            IsolatedCache.Clear(RepositoryCacheKeys.GetKey<IDictionaryItem, string>(child.Key));
        }
    }

    /// <summary>
    /// Deletes a dictionary entry and its translations by unique ID.
    /// </summary>
    private static async Task DeleteEntityAsync(Guid key, UmbracoDbContext db)
    {
        await db.Set<LanguageTextDto>()
            .Where(x => x.UniqueId == key)
            .ExecuteDeleteAsync();

        await db.DictionaryEntries
            .Where(x => x.UniqueId == key)
            .ExecuteDeleteAsync();
    }

    private async Task<IDictionary<int, ILanguage>> GetLanguagesByIdAsync() =>
        (await _languageRepository.GetAllAsync(CancellationToken.None))
            .ToDictionary(language => language.Id);

    private async Task<IDictionary<string, ILanguage>> GetLanguagesByIsoCodeAsync() =>
        (await _languageRepository.GetAllAsync(CancellationToken.None))
            .ToDictionary(language => language.IsoCode);

    #endregion
}
