using System;
using System.Collections.Generic;
using System.Linq;
using NPoco;
using Umbraco.Core.Cache;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence.Factories;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Persistence.Repositories
{
    /// <summary>
    /// Represents a repository for doing CRUD operations for <see cref="RelationType"/>
    /// </summary>
    internal class RelationTypeRepository : NPocoRepositoryBase<int, IRelationType>, IRelationTypeRepository
    {
        private IRepositoryCachePolicy<IRelationType, int> _cachePolicy;

        public RelationTypeRepository(IDatabaseUnitOfWork work, CacheHelper cache, ILogger logger, IQueryFactory queryFactory)
            : base(work, cache, logger, queryFactory)
        { }

        protected override IRepositoryCachePolicy<IRelationType, int> CachePolicy
        {
            get
            {
                if (_cachePolicy != null) return _cachePolicy;

                // assuming we don't have tons of relation types, use a FullDataSet policy, ie
                // cache the entire GetAll result once in a single collection - which can expire
                _cachePolicy = new FullDataSetRepositoryCachePolicy<IRelationType, int>(RuntimeCache, GetEntityId, /*expires:*/ true);

                return _cachePolicy;
            }
        }

        #region Overrides of RepositoryBase<int,RelationType>

        protected override IRelationType PerformGet(int id)
        {
            // use the underlying GetAll which will force cache all content types
            return GetAll().FirstOrDefault(x => x.Id == id);
        }

        protected override IEnumerable<IRelationType> PerformGetAll(params int[] ids)
        {
            var sql = GetBaseQuery(false);

            // should not happen due to the cache policy
            if (ids.Any())
                throw new NotImplementedException();

            var dtos = Database.Fetch<RelationTypeDto>(sql);
            var factory = new RelationTypeFactory();
            return dtos.Select(x => DtoToEntity(x, factory));
        }

        protected override IEnumerable<IRelationType> PerformGetByQuery(IQuery<IRelationType> query)
        {
            var sqlClause = GetBaseQuery(false);
            var translator = new SqlTranslator<IRelationType>(sqlClause, query);
            var sql = translator.Translate();

            var dtos = Database.Fetch<RelationTypeDto>(sql);
            var factory = new RelationTypeFactory();
            return dtos.Select(x => DtoToEntity(x, factory));
        }

        private static IRelationType DtoToEntity(RelationTypeDto dto, RelationTypeFactory factory)
        {
            var entity = factory.BuildEntity(dto);

            //on initial construction we don't want to have dirty properties tracked
            // http://issues.umbraco.org/issue/U4-1946
            ((TracksChangesEntityBase) entity).ResetDirtyProperties(false);

            return entity;
        }

        #endregion

        #region Overrides of NPocoRepositoryBase<int,RelationType>

        protected override Sql<SqlContext> GetBaseQuery(bool isCount)
        {
            var sql = Sql();

            sql = isCount
                ? sql.SelectCount()
                : sql.Select<RelationTypeDto>();

            sql
               .From<RelationTypeDto>();

            return sql;
        }

        protected override string GetBaseWhereClause()
        {
            return "umbracoRelationType.id = @Id";
        }

        protected override IEnumerable<string> GetDeleteClauses()
        {
            var list = new List<string>
                           {
                               "DELETE FROM umbracoRelation WHERE relType = @Id",
                               "DELETE FROM umbracoRelationType WHERE id = @Id"
                           };
            return list;
        }

        protected override Guid NodeObjectTypeId
        {
            get { throw new NotImplementedException(); }
        }

        #endregion

        #region Unit of Work Implementation

        protected override void PersistNewItem(IRelationType entity)
        {
            ((Entity)entity).AddingEntity();

            var factory = new RelationTypeFactory();
            var dto = factory.BuildDto(entity);

            var id = Convert.ToInt32(Database.Insert(dto));
            entity.Id = id;

            entity.ResetDirtyProperties();
        }

        protected override void PersistUpdatedItem(IRelationType entity)
        {
            ((Entity)entity).UpdatingEntity();

            var factory = new RelationTypeFactory();
            var dto = factory.BuildDto(entity);
            Database.Update(dto);

            entity.ResetDirtyProperties();
        }

        #endregion
    }
}