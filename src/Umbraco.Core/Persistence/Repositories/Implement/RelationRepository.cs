using System;
using System.Collections.Generic;
using System.Linq;
using NPoco;
using Umbraco.Core.Cache;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Entities;
using Umbraco.Core.Persistence.Dtos;
using Umbraco.Core.Persistence.Factories;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Scoping;
using Umbraco.Core.Services;
using static Umbraco.Core.Persistence.NPocoSqlExtensions.Statics;

namespace Umbraco.Core.Persistence.Repositories.Implement
{
    /// <summary>
    /// Represents a repository for doing CRUD operations for <see cref="Relation"/>
    /// </summary>
    internal class RelationRepository : NPocoRepositoryBase<int, IRelation>, IRelationRepository
    {
        private readonly IRelationTypeRepository _relationTypeRepository;
        private readonly IEntityRepository _entityRepository;

        public RelationRepository(IScopeAccessor scopeAccessor, ILogger logger, IRelationTypeRepository relationTypeRepository, IEntityRepository entityRepository)
            : base(scopeAccessor, AppCaches.NoCache, logger)
        {
            _relationTypeRepository = relationTypeRepository;
            _entityRepository = entityRepository;
        }

        #region Overrides of RepositoryBase<int,Relation>

        protected override IRelation PerformGet(int id)
        {
            var sql = GetBaseQuery(false);
            sql.Where(GetBaseWhereClause(), new { id });

            var dto = Database.Fetch<RelationDto>(SqlSyntax.SelectTop(sql, 1)).FirstOrDefault();
            if (dto == null)
                return null;

            var relationType = _relationTypeRepository.Get(dto.RelationType);
            if (relationType == null)
                throw new InvalidOperationException(string.Format("RelationType with Id: {0} doesn't exist", dto.RelationType));

            return DtoToEntity(dto, relationType);
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
            //NOTE: This is N+1, BUT ALL relation types are cached so shouldn't matter

            return dtos.Select(x => DtoToEntity(x, _relationTypeRepository.Get(x.RelationType))).ToList();
        }

        private static IRelation DtoToEntity(RelationDto dto, IRelationType relationType)
        {
            var entity = RelationFactory.BuildEntity(dto, relationType);

            // reset dirty initial properties (U4-1946)
            entity.ResetDirtyProperties(false);

            return entity;
        }

        #endregion

        #region Overrides of NPocoRepositoryBase<int,Relation>

        protected override Sql<ISqlContext> GetBaseQuery(bool isCount)
        {
            if (isCount)
            {
                return Sql().SelectCount().From<RelationDto>();
            }

            var sql = Sql().Select<RelationDto>()
                .AndSelect<NodeDto>("uchild", x => Alias(x.NodeObjectType, "childObjectType"))
                .AndSelect<NodeDto>("uparent", x => Alias(x.NodeObjectType, "parentObjectType"))
                .From<RelationDto>()
                .InnerJoin<NodeDto>("uchild").On<RelationDto, NodeDto>((rel, node) => rel.ChildId == node.NodeId, aliasRight: "uchild")
                .InnerJoin<NodeDto>("uparent").On<RelationDto, NodeDto>((rel, node) => rel.ParentId == node.NodeId, aliasRight: "uparent");


            return sql;
        }

        protected override string GetBaseWhereClause()
        {
            return "umbracoRelation.id = @id";
        }

        protected override IEnumerable<string> GetDeleteClauses()
        {
            var list = new List<string>
                           {
                               "DELETE FROM umbracoRelation WHERE id = @id"
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
            entity.AddingEntity();

            var dto = RelationFactory.BuildDto(entity);

            var id = Convert.ToInt32(Database.Insert(dto));

            entity.Id = id;
            PopulateObjectTypes(entity);

            entity.ResetDirtyProperties();
        }

        protected override void PersistUpdatedItem(IRelation entity)
        {
            entity.UpdatingEntity();

            var dto = RelationFactory.BuildDto(entity);
            Database.Update(dto);

            PopulateObjectTypes(entity);

            entity.ResetDirtyProperties();
        }

        #endregion

        public IEnumerable<IUmbracoEntity> GetPagedParentEntitiesByChildId(int childId, long pageIndex, int pageSize, out long totalRecords)
        {
            // Create a query to match the child id
            var relQuery = Query<IRelation>().Where(r => r.ChildId == childId);

            // Because of the way that the entity repository joins relations (on both child or parent) we need to add
            // a clause to filter out the child entity from being returned from the results
            var entityQuery = Query<IUmbracoEntity>().Where(e => e.Id != childId);

            return _entityRepository.GetPagedResultsByQuery(entityQuery, Array.Empty<Guid>(), pageIndex, pageSize, out totalRecords, null, null, relQuery);
        }

        public IEnumerable<IUmbracoEntity> GetPagedChildEntitiesByParentId(int parentId, long pageIndex, int pageSize, out long totalRecords)
        {
            // Create a query to match the parent id
            var relQuery = Query<IRelation>().Where(r => r.ParentId == parentId);

            // Because of the way that the entity repository joins relations (on both child or parent) we need to add
            // a clause to filter out the child entity from being returned from the results
            var entityQuery = Query<IUmbracoEntity>().Where(e => e.Id != parentId);

            return _entityRepository.GetPagedResultsByQuery(entityQuery, Array.Empty<Guid>(), pageIndex, pageSize, out totalRecords, null, null, relQuery);
        }

        public void DeleteByParent(int parentId, params string[] relationTypeAliases)
        {
            var subQuery = Sql().Select<RelationDto>(x => x.Id)
                .From<RelationDto>()
                .InnerJoin<RelationTypeDto>().On<RelationDto, RelationTypeDto>(x => x.RelationType, x => x.Id)
                .Where<RelationDto>(x => x.ParentId == parentId);

            if (relationTypeAliases.Length > 0)
            {
                subQuery.WhereIn<RelationTypeDto>(x => x.Alias, relationTypeAliases);
            }

            Database.Execute(Sql().Delete<RelationDto>().WhereIn<RelationDto>(x => x.Id, subQuery));
        }

        private void PopulateObjectTypes(IRelation entity)
        {
            var nodes = Database.Fetch<NodeDto>(Sql().Select<NodeDto>().From<NodeDto>().Where<NodeDto>(x => x.NodeId == entity.ChildId || x.NodeId == entity.ParentId))
                .ToDictionary(x => x.NodeId, x => x.NodeObjectType);

            if(nodes.TryGetValue(entity.ParentId, out var parentObjectType))
            {
                entity.ParentObjectType = parentObjectType.GetValueOrDefault();
            }

            if(nodes.TryGetValue(entity.ChildId, out var childObjectType))
            {
                entity.ChildObjectType = childObjectType.GetValueOrDefault();
            }
        }
    }
}
