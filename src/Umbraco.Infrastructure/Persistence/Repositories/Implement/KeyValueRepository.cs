using Microsoft.Extensions.Logging;
using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Persistence.Querying;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

internal sealed class KeyValueRepository : EntityRepositoryBase<string, IKeyValue>, IKeyValueRepository
{
    /// <summary>
    /// Initializes a new instance of the <see cref="KeyValueRepository"/> class.
    /// </summary>
    /// <param name="scopeAccessor">Provides access to the current database scope for repository operations.</param>
    /// <param name="logger">The logger used to record diagnostic and operational information for this repository.</param>
    /// <param name="repositoryCacheVersionService">Service used to manage cache versioning for repository data.</param>
    /// <param name="cacheSyncService">Service responsible for synchronizing cache across distributed environments.</param>
    public KeyValueRepository(
        IScopeAccessor scopeAccessor,
        ILogger<KeyValueRepository> logger,
        IRepositoryCacheVersionService repositoryCacheVersionService,
        ICacheSyncService cacheSyncService)
        : base(
            scopeAccessor,
            AppCaches.NoCache,
            logger,
            repositoryCacheVersionService,
            cacheSyncService)
    {
    }

    /// <inheritdoc />
    public IReadOnlyDictionary<string, string?> FindByKeyPrefix(string keyPrefix)
        => Get(Query<IKeyValue>().Where(entity => entity.Identifier.StartsWith(keyPrefix)))
            .ToDictionary(x => x.Identifier, x => x.Value);

    #region Overrides of IReadWriteQueryRepository<string, IKeyValue>

    /// <summary>
    /// Saves the specified key-value entity to the repository. If the entity does not exist, it will be inserted; otherwise, it will be updated.
    /// </summary>
    /// <param name="entity">The key-value entity to save.</param>
    public override void Save(IKeyValue entity)
    {
        if (Get(entity.Identifier) == null)
        {
            PersistNewItem(entity);
        }
        else
        {
            PersistUpdatedItem(entity);
        }
    }

    #endregion

    #region Overrides of EntityRepositoryBase<string, IKeyValue>

    protected override Sql<ISqlContext> GetBaseQuery(bool isCount)
    {
        Sql<ISqlContext> sql = SqlContext.Sql();

        sql = isCount
            ? sql.SelectCount()
            : sql.Select<KeyValueDto>();

        sql
            .From<KeyValueDto>();

        return sql;
    }

    protected override string GetBaseWhereClause() => QuoteTableName(Constants.DatabaseSchema.Tables.KeyValue) + ".key = @id";

    protected override IEnumerable<string> GetDeleteClauses() => Enumerable.Empty<string>();

    protected override IKeyValue? PerformGet(string? id)
    {
        Sql<ISqlContext> sql = GetBaseQuery(false).Where<KeyValueDto>(x => x.Key == id);
        KeyValueDto? dto = Database.Fetch<KeyValueDto>(sql).FirstOrDefault();
        return dto == null ? null : Map(dto);
    }

    protected override IEnumerable<IKeyValue> PerformGetAll(params string[]? ids)
    {
        Sql<ISqlContext> sql = GetBaseQuery(false).WhereIn<KeyValueDto>(x => x.Key, ids);
        List<KeyValueDto>? dtos = Database.Fetch<KeyValueDto>(sql);
        return dtos?.WhereNotNull().Select(Map)!;
    }

    protected override IEnumerable<IKeyValue> PerformGetByQuery(IQuery<IKeyValue> query)
    {
        Sql<ISqlContext> sqlClause = GetBaseQuery(false);
        var translator = new SqlTranslator<IKeyValue>(sqlClause, query);
        Sql<ISqlContext> sql = translator.Translate();
        return Database.Fetch<KeyValueDto>(sql).Select(Map).WhereNotNull();
    }

    protected override void PersistNewItem(IKeyValue entity)
    {
        KeyValueDto? dto = Map(entity);
        Database.Insert(dto);
    }

    protected override void PersistUpdatedItem(IKeyValue entity)
    {
        KeyValueDto? dto = Map(entity);
        if (dto is not null)
        {
            Database.Update(dto);
        }
    }

    private static KeyValueDto? Map(IKeyValue? keyValue)
    {
        if (keyValue == null)
        {
            return null;
        }

        return new KeyValueDto { Key = keyValue.Identifier, Value = keyValue.Value, UpdateDate = keyValue.UpdateDate };
    }

    private static IKeyValue? Map(KeyValueDto? dto)
    {
        if (dto == null)
        {
            return null;
        }

        return new KeyValue { Identifier = dto.Key, Value = dto.Value, UpdateDate = dto.UpdateDate };
    }

    #endregion
}
