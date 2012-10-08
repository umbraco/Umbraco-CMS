using System;
using System.Collections.Generic;
using Umbraco.Core.Models;
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
        where TEntity : IAggregateRoot
    {
        private Database _database;

        protected PetaPocoRepositoryBase(IUnitOfWork work) : base(work)
        {
        }

        protected PetaPocoRepositoryBase(IUnitOfWork work, IRepositoryCacheProvider cache) : base(work, cache)
        {
            _database = DatabaseFactory.Current.Database;
        }

        protected Database Database
        {
            get { return _database; }
        }

        #region Abstract Methods
        
        protected abstract Sql GetBaseQuery(bool isCount);
        protected abstract Sql GetBaseWhereClause();
        protected abstract IEnumerable<string> GetDeleteClauses();
        protected abstract Guid NodeObjectTypeId { get; }
        protected abstract override void PersistNewItem(TEntity entity);
        protected abstract override void PersistUpdatedItem(TEntity entity);

        #endregion

        protected override int PerformCount(IQuery<TEntity> query)
        {
            var sqlClause = GetBaseQuery(true);
            var translator = new SqlTranslator<TEntity>(sqlClause, query);
            var sql = translator.Translate();

            return _database.ExecuteScalar<int>(sql);
        }

        protected override void PersistDeletedItem(TEntity entity)
        {
            var deletes = GetDeleteClauses();
            foreach (var delete in deletes)
            {
                _database.Execute(delete, new {Id = entity.Id});
            }
        }
    }
}