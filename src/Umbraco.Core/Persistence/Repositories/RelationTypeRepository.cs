using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Models.Rdbms;

using Umbraco.Core.Persistence.Factories;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Persistence.Repositories
{
    /// <summary>
    /// Represents a repository for doing CRUD operations for <see cref="RelationType"/>
    /// </summary>
    internal class RelationTypeRepository : PetaPocoRepositoryBase<int, IRelationType>, IRelationTypeRepository
    {

        public RelationTypeRepository(IDatabaseUnitOfWork work, CacheHelper cache, ILogger logger, ISqlSyntaxProvider sqlSyntax)
            : base(work, cache, logger, sqlSyntax)
        {
        }

        #region Overrides of RepositoryBase<int,RelationType>

        protected override IRelationType PerformGet(int id)
        {
            var sql = GetBaseQuery(false);
            sql.Where(GetBaseWhereClause(), new { Id = id });

            var dto = Database.FirstOrDefault<RelationTypeDto>(sql);
            if (dto == null)
                return null;

            var factory = new RelationTypeFactory();
            var entity = factory.BuildEntity(dto);

            //on initial construction we don't want to have dirty properties tracked
            // http://issues.umbraco.org/issue/U4-1946
            ((TracksChangesEntityBase)entity).ResetDirtyProperties(false);

            return entity;
        }

        protected override IEnumerable<IRelationType> PerformGetAll(params int[] ids)
        {
            if (ids.Any())
            {
                foreach (var id in ids)
                {
                    yield return Get(id);
                }
            }
            else
            {
                var dtos = Database.Fetch<RelationTypeDto>("WHERE id > 0");
                foreach (var dto in dtos)
                {
                    yield return Get(dto.Id);
                }
            }
        }

        protected override IEnumerable<IRelationType> PerformGetByQuery(IQuery<IRelationType> query)
        {
            var sqlClause = GetBaseQuery(false);
            var translator = new SqlTranslator<IRelationType>(sqlClause, query);
            var sql = translator.Translate();

            var dtos = Database.Fetch<RelationTypeDto>(sql);

            foreach (var dto in dtos)
            {
                yield return Get(dto.Id);
            }
        }

        #endregion

        #region Overrides of PetaPocoRepositoryBase<int,RelationType>

        protected override Sql GetBaseQuery(bool isCount)
        {
            var sql = new Sql();
            sql.Select(isCount ? "COUNT(*)" : "*")
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