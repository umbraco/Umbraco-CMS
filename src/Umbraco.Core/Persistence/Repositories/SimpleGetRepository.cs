using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.EntityBase;

using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Persistence.Repositories
{
    /// <summary>
    /// Simple abstract ReadOnly repository used to simply have PerformGet and PeformGetAll with an underlying cache
    /// </summary>
    internal abstract class SimpleGetRepository<TId, TEntity, TDto> : PetaPocoRepositoryBase<TId, TEntity>
        where TEntity : class, IAggregateRoot
        where TDto: class
    {

        protected SimpleGetRepository(IDatabaseUnitOfWork work, CacheHelper cache, ILogger logger, ISqlSyntaxProvider sqlSyntax)
            : base(work, cache, logger, sqlSyntax)
        {
        }

        protected abstract TEntity ConvertToEntity(TDto dto);
        protected abstract object GetBaseWhereClauseArguments(TId id);
        protected abstract string GetWhereInClauseForGetAll();

        protected virtual IEnumerable<TDto> PerformFetch(Sql sql)
        {
            return Database.Fetch<TDto>(sql);
        } 

        protected override TEntity PerformGet(TId id)
        {
            var sql = GetBaseQuery(false);
            sql.Where(GetBaseWhereClause(), GetBaseWhereClauseArguments(id));

            var dto = PerformFetch(sql).FirstOrDefault();
            if (dto == null)
                return null;

            var entity = ConvertToEntity(dto);

            var dirtyEntity = entity as Entity;
            if (dirtyEntity != null)
            {
                //on initial construction we don't want to have dirty properties tracked
                // http://issues.umbraco.org/issue/U4-1946
                dirtyEntity.ResetDirtyProperties(false);    
            }

            return entity;
        }

        protected override IEnumerable<TEntity> PerformGetAll(params TId[] ids)
        {
            var sql = new Sql().From<TEntity>();

            if (ids.Any())
            {
                sql.Where(GetWhereInClauseForGetAll(), new { ids = ids });
            }
            
            return Database.Fetch<TDto>(sql).Select(ConvertToEntity);
        }

        protected override sealed IEnumerable<TEntity> PerformGetByQuery(IQuery<TEntity> query)
        {
            var sqlClause = GetBaseQuery(false);
            var translator = new SqlTranslator<TEntity>(sqlClause, query);
            var sql = translator.Translate();
            return Database.Fetch<TDto>(sql).Select(ConvertToEntity);
        }

        #region Not implemented and not required

        protected override sealed IEnumerable<string> GetDeleteClauses()
        {
            throw new NotImplementedException();
        }

        protected override sealed Guid NodeObjectTypeId
        {
            get { throw new NotImplementedException(); }
        }

        protected override sealed void PersistNewItem(TEntity entity)
        {
            throw new NotImplementedException();
        }

        protected override sealed void PersistUpdatedItem(TEntity entity)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}