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

    public Dictionary<string, Guid> GetDictionaryItemKeyMap()
    {
        var columns = new[] { "key", "id" }.Select(x => (object)QuotedColumn(x)).ToArray();
        Sql<ISqlContext> sql = Sql().Select(columns).From<DictionaryDto>();
        return Database.Fetch<DictionaryItemKeyIdDto>(sql).ToDictionary(x => x.Key, x => x.Id);
    }

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
        public string Key { get; } = null!;

        public Guid Id { get; set; }
    }

    private sealed class DictionaryByUniqueIdRepository : SimpleGetRepository<Guid, IDictionaryItem, DictionaryDto>
    {
        private readonly DictionaryRepository _dictionaryRepository;
        private readonly IRepositoryCacheVersionService _repositoryCacheVersionService;
        private readonly ICacheSyncService _cacheSyncService;
        private readonly IDictionary<int, ILanguage> _languagesById;

        private string QuotedColumn(string columnName) => $"{QuoteTableName(DictionaryDto.TableName)}.{QuoteColumnName(columnName)}";

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
