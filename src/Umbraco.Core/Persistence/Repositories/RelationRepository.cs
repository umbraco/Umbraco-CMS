using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Models;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence.Caching;
using Umbraco.Core.Persistence.Factories;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Persistence.Repositories
{
    /// <summary>
    /// Represents a repository for doing CRUD operations for <see cref="Relation"/>
    /// </summary>
    internal class RelationRepository : PetaPocoRepositoryBase<int, Relation>, IRelationRepository
    {
        private readonly IRelationTypeRepository _relationTypeRepository;

		public RelationRepository(IDatabaseUnitOfWork work, IRelationTypeRepository relationTypeRepository)
			: base(work)
        {
            _relationTypeRepository = relationTypeRepository;
        }

		public RelationRepository(IDatabaseUnitOfWork work, IRepositoryCacheProvider cache, IRelationTypeRepository relationTypeRepository)
            : base(work, cache)
        {
            _relationTypeRepository = relationTypeRepository;
        }

        #region Overrides of RepositoryBase<int,Relation>

        protected override Relation PerformGet(int id)
        {
            var sql = GetBaseQuery(false);
            sql.Where(GetBaseWhereClause(), new { Id = id });

            var dto = Database.FirstOrDefault<RelationDto>(sql);
            if (dto == null)
                return null;

            var relationType = _relationTypeRepository.Get(dto.RelationType);
            if(relationType == null)
                throw new Exception(string.Format("RelationType with Id: {0} doesn't exist", dto.RelationType));

            var factory = new RelationFactory(relationType);
            var entity = factory.BuildEntity(dto);

            entity.ResetDirtyProperties();

            return entity;
        }

        protected override IEnumerable<Relation> PerformGetAll(params int[] ids)
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
                var dtos = Database.Fetch<RelationDto>("WHERE id > 0");
                foreach (var dto in dtos)
                {
                    yield return Get(dto.Id);
                }
            }
        }

        protected override IEnumerable<Relation> PerformGetByQuery(IQuery<Relation> query)
        {
            var sqlClause = GetBaseQuery(false);
            var translator = new SqlTranslator<Relation>(sqlClause, query);
            var sql = translator.Translate();

            var dtos = Database.Fetch<RelationDto>(sql);

            foreach (var dto in dtos)
            {
                yield return Get(dto.Id);
            }
        }

        #endregion

        #region Overrides of PetaPocoRepositoryBase<int,Relation>

        protected override Sql GetBaseQuery(bool isCount)
        {
            var sql = new Sql();
            sql.Select(isCount ? "COUNT(*)" : "*");
            sql.From("umbracoRelation");
            return sql;
        }

        protected override string GetBaseWhereClause()
        {
            return "umbracoRelation.id = @Id";
        }

        protected override IEnumerable<string> GetDeleteClauses()
        {
            var list = new List<string>
                           {
                               string.Format("DELETE FROM umbracoRelation WHERE id = @Id")
                           };
            return list;
        }

        protected override Guid NodeObjectTypeId
        {
            get { throw new NotImplementedException(); }
        }

        #endregion

        #region Unit of Work Implementation

        protected override void PersistNewItem(Relation entity)
        {
            entity.AddingEntity();

            var factory = new RelationFactory(null);
            var dto = factory.BuildDto(entity);

            var id = Convert.ToInt32(Database.Insert(dto));
            entity.Id = id;

            entity.ResetDirtyProperties();
        }

        protected override void PersistUpdatedItem(Relation entity)
        {
            entity.UpdatingEntity();

            var factory = new RelationFactory(null);
            var dto = factory.BuildDto(entity);
            Database.Update(dto);

            entity.ResetDirtyProperties();
        }

        #endregion
    }
}