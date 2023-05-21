using Microsoft.Extensions.Logging;
using NPoco;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Infrastructure.Persistence.Querying;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

// TODO: Obsolete this, change all implementations of this like in Dictionary to just use custom Cache policies like in the member repository.

/// <summary>
///     Simple abstract ReadOnly repository used to simply have PerformGet and PeformGetAll with an underlying cache
/// </summary>
internal abstract class SimpleGetRepository<TId, TEntity, TDto> : EntityRepositoryBase<TId, TEntity>
    where TEntity : class, IEntity
    where TDto : class
{
    protected SimpleGetRepository(IScopeAccessor scopeAccessor, AppCaches cache, ILogger<SimpleGetRepository<TId, TEntity, TDto>> logger)
        : base(scopeAccessor, cache, logger)
    {
    }

    protected abstract TEntity ConvertToEntity(TDto dto);

    protected abstract object GetBaseWhereClauseArguments(TId? id);

    protected abstract string GetWhereInClauseForGetAll();

    protected virtual IEnumerable<TDto> PerformFetch(Sql sql) => Database.Fetch<TDto>(sql);

    protected override TEntity? PerformGet(TId? id)
    {
        Sql<ISqlContext> sql = GetBaseQuery(false);
        sql.Where(GetBaseWhereClause(), GetBaseWhereClauseArguments(id));

        TDto? dto = PerformFetch(sql).FirstOrDefault();
        if (dto == null)
        {
            return null;
        }

        TEntity entity = ConvertToEntity(dto);

        if (entity is EntityBase dirtyEntity)
        {
            // reset dirty initial properties (U4-1946)
            dirtyEntity.ResetDirtyProperties(false);
        }

        return entity;
    }

    protected override IEnumerable<TEntity> PerformGetAll(params TId[]? ids)
    {
        Sql<ISqlContext> sql = Sql().From<TDto>();

        if (ids?.Any() ?? false)
        {
            sql.Where(GetWhereInClauseForGetAll(), new
            {
                ids,
            });
        }

        return Database.Fetch<TDto>(sql).Select(ConvertToEntity);
    }

    protected sealed override IEnumerable<TEntity> PerformGetByQuery(IQuery<TEntity> query)
    {
        Sql<ISqlContext> sqlClause = GetBaseQuery(false);
        var translator = new SqlTranslator<TEntity>(sqlClause, query);
        Sql<ISqlContext> sql = translator.Translate();
        return Database.Fetch<TDto>(sql).Select(ConvertToEntity);
    }

    protected sealed override IEnumerable<string> GetDeleteClauses() =>
        throw new InvalidOperationException("This method won't be implemented.");

    protected sealed override void PersistNewItem(TEntity entity) =>
        throw new InvalidOperationException("This method won't be implemented.");

    protected sealed override void PersistUpdatedItem(TEntity entity) =>
        throw new InvalidOperationException("This method won't be implemented.");
}
