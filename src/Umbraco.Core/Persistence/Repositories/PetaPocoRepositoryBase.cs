using System;
using System.Collections.Generic;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Persistence.Caching;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Persistence.Repositories
{
    /// <summary>
    /// Represent an abstract Repository for PetaPoco based repositories
    /// </summary>
    /// <typeparam name="TId"></typeparam>
    /// <typeparam name="TEntity"></typeparam>
    internal abstract class PetaPocoRepositoryBase<TId, TEntity> : RepositoryBase<TId, TEntity>
        where TEntity : class, IAggregateRoot
    {
		protected PetaPocoRepositoryBase(IDatabaseUnitOfWork work)
			: base(work)
        {
        }

		protected PetaPocoRepositoryBase(IDatabaseUnitOfWork work, IRepositoryCacheProvider cache)
			: base(work, cache)
        {
        }

		/// <summary>
		/// Returns the database Unit of Work added to the repository
		/// </summary>
		protected internal new IDatabaseUnitOfWork UnitOfWork
		{
			get { return (IDatabaseUnitOfWork)base.UnitOfWork; }
		}

		protected UmbracoDatabase Database
        {
            get { return UnitOfWork.Database; }			
        }

        #region Abstract Methods
        
        protected abstract Sql GetBaseQuery(bool isCount);
        protected abstract string GetBaseWhereClause();
        protected abstract IEnumerable<string> GetDeleteClauses();
        protected abstract Guid NodeObjectTypeId { get; }
        protected abstract override void PersistNewItem(TEntity entity);
        protected abstract override void PersistUpdatedItem(TEntity entity);

        #endregion

        protected override bool PerformExists(TId id)
        {
            var sql = GetBaseQuery(true);
            sql.Where(GetBaseWhereClause(), new { Id = id});
            var count = Database.ExecuteScalar<int>(sql);
            return count == 1;
        }

        protected override int PerformCount(IQuery<TEntity> query)
        {
            var sqlClause = GetBaseQuery(true);
            var translator = new SqlTranslator<TEntity>(sqlClause, query);
            var sql = translator.Translate();

            return Database.ExecuteScalar<int>(sql);
        }

        protected override void PersistDeletedItem(TEntity entity)
        {
            var deletes = GetDeleteClauses();

            //wrap in a transaction in case any of the delete processes fail so they'll get rolled back!
            using (var trans = Database.GetTransaction())
            {
                try
                {
                    foreach (var delete in deletes)
                    {
                        Database.Execute(delete, new { Id = entity.Id });
                    }
                    trans.Complete();
                }
                catch (Exception ex)
                {
                    trans.Dispose();
                    LogHelper.Error(GetType(), "Deletion process failed, transaction rolled back", ex);
                    throw;
                }
            }
            
        }
    }
}