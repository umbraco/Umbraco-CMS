using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Cache;
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
    /// An internal repository for managing entity containers such as doc type, media type, data type containers
    /// </summary>
    /// <remarks>
    /// All we're supporting here is a single get, creating and deleting
    /// </remarks>
    internal class EntityContainerRepository : PetaPocoRepositoryBase<int, EntityContainer>
    {
        private readonly Guid _containerObjectType;
        private readonly Guid _entityObjectType;

        public EntityContainerRepository(IDatabaseUnitOfWork work, CacheHelper cache, ILogger logger, ISqlSyntaxProvider sqlSyntax,
            Guid containerObjectType, Guid entityObjectType) 
            : base(work, cache, logger, sqlSyntax)
        {
            _containerObjectType = containerObjectType;
            _entityObjectType = entityObjectType;
        }

        /// <summary>
        /// Do not cache anything
        /// </summary>
        protected override IRuntimeCacheProvider RuntimeCache
        {
            get { return new NullCacheProvider(); }
        }

        protected override EntityContainer PerformGet(int id)
        {
            var sql = GetBaseQuery(false);
            sql.Where(GetBaseWhereClause(), new { Id = id, NodeObjectType = _containerObjectType });

            var containerDto = Database.Fetch<NodeDto>(sql).FirstOrDefault();
            if (containerDto == null)
                return null;

            var entity = new EntityContainer(containerDto.NodeId,
                containerDto.ParentId, containerDto.Text, containerDto.UserId ?? 0,
                containerDto.Path);

            //on initial construction we don't want to have dirty properties tracked
            // http://issues.umbraco.org/issue/U4-1946
            entity.ResetDirtyProperties(false);

            return entity;
        }

        protected override IEnumerable<EntityContainer> PerformGetAll(params int[] ids)
        {
            throw new NotImplementedException();
        }

        protected override IEnumerable<EntityContainer> PerformGetByQuery(IQuery<EntityContainer> query)
        {
            throw new NotImplementedException();
        }

        protected override Sql GetBaseQuery(bool isCount)
        {
            var sql = new Sql();
            if (isCount)
            {
                sql.Select("COUNT(*)").From<NodeDto>(SqlSyntax);
            }
            else
            {
                sql.Select("*").From<NodeDto>(SqlSyntax);
            }
            return sql;
        }

        protected override string GetBaseWhereClause()
        {
            return "umbracoNode.id = @Id and nodeObjectType = @NodeObjectType";
        }

        protected override IEnumerable<string> GetDeleteClauses()
        {
            throw new NotImplementedException();
        }

        protected override Guid NodeObjectTypeId
        {
            get { return _containerObjectType; }
        }

        protected override void PersistDeletedItem(EntityContainer entity)
        {
            var exists = Database.FirstOrDefault<NodeDto>(
                new Sql().Select("*")
                    .From<NodeDto>(SqlSyntax)
                    .Where<NodeDto>(dto => dto.NodeId == entity.Id && dto.NodeObjectType == _containerObjectType));

            if (exists == null) return;

            //We need to move the content types and folders that exist under this folder to it's parent folder
            var children = Database.Fetch<NodeDto>(
                new Sql().Select("*")
                    .From<NodeDto>(SqlSyntax)
                    .Where("parentID=@parentID AND (nodeObjectType=@entityObjectType OR nodeObjectType=@containerObjectType)",
                        new {parentID = entity.ParentId, entityObjectType = _entityObjectType, containerObjectType = _containerObjectType}));

            foreach (var childDto in children)
            {
                childDto.ParentId = exists.ParentId;
                Database.Update(childDto);
            }

            //now that everything is moved up a level, we need to delete the container
            Database.Delete(exists);
        }

        protected override void PersistNewItem(EntityContainer entity)
        {
            entity.Name = entity.Name.Trim();

            Mandate.ParameterNotNullOrEmpty(entity.Name, "entity.Name");

            var exists = Database.FirstOrDefault<NodeDto>(
                new Sql().Select("*")
                    .From<NodeDto>(SqlSyntax)
                    .Where<NodeDto>(dto => dto.ParentId == entity.ParentId && dto.Text == entity.Name && dto.NodeObjectType == _containerObjectType));

            if (exists != null)
            {
                throw new InvalidOperationException("A folder with the same name already exists");
            }

            var level = 0;
            var path = "-1";
            if (entity.ParentId > -1)
            {
                var parent = Database.FirstOrDefault<NodeDto>(
                    new Sql().Select("*")
                        .From<NodeDto>(SqlSyntax)
                        .Where<NodeDto>(dto => dto.NodeId == entity.ParentId && dto.NodeObjectType == _containerObjectType));

                if (parent == null)
                {
                    throw new NullReferenceException("No content type container found with parent id " + entity.ParentId);
                }
                level = parent.Level;
                path = parent.Path;
            }

            var nodeDto = new NodeDto
            {
                CreateDate = DateTime.Now,
                Level = Convert.ToInt16(level + 1),
                NodeObjectType = _containerObjectType,
                ParentId = entity.ParentId,
                Path = path,
                SortOrder = 0,
                Text = entity.Name,
                Trashed = false,
                UniqueId = Guid.NewGuid(),
                UserId = entity.CreatorId
            };

            var id = Convert.ToInt32(Database.Insert(nodeDto));

            //update the path
            nodeDto.Path = nodeDto.Path + "," + nodeDto.NodeId;
            Database.Save(nodeDto);

            entity.Id = id;
            entity.Path = nodeDto.Path;

            entity.ResetDirtyProperties();
        }

        protected override void PersistUpdatedItem(EntityContainer entity)
        {
            throw new NotImplementedException();
        }
    }
}