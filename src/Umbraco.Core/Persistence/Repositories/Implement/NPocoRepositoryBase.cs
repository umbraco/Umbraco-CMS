using System;
using System.Collections.Generic;
using NPoco;
using Umbraco.Core.Cache;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.Entities;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Scoping;

namespace Umbraco.Core.Persistence.Repositories.Implement
{
    /// <summary>
    /// Represent an abstract Repository for NPoco based repositories
    /// </summary>
    /// <typeparam name="TId"></typeparam>
    /// <typeparam name="TEntity"></typeparam>
    internal abstract class NPocoRepositoryBase<TId, TEntity> : RepositoryBase<TId, TEntity>
        where TEntity : class, IEntity
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NPocoRepositoryBase{TId, TEntity}"/> class.
        /// </summary>
        protected NPocoRepositoryBase(IScopeAccessor scopeAccessor, AppCaches cache, ILogger logger)
            : base(scopeAccessor, cache, logger)
        { }

        /// <summary>
        /// Gets the repository's database.
        /// </summary>
        protected IUmbracoDatabase Database => AmbientScope.Database;

        /// <summary>
        /// Gets the Sql context.
        /// </summary>
        protected ISqlContext SqlContext=> AmbientScope.SqlContext;

        protected Sql<ISqlContext> Sql() => SqlContext.Sql();
        protected Sql<ISqlContext> Sql(string sql, params object[] args) => SqlContext.Sql(sql, args);
        protected ISqlSyntaxProvider SqlSyntax => SqlContext.SqlSyntax;
        protected IQuery<T> Query<T>() => SqlContext.Query<T>();

        #region Abstract Methods

        protected abstract Sql<ISqlContext> GetBaseQuery(bool isCount); // TODO: obsolete, use QueryType instead everywhere
        protected abstract string GetBaseWhereClause();
        protected abstract IEnumerable<string> GetDeleteClauses();
        protected abstract Guid NodeObjectTypeId { get; }
        protected abstract override void PersistNewItem(TEntity entity);
        protected abstract override void PersistUpdatedItem(TEntity entity);

        #endregion

        protected override bool PerformExists(TId id)
        {
            var sql = GetBaseQuery(true);
            sql.Where(GetBaseWhereClause(), new { id = id});
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
            foreach (var delete in deletes)
            {
                Database.Execute(delete, new { id = GetEntityId(entity) });
            }
            entity.DeleteDate = DateTime.Now;
        }
    }
}
