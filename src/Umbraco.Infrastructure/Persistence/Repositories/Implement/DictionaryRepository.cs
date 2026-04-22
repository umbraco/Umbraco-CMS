using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Persistence.Factories;
using Umbraco.Cms.Infrastructure.Persistence.Querying;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

/// <summary>
///     Represents a repository for doing CRUD operations for <see cref="DictionaryItem" />
/// </summary>
internal sealed class DictionaryRepository : EntityRepositoryBase<int, IDictionaryItem>, IDictionaryRepository
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILanguageRepository _languageRepository;
    private readonly IOptionsMonitor<DictionarySettings> _dictionarySettings;

    private string QuotedColumn(string columnName) => $"{QuoteTableName(DictionaryDto.TableName)}.{QuoteColumnName(columnName)}";

    /// <summary>
    /// Initializes a new instance of the <see cref="DictionaryRepository"/> class.
    /// </summary>
    /// <param name="scopeAccessor">Provides access to the database scope for context management.</param>
    /// <param name="cache">The application-level cache manager.</param>
    /// <param name="logger">The logger used for logging repository operations.</param>
    /// <param name="loggerFactory">Factory for creating logger instances.</param>
    /// <param name="languageRepository">Repository for managing language entities.</param>
    /// <param name="repositoryCacheVersionService">Service for managing repository cache versions.</param>
    /// <param name="cacheSyncService">Service for synchronizing cache across distributed environments.</param>
    /// <param name="dictionarySettings">Monitors and provides access to dictionary-related settings.</param>
    public DictionaryRepository(
        IScopeAccessor scopeAccessor,
        AppCaches cache,
        ILogger<DictionaryRepository> logger,
        ILoggerFactory loggerFactory,
        ILanguageRepository languageRepository,
        IRepositoryCacheVersionService repositoryCacheVersionService,
        ICacheSyncService cacheSyncService,
        IOptionsMonitor<DictionarySettings> dictionarySettings)
        : base(scopeAccessor, cache, logger, repositoryCacheVersionService, cacheSyncService)
    {
        _loggerFactory = loggerFactory;
        _languageRepository = languageRepository;
        _dictionarySettings = dictionarySettings;
    }

    /// <summary>
    /// Gets a dictionary item by its unique identifier.
    /// </summary>
    /// <param name="uniqueId">The unique identifier of the dictionary item.</param>
    /// <returns>The dictionary item if found; otherwise, null.</returns>
    public IDictionaryItem? Get(Guid uniqueId)
    {
        var uniqueIdRepo = new DictionaryByUniqueIdRepository(
            this,
            ScopeAccessor,
            AppCaches,
            _loggerFactory.CreateLogger<DictionaryByUniqueIdRepository>(),
            RepositoryCacheVersionService,
            CacheSyncService);
        return uniqueIdRepo.Get(uniqueId);
    }

    /// <summary>
    /// Retrieves multiple dictionary items by their unique identifiers.
    /// </summary>
    /// <param name="uniqueIds">An array of unique identifiers for the dictionary items to retrieve.</param>
    /// <returns>An enumerable collection of dictionary items matching the specified unique identifiers.</returns>
    public IEnumerable<IDictionaryItem> GetMany(params Guid[] uniqueIds)
    {
        var uniqueIdRepo = new DictionaryByUniqueIdRepository(
            this,
            ScopeAccessor,
            AppCaches,
            _loggerFactory.CreateLogger<DictionaryByUniqueIdRepository>(),
            RepositoryCacheVersionService,
            CacheSyncService);
        return uniqueIdRepo.GetMany(uniqueIds);
    }

    /// <summary>
    /// Gets a dictionary item by its key.
    /// </summary>
    /// <param name="key">The string key of the dictionary item.</param>
    /// <returns>The dictionary item if found; otherwise, <c>null</c>.</returns>
    public IDictionaryItem? Get(string key)
    {
        var keyRepo = new DictionaryByKeyRepository(
            this,
            ScopeAccessor,
            AppCaches,
            _loggerFactory.CreateLogger<DictionaryByKeyRepository>(),
            RepositoryCacheVersionService,
            CacheSyncService);
        return keyRepo.Get(key);
    }

    /// <summary>
    /// Retrieves dictionary items corresponding to the specified keys.
    /// </summary>
    /// <param name="keys">The array of keys for which to retrieve dictionary items.</param>
    /// <returns>An enumerable collection of dictionary items matching the provided keys.</returns>
    public IEnumerable<IDictionaryItem> GetManyByKeys(string[] keys)
    {
        var keyRepo = new DictionaryByKeyRepository(
            this,
            ScopeAccessor,
            AppCaches,
            _loggerFactory.CreateLogger<DictionaryByKeyRepository>(),
            RepositoryCacheVersionService,
            CacheSyncService);
        return keyRepo.GetMany(keys);
    }

    /// <summary>
    /// Gets a dictionary mapping of dictionary item keys to their unique identifiers.
    /// </summary>
    /// <returns>A dictionary where the key is the dictionary item key and the value is the corresponding unique identifier (Guid).</returns>
    public Dictionary<string, Guid> GetDictionaryItemKeyMap()
    {
        var columns = new[] { "key", "id" }.Select(x => (object)QuotedColumn(x)).ToArray();
        Sql<ISqlContext> sql = Sql().Select(columns).From<DictionaryDto>();
        return Database.Fetch<DictionaryItemKeyIdDto>(sql).ToDictionary(x => x.Key, x => x.Id);
    }

    /// <summary>
    /// Retrieves all descendant dictionary items of a specified parent item, optionally filtered by a search string.
    /// </summary>
    /// <param name="parentId">The unique identifier of the parent dictionary item from which to retrieve descendants. If <c>null</c>, all dictionary items in the repository are returned.</param>
    /// <param name="filter">An optional string to filter the dictionary items by key or value.</param>
    /// <returns>
    /// An <see cref="IEnumerable{IDictionaryItem}"/> containing all descendant dictionary items of the specified parent, or all items if <paramref name="parentId"/> is <c>null</c>. The results are ordered by item key.
    /// </returns>
    public IEnumerable<IDictionaryItem> GetDictionaryItemDescendants(Guid? parentId, string? filter = null)
    {
        IDictionary<int, ILanguage> languageIsoCodeById = GetLanguagesById();

        // This methods will look up children at each level, since we do not store a path for dictionary (ATM), we need to do a recursive
        // lookup to get descendants. Currently this is the most efficient way to do it
        Func<Guid[], IEnumerable<IEnumerable<IDictionaryItem>>> getItemsFromParents = guids =>
        {
            return guids.InGroupsOf(Constants.Sql.MaxParameterCount)
                .Select(group =>
                {
                    Sql<ISqlContext> sql = GetBaseQuery(false)
                        .Where<DictionaryDto>(x => x.Parent != null)
                        .WhereIn<DictionaryDto>(x => x.Parent, group);

                    ApplyFilterToQuery(sql, filter);
                    sql.OrderBy<DictionaryDto>(x => x.UniqueId);

                    return Database
                        .FetchOneToMany<DictionaryDto>(x => x.LanguageTextDtos, sql)
                        .Select(dto => ConvertFromDto(dto, languageIsoCodeById));
                });
        };

        if (!parentId.HasValue)
        {
            Sql<ISqlContext> sql = GetBaseQuery(false)
                .Where<DictionaryDto>(x => x.PrimaryKey > 0);

            ApplyFilterToQuery(sql, filter);

            return Database
                .FetchOneToMany<DictionaryDto>(x => x.LanguageTextDtos, sql)
                .Select(dto => ConvertFromDto(dto, languageIsoCodeById))
                .OrderBy(DictionaryItemOrdering);
        }

        return getItemsFromParents(new[] { parentId.Value })
            .SelectRecursive(items => getItemsFromParents(items.Select(x => x.Key).ToArray())).SelectMany(items => items)
            .OrderBy(DictionaryItemOrdering);

        // we're loading all descendants into memory, sometimes recursively... so we have to order them in memory too
        string DictionaryItemOrdering(IDictionaryItem item) => item.ItemKey;
    }

    /// <summary>
    ///     Applies the filter condition to the SQL query based on configuration settings.
    /// </summary>
    /// <param name="sql">The SQL query to modify.</param>
    /// <param name="filter">The filter string to apply, or null if no filter should be applied.</param>
    private void ApplyFilterToQuery(Sql<ISqlContext> sql, string? filter)
    {
        if (filter.IsNullOrWhiteSpace())
        {
            return;
        }

        if (_dictionarySettings.CurrentValue.EnableValueSearch)
        {
            // Search in both keys and values
            // Use a subquery to find dictionary items that have matching translations
            // Then fetch ALL translations for those items
            sql.Where(
                $"({QuotedColumn("key")} LIKE @0 OR {QuotedColumn("id")} IN (SELECT DISTINCT {QuoteColumnName("UniqueId")} FROM {QuoteTableName(LanguageTextDto.TableName)} WHERE {QuoteColumnName("value")} LIKE @1))",
                $"{filter}%",
                $"%{filter}%");
        }
        else
        {
            // Search only in keys
            sql.Where<DictionaryDto>(x => x.Key.StartsWith(filter));
        }
    }

    protected override IRepositoryCachePolicy<IDictionaryItem, int> CreateCachePolicy()
    {
        var options = new RepositoryCachePolicyOptions
        {
            // allow zero to be cached
            GetAllCacheAllowZeroCount = true
        };

        return new SingleItemsOnlyRepositoryCachePolicy<IDictionaryItem, int>(
            GlobalIsolatedCache,
            ScopeAccessor,
            options,
            RepositoryCacheVersionService,
            CacheSyncService);
    }

    private static IDictionaryItem ConvertFromDto(DictionaryDto dto, IDictionary<int, ILanguage> languagesById)
    {
        IDictionaryItem entity = DictionaryItemFactory.BuildEntity(dto);

        entity.Translations = dto.LanguageTextDtos.EmptyNull()
            .Where(x => x.LanguageId > 0)
            .Select(x => languagesById.TryGetValue(x.LanguageId, out ILanguage? language)
                ? DictionaryTranslationFactory.BuildEntity(x, dto.UniqueId, language)
                : null)
            .WhereNotNull()
            .ToList();

        return entity;
    }

    #region Overrides of RepositoryBase<int,DictionaryItem>

    protected override IDictionaryItem? PerformGet(int id)
    {
        Sql<ISqlContext> sql = GetBaseQuery(false)
            .Where(GetBaseWhereClause(), new { id })
            .OrderBy<DictionaryDto>(x => x.UniqueId);

        DictionaryDto? dto = Database
            .FetchOneToMany<DictionaryDto>(x => x.LanguageTextDtos, sql)
            .FirstOrDefault();

        if (dto == null)
        {
            return null;
        }

        IDictionaryItem entity = ConvertFromDto(dto, GetLanguagesById());

        // reset dirty initial properties (U4-1946)
        ((EntityBase)entity).ResetDirtyProperties(false);

        return entity;
    }

    private IEnumerable<IDictionaryItem> GetRootDictionaryItems()
    {
        IQuery<IDictionaryItem> query = Query<IDictionaryItem>().Where(x => x.ParentId == null);
        return Get(query);
    }

    private sealed class DictionaryItemKeyIdDto
    {
        /// <summary>
        /// Gets the unique key identifier for the dictionary item.
        /// </summary>
        public string Key { get; } = null!;

        /// <summary>
        /// Gets or sets the unique identifier of the dictionary item.
        /// </summary>
        public Guid Id { get; set; }
    }

    private sealed class DictionaryByUniqueIdRepository : SimpleGetRepository<Guid, IDictionaryItem, DictionaryDto>
    {
        private readonly DictionaryRepository _dictionaryRepository;
        private readonly IRepositoryCacheVersionService _repositoryCacheVersionService;
        private readonly ICacheSyncService _cacheSyncService;
        private readonly IDictionary<int, ILanguage> _languagesById;

        private string QuotedColumn(string columnName) => $"{QuoteTableName(DictionaryDto.TableName)}.{QuoteColumnName(columnName)}";

        /// <summary>
        /// Initializes a new instance of the <see cref="Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement.DictionaryRepository.DictionaryByUniqueIdRepository"/> class.
        /// </summary>
        /// <param name="dictionaryRepository">The parent <see cref="DictionaryRepository"/> used for dictionary item operations.</param>
        /// <param name="scopeAccessor">Provides access to the current database scope.</param>
        /// <param name="cache">The <see cref="AppCaches"/> instance for managing application-level caching.</param>
        /// <param name="logger">The <see cref="ILogger{TCategoryName}"/> instance for logging repository activity.</param>
        /// <param name="repositoryCacheVersionService">Service for managing repository cache versioning.</param>
        /// <param name="cacheSyncService">Service for synchronizing cache across distributed environments.</param>
        public DictionaryByUniqueIdRepository(
            DictionaryRepository dictionaryRepository,
            IScopeAccessor scopeAccessor,
            AppCaches cache,
            ILogger<DictionaryByUniqueIdRepository> logger,
            IRepositoryCacheVersionService repositoryCacheVersionService,
            ICacheSyncService cacheSyncService)
            : base(
                scopeAccessor,
                cache,
                logger,
                repositoryCacheVersionService,
                cacheSyncService)
        {
            _dictionaryRepository = dictionaryRepository;
            _repositoryCacheVersionService = repositoryCacheVersionService;
            _cacheSyncService = cacheSyncService;
            _languagesById = dictionaryRepository.GetLanguagesById();
        }

        protected override IEnumerable<DictionaryDto> PerformFetch(Sql sql) =>
            Database
                .FetchOneToMany<DictionaryDto>(x => x.LanguageTextDtos, sql);

        protected override Sql<ISqlContext> GetBaseQuery(bool isCount) => _dictionaryRepository.GetBaseQuery(isCount);

        protected override string GetBaseWhereClause() =>
            $"{QuotedColumn("id")} = @id";

        protected override IDictionaryItem ConvertToEntity(DictionaryDto dto) =>
            ConvertFromDto(dto, _languagesById);

        protected override object GetBaseWhereClauseArguments(Guid id) => new { id };

        protected override string GetWhereInClauseForGetAll() =>
            $"{QuotedColumn("id")} in (@ids)";

        protected override IRepositoryCachePolicy<IDictionaryItem, Guid> CreateCachePolicy()
        {
            var options = new RepositoryCachePolicyOptions
            {
                // allow zero to be cached
                GetAllCacheAllowZeroCount = true
            };

            return new SingleItemsOnlyRepositoryCachePolicy<IDictionaryItem, Guid>(
                GlobalIsolatedCache,
                ScopeAccessor,
                options,
                _repositoryCacheVersionService,
                _cacheSyncService);
        }

        protected override IEnumerable<IDictionaryItem> PerformGetAll(params Guid[]? ids)
        {
            Sql<ISqlContext> sql = GetBaseQuery(false).Where<DictionaryDto>(x => x.PrimaryKey > 0);
            if (ids?.Any() ?? false)
            {
                sql.WhereIn<DictionaryDto>(x => x.UniqueId, ids);
            }

            return Database
                .FetchOneToMany<DictionaryDto>(x => x.LanguageTextDtos, sql)
                .Select(ConvertToEntity);
        }
    }

    private sealed class DictionaryByKeyRepository : SimpleGetRepository<string, IDictionaryItem, DictionaryDto>
    {
        private readonly DictionaryRepository _dictionaryRepository;
        private readonly IRepositoryCacheVersionService _repositoryCacheVersionService;
        private readonly ICacheSyncService _cacheSyncService;
        private readonly IDictionary<int, ILanguage> _languagesById;

        private string QuotedColumn(string columnName) => $"{QuoteTableName(DictionaryDto.TableName)}.{QuoteColumnName(columnName)}";

        /// <summary>
        /// Initializes a new instance of the <see cref="Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement.DictionaryRepository.DictionaryByKeyRepository"/> class.
        /// </summary>
        /// <param name="dictionaryRepository">The parent <see cref="DictionaryRepository"/> instance.</param>
        /// <param name="scopeAccessor">The <see cref="IScopeAccessor"/> used to manage database scopes.</param>
        /// <param name="cache">The <see cref="AppCaches"/> instance for application-level caching.</param>
        /// <param name="logger">The <see cref="ILogger{DictionaryByKeyRepository}"/> instance for logging.</param>
        /// <param name="repositoryCacheVersionService">The <see cref="IRepositoryCacheVersionService"/> for managing cache versions.</param>
        /// <param name="cacheSyncService">The <see cref="ICacheSyncService"/> for synchronizing cache across instances.</param>
        public DictionaryByKeyRepository(
            DictionaryRepository dictionaryRepository,
            IScopeAccessor scopeAccessor,
            AppCaches cache,
            ILogger<DictionaryByKeyRepository> logger,
            IRepositoryCacheVersionService repositoryCacheVersionService,
            ICacheSyncService cacheSyncService)
            : base(
                scopeAccessor,
                cache,
                logger,
                repositoryCacheVersionService,
                cacheSyncService)
        {
            _dictionaryRepository = dictionaryRepository;
            _repositoryCacheVersionService = repositoryCacheVersionService;
            _cacheSyncService = cacheSyncService;
            _languagesById = dictionaryRepository.GetLanguagesById();
        }

        protected override IEnumerable<DictionaryDto> PerformFetch(Sql sql) =>
            Database
                .FetchOneToMany<DictionaryDto>(x => x.LanguageTextDtos, sql);

        protected override Sql<ISqlContext> GetBaseQuery(bool isCount) => _dictionaryRepository.GetBaseQuery(isCount);

        protected override string GetBaseWhereClause() =>
            $"{QuotedColumn("key")} = @id";

        protected override IDictionaryItem ConvertToEntity(DictionaryDto dto) =>
            ConvertFromDto(dto, _languagesById);

        protected override object GetBaseWhereClauseArguments(string? id) => new { id };

        protected override string GetWhereInClauseForGetAll() =>
            $"{QuotedColumn("key")} IN (@ids)";

        protected override IRepositoryCachePolicy<IDictionaryItem, string> CreateCachePolicy()
        {
            var options = new RepositoryCachePolicyOptions
            {
                // allow null to be cached
                CacheNullValues = true,
                // allow zero to be cached
                GetAllCacheAllowZeroCount = true
            };

            return new SingleItemsOnlyRepositoryCachePolicy<IDictionaryItem, string>(
                GlobalIsolatedCache,
                ScopeAccessor,
                options,
                _repositoryCacheVersionService,
                _cacheSyncService);
        }

        protected override IEnumerable<IDictionaryItem> PerformGetAll(params string[]? ids)
        {
            Sql<ISqlContext> sql = GetBaseQuery(false).Where<DictionaryDto>(x => x.PrimaryKey > 0);
            if (ids?.Any() ?? false)
            {
                sql.WhereIn<DictionaryDto>(x => x.Key, ids);
            }

            return Database
                .FetchOneToMany<DictionaryDto>(x => x.LanguageTextDtos, sql)
                .Select(ConvertToEntity);
        }
    }

    protected override IEnumerable<IDictionaryItem> PerformGetAll(params int[]? ids)
    {
        Sql<ISqlContext> sql = GetBaseQuery(false).Where<DictionaryDto>(x => x.PrimaryKey > 0);
        if (ids?.Any() ?? false)
        {
            sql.WhereIn<DictionaryDto>(x => x.PrimaryKey, ids);
        }

        IDictionary<int, ILanguage> languageIsoCodeById = GetLanguagesById();

        return Database
            .FetchOneToMany<DictionaryDto>(x => x.LanguageTextDtos, sql)
            .Select(dto => ConvertFromDto(dto, languageIsoCodeById));
    }

    protected override IEnumerable<IDictionaryItem> PerformGetByQuery(IQuery<IDictionaryItem> query)
    {
        Sql<ISqlContext> sqlClause = GetBaseQuery(false);
        var translator = new SqlTranslator<IDictionaryItem>(sqlClause, query);
        Sql<ISqlContext> sql = translator.Translate();
        sql.OrderBy<DictionaryDto>(x => x.UniqueId);

        IDictionary<int, ILanguage> languageIsoCodeById = GetLanguagesById();

        return Database
            .FetchOneToMany<DictionaryDto>(x => x.LanguageTextDtos, sql)
            .Select(dto => ConvertFromDto(dto, languageIsoCodeById));
    }

    #endregion

    #region Overrides of EntityRepositoryBase<int,DictionaryItem>

    protected override Sql<ISqlContext> GetBaseQuery(bool isCount)
    {
        Sql<ISqlContext> sql = Sql();
        if (isCount)
        {
            sql.SelectCount()
                .From<DictionaryDto>();
        }
        else
        {
            sql.SelectAll()
                .From<DictionaryDto>()
                .LeftJoin<LanguageTextDto>()
                .On<DictionaryDto, LanguageTextDto>(left => left.UniqueId, right => right.UniqueId);
        }

        return sql;
    }

    protected override string GetBaseWhereClause() => $"{QuotedColumn("pk")} = @id";

    protected override IEnumerable<string> GetDeleteClauses() => new List<string>();

    #endregion

    #region Unit of Work Implementation

    protected override void PersistNewItem(IDictionaryItem entity)
    {
        var dictionaryItem = (DictionaryItem)entity;

        dictionaryItem.AddingEntity();

        foreach (IDictionaryTranslation translation in dictionaryItem.Translations)
        {
            translation.Value = translation.Value.ToValidXmlString();
        }

        DictionaryDto dto = DictionaryItemFactory.BuildDto(dictionaryItem);

        var id = Convert.ToInt32(Database.Insert(dto));
        dictionaryItem.Id = id;

        IDictionary<string, ILanguage> languagesByIsoCode = GetLanguagesByIsoCode();

        foreach (IDictionaryTranslation translation in dictionaryItem.Translations)
        {
            LanguageTextDto textDto = DictionaryTranslationFactory.BuildDto(translation, dictionaryItem.Key, languagesByIsoCode);
            translation.Id = Convert.ToInt32(Database.Insert(textDto));
            translation.Key = dictionaryItem.Key;
        }

        dictionaryItem.ResetDirtyProperties();
    }

    protected override void PersistUpdatedItem(IDictionaryItem entity)
    {
        entity.UpdatingEntity();

        foreach (IDictionaryTranslation translation in entity.Translations)
        {
            translation.Value = translation.Value.ToValidXmlString();
        }

        DictionaryDto dto = DictionaryItemFactory.BuildDto(entity);

        Database.Update(dto);

        IDictionary<string, ILanguage> languagesByIsoCode = GetLanguagesByIsoCode();

        foreach (IDictionaryTranslation translation in entity.Translations)
        {
            LanguageTextDto textDto = DictionaryTranslationFactory.BuildDto(translation, entity.Key, languagesByIsoCode);
            if (translation.HasIdentity)
            {
                Database.Update(textDto);
            }
            else
            {
                translation.Id = Convert.ToInt32(Database.Insert(textDto));
                translation.Key = entity.Key;
            }
        }

        entity.ResetDirtyProperties();

        // Clear the cache entries that exist by uniqueid/item key
        IsolatedCache.Clear(RepositoryCacheKeys.GetKey<IDictionaryItem, string>(entity.ItemKey));
        IsolatedCache.Clear(RepositoryCacheKeys.GetKey<IDictionaryItem, Guid>(entity.Key));
    }

    protected override void PersistDeletedItem(IDictionaryItem entity)
    {
        RecursiveDelete(entity.Key);

        DeleteEntity(entity.Key);

        // Clear the cache entries that exist by uniqueid/item key
        IsolatedCache.Clear(RepositoryCacheKeys.GetKey<IDictionaryItem, string>(entity.ItemKey));
        IsolatedCache.Clear(RepositoryCacheKeys.GetKey<IDictionaryItem, Guid>(entity.Key));

        entity.DeleteDate = DateTime.UtcNow;
    }

    private void RecursiveDelete(Guid parentId)
    {
        Sql<ISqlContext> sql = SqlContext.Sql()
            .Select<DictionaryDto>()
            .From<DictionaryDto>()
            .Where<DictionaryDto>(c => c.Parent == parentId);
        List<DictionaryDto>? list = Database.Fetch<DictionaryDto>(sql);

        foreach (DictionaryDto? dto in list)
        {
            RecursiveDelete(dto.UniqueId);

            DeleteEntity(dto.UniqueId);

            // Clear the cache entries that exist by uniqueid/item key
            IsolatedCache.Clear(RepositoryCacheKeys.GetKey<IDictionaryItem, string>(dto.Key));
            IsolatedCache.Clear(RepositoryCacheKeys.GetKey<IDictionaryItem, Guid>(dto.UniqueId));
        }
    }

    private void DeleteEntity(Guid key)
    {
        Sql<ISqlContext> sql = SqlContext.Sql()
            .Delete<LanguageTextDto>()
            .Where<LanguageTextDto>(c => c.UniqueId == key);
        Database.Execute(sql);

        sql = SqlContext.Sql()
            .Delete<DictionaryDto>()
            .Where<DictionaryDto>(c => c.UniqueId == key);
        Database.Execute(sql);
    }

    private IDictionary<int, ILanguage> GetLanguagesById() => _languageRepository
        .GetMany()
        .ToDictionary(language => language.Id);

    private IDictionary<string, ILanguage> GetLanguagesByIsoCode() => _languageRepository
        .GetMany()
        .ToDictionary(language => language.IsoCode);

    #endregion
}
