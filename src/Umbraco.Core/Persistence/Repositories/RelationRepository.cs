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
    /// Represents a repository for doing CRUD operations for <see cref="Relation"/>
    /// </summary>
    internal class RelationRepository : PetaPocoRepositoryBase<int, IRelation>, IRelationRepository
    {
        private readonly IRelationTypeRepository _relationTypeRepository;
        private readonly IEntityRepository _entityRepository;

        public RelationRepository(IScopeUnitOfWork work, CacheHelper cache, ILogger logger, ISqlSyntaxProvider sqlSyntax, IRelationTypeRepository relationTypeRepository, IEntityRepository entityRepository)
            : base(work, cache, logger, sqlSyntax)
        {
            _relationTypeRepository = relationTypeRepository;
            _entityRepository = entityRepository;
        }

        public IRelation Get(Guid id)
        {
            var sql = GetBaseQuery(false).Where("uniqueId=@Id", new { Id = id });
            var dto = Database.Fetch<RelationDto>(sql).FirstOrDefault();
            if (dto == null)
                return null;

            var relationType = _relationTypeRepository.Get(dto.RelationType);
            if (relationType == null)
                throw new Exception(string.Format("RelationType with Id: {0} doesn't exist", dto.RelationType));

            var factory = new RelationFactory(relationType);
            return DtoToEntity(dto, factory);
        }

        public IEnumerable<IRelation> GetAll(params Guid[] ids)
        {
            return ids.Length > 0 ? ids.Select(Get) : PerformGetAll();
        }

        public bool Exists(Guid id)
        {
            return Get(id) != null;
        }

        #region Overrides of RepositoryBase<int,Relation>

        protected override IRelation PerformGet(int id)
        {
            var sql = GetBaseQuery(false);
            sql.Where(GetBaseWhereClause(), new { Id = id });

            var dto = Database.Fetch<RelationDto>(SqlSyntax.SelectTop(sql, 1)).FirstOrDefault();
            if (dto == null)
                return null;

            var relationType = _relationTypeRepository.Get(dto.RelationType);
            if (relationType == null)
                throw new Exception(string.Format("RelationType with Id: {0} doesn't exist", dto.RelationType));

            var factory = new RelationFactory(relationType);
            return DtoToEntity(dto, factory);
        }

        protected override IEnumerable<IRelation> PerformGetAll(params int[] ids)
        {
            var sql = GetBaseQuery(false);
            if (ids.Length > 0)
                sql.WhereIn<RelationDto>(x => x.Id, ids);
            sql.OrderBy<RelationDto>(x => x.RelationType);
            var dtos = Database.Fetch<RelationDto>(sql);
            return DtosToEntities(dtos);
        }

        protected override IEnumerable<IRelation> PerformGetByQuery(IQuery<IRelation> query)
        {
            var sqlClause = GetBaseQuery(false);
            var translator = new SqlTranslator<IRelation>(sqlClause, query);
            var sql = translator.Translate();
            sql.OrderBy<RelationDto>(x => x.RelationType);
            var dtos = Database.Fetch<RelationDto>(sql);
            return DtosToEntities(dtos);
        }

        private IEnumerable<IRelation> DtosToEntities(IEnumerable<RelationDto> dtos)
        {
            // in most cases, the relation type will be the same for all of them,
            // plus we've ordered the relations by type, so try to allocate as few
            // factories as possible - bearing in mind that relation types are cached
            RelationFactory factory = null;
            var relationTypeId = -1;

            return dtos.Select(x =>
            {
                if (relationTypeId != x.RelationType)
                    factory = new RelationFactory(_relationTypeRepository.Get(relationTypeId = x.RelationType));
                return DtoToEntity(x, factory);
            });
        }

        private static IRelation DtoToEntity(RelationDto dto, RelationFactory factory)
        {
            var entity = factory.BuildEntity(dto);

            //on initial construction we don't want to have dirty properties tracked
            // http://issues.umbraco.org/issue/U4-1946
            ((TracksChangesEntityBase)entity).ResetDirtyProperties(false);

            return entity;
        }

        #endregion

        #region Overrides of PetaPocoRepositoryBase<int,Relation>

        protected override Sql GetBaseQuery(bool isCount)
        {
            var sql = new Sql();
            sql.Select(isCount ? "COUNT(*)" : "*")
               .From<RelationDto>();
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
                "DELETE FROM umbracoRelation WHERE id = @Id"
            };
            return list;
        }

        protected override Guid NodeObjectTypeId
        {
            get { throw new NotImplementedException(); }
        }

        #endregion

        #region Unit of Work Implementation

        protected override void PersistNewItem(IRelation entity)
        {
            ((Entity)entity).AddingEntity();

            var parent = _entityRepository.Get(entity.ParentId);
            var child = _entityRepository.Get(entity.ChildId);
            entity.Key = GuidExtensions.Combine(parent.Key, child.Key, entity.RelationType.Key);

            var factory = new RelationFactory(entity.RelationType);
            var dto = factory.BuildDto(entity);
            
            var id = Convert.ToInt32(Database.Insert(dto));
            entity.Id = id;

            entity.ResetDirtyProperties();
        }

        protected override void PersistUpdatedItem(IRelation entity)
        {
            ((Entity)entity).UpdatingEntity();

            var parent = _entityRepository.Get(entity.ParentId);
            var child = _entityRepository.Get(entity.ChildId);
            entity.Key = GuidExtensions.Combine(parent.Key, child.Key, entity.RelationType.Key);

            var factory = new RelationFactory(entity.RelationType);
            var dto = factory.BuildDto(entity);

            Database.Update(dto);

            entity.ResetDirtyProperties();
        }

        #endregion

        /// <summary>
        /// Dispose disposable properties
        /// </summary>
        /// <remarks>
        /// Ensure the unit of work is disposed
        /// </remarks>
        protected override void DisposeResources()
        {
            _relationTypeRepository.Dispose();
        }
    }
}