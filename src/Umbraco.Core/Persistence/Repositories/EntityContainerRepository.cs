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
    /// An internal repository for managing entity containers such as doc type, media type, data type containers.
    /// </summary>
    internal class EntityContainerRepository : PetaPocoRepositoryBase<int, EntityContainer>
    {
        private readonly Guid _containerObjectType;

        public EntityContainerRepository(IDatabaseUnitOfWork work, CacheHelper cache, ILogger logger, ISqlSyntaxProvider sqlSyntax, Guid containerObjectType) 
            : base(work, cache, logger, sqlSyntax)
        {
            var allowedContainers = new[] {Constants.ObjectTypes.DocumentTypeContainerGuid, Constants.ObjectTypes.MediaTypeContainerGuid, Constants.ObjectTypes.DataTypeContainerGuid};
            _containerObjectType = containerObjectType;
            if (allowedContainers.Contains(_containerObjectType) == false)
                throw new InvalidOperationException("No container type exists with ID: " + _containerObjectType);
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
            var sql = GetBaseQuery(false).Where(GetBaseWhereClause(), new { id = id, NodeObjectType = NodeObjectTypeId });

            var nodeDto = Database.Fetch<NodeDto>(sql).FirstOrDefault();
            return nodeDto == null ? null : CreateEntity(nodeDto);
        }

        // temp - so we don't have to implement GetByQuery
        public EntityContainer Get(Guid id)
        {
            var sql = GetBaseQuery(false).Where("UniqueId=@uniqueId", new { uniqueId = id });

            var nodeDto = Database.Fetch<NodeDto>(sql).FirstOrDefault();
            return nodeDto == null ? null : CreateEntity(nodeDto);
        }

        public IEnumerable<EntityContainer> Get(string name, int level)
        {
            var sql = GetBaseQuery(false).Where("text=@name AND level=@level AND nodeObjectType=@umbracoObjectTypeId", new { name, level, umbracoObjectTypeId = NodeObjectTypeId });
            return Database.Fetch<NodeDto>(sql).Select(CreateEntity);
        }

        protected override IEnumerable<EntityContainer> PerformGetAll(params int[] ids)
        {
            //we need to batch these in groups of 2000 so we don't exceed the max 2100 limit
            return ids.InGroupsOf(2000).SelectMany(@group =>
            {
                var sql = GetBaseQuery(false)
                    .Where("nodeObjectType=@umbracoObjectTypeId", new { umbracoObjectTypeId = NodeObjectTypeId })
                    .Where(string.Format("{0} IN (@ids)", SqlSyntax.GetQuotedColumnName("id")), new { ids = @group });

                sql.OrderBy<NodeDto>(x => x.Level, SqlSyntax);

                return Database.Fetch<NodeDto>(sql).Select(CreateEntity);
            });
        }

        protected override IEnumerable<EntityContainer> PerformGetByQuery(IQuery<EntityContainer> query)
        {
            throw new NotImplementedException();
        }

        private static EntityContainer CreateEntity(NodeDto nodeDto)
        {
            if (nodeDto.NodeObjectType.HasValue == false)
                throw new InvalidOperationException("Node with id " + nodeDto.NodeId + " has no object type.");

            // throws if node is not a container
            var containedObjectType = EntityContainer.GetContainedObjectType(nodeDto.NodeObjectType.Value);

            var entity = new EntityContainer(nodeDto.NodeId, nodeDto.UniqueId,
                nodeDto.ParentId, nodeDto.Path, nodeDto.Level, nodeDto.SortOrder,
                containedObjectType,
                nodeDto.Text, nodeDto.UserId ?? 0);

            //on initial construction we don't want to have dirty properties tracked
            // http://issues.umbraco.org/issue/U4-1946
            entity.ResetDirtyProperties(false);

            return entity;
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
            return "umbracoNode.id = @id and nodeObjectType = @NodeObjectType";
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
            EnsureContainerType(entity);

            var nodeDto = Database.FirstOrDefault<NodeDto>(new Sql().Select("*")
                .From<NodeDto>(SqlSyntax)
                .Where<NodeDto>(dto => dto.NodeId == entity.Id && dto.NodeObjectType == entity.ContainerObjectType));

            if (nodeDto == null) return;

            // move children to the parent so they are not orphans
            var childDtos = Database.Fetch<NodeDto>(new Sql().Select("*")
                .From<NodeDto>(SqlSyntax)
                .Where("parentID=@parentID AND (nodeObjectType=@containedObjectType OR nodeObjectType=@containerObjectType)",
                    new
                    {
                        parentID = entity.Id,
                        containedObjectType = entity.ContainedObjectType,
                        containerObjectType = entity.ContainerObjectType
                    }));

            foreach (var childDto in childDtos)
            {
                childDto.ParentId = nodeDto.ParentId;
                Database.Update(childDto);
            }

            // delete
            Database.Delete(nodeDto);
        }

        protected override void PersistNewItem(EntityContainer entity)
        {
            EnsureContainerType(entity);

            entity.Name = entity.Name.Trim();
            Mandate.ParameterNotNullOrEmpty(entity.Name, "entity.Name");

            // guard against duplicates
            var nodeDto = Database.FirstOrDefault<NodeDto>(new Sql().Select("*")
                .From<NodeDto>(SqlSyntax)
                .Where<NodeDto>(dto => dto.ParentId == entity.ParentId && dto.Text == entity.Name && dto.NodeObjectType == entity.ContainerObjectType));
            if (nodeDto != null)
                throw new InvalidOperationException("A container with the same name already exists.");

            // create
            var level = 0;
            var path = "-1";
            if (entity.ParentId > -1)
            {
                var parentDto = Database.FirstOrDefault<NodeDto>(new Sql().Select("*")
                    .From<NodeDto>(SqlSyntax)
                    .Where<NodeDto>(dto => dto.NodeId == entity.ParentId && dto.NodeObjectType == entity.ContainerObjectType));

                if (parentDto == null)
                    throw new NullReferenceException("Could not find parent container with id " + entity.ParentId);

                level = parentDto.Level;
                path = parentDto.Path;
            }

            // note: sortOrder is NOT managed and always zero for containers

            nodeDto = new NodeDto
            {
                CreateDate = DateTime.Now,
                Level = Convert.ToInt16(level + 1),
                NodeObjectType = entity.ContainerObjectType,
                ParentId = entity.ParentId,
                Path = path,
                SortOrder = 0,
                Text = entity.Name,
                UserId = entity.CreatorId,
                UniqueId = entity.Key
            };

            // insert, get the id, update the path with the id
            var id = Convert.ToInt32(Database.Insert(nodeDto));
            nodeDto.Path = nodeDto.Path + "," + nodeDto.NodeId;
            Database.Save(nodeDto);

            // refresh the entity
            entity.Id = id;
            entity.Path = nodeDto.Path;
            entity.Level = nodeDto.Level;
            entity.SortOrder = 0;
            entity.CreateDate = nodeDto.CreateDate;
            entity.ResetDirtyProperties();
        }

        // beware! does NOT manage descendants in case of a new parent
        //
        protected override void PersistUpdatedItem(EntityContainer entity)
        {
            EnsureContainerType(entity);

            entity.Name = entity.Name.Trim();
            Mandate.ParameterNotNullOrEmpty(entity.Name, "entity.Name");

            // find container to update
            var nodeDto = Database.FirstOrDefault<NodeDto>(new Sql().Select("*")
                .From<NodeDto>(SqlSyntax)
                .Where<NodeDto>(dto => dto.NodeId == entity.Id && dto.NodeObjectType == entity.ContainerObjectType));
            if (nodeDto == null)
                throw new InvalidOperationException("Could not find container with id " + entity.Id);

            // guard against duplicates
            var dupNodeDto = Database.FirstOrDefault<NodeDto>(new Sql().Select("*")
                .From<NodeDto>(SqlSyntax)
                .Where<NodeDto>(dto => dto.ParentId == entity.ParentId && dto.Text == entity.Name && dto.NodeObjectType == entity.ContainerObjectType));
            if (dupNodeDto != null && dupNodeDto.NodeId != nodeDto.NodeId)
                throw new InvalidOperationException("A container with the same name already exists.");

            // update
            nodeDto.Text = entity.Name;
            if (nodeDto.ParentId != entity.ParentId)
            {
                nodeDto.Level = 0;
                nodeDto.Path = "-1";
                if (entity.ParentId > -1)
                {
                    var parent = Database.FirstOrDefault<NodeDto>(new Sql().Select("*")
                        .From<NodeDto>(SqlSyntax)
                        .Where<NodeDto>(dto => dto.NodeId == entity.ParentId && dto.NodeObjectType == entity.ContainerObjectType));

                    if (parent == null)
                        throw new NullReferenceException("Could not find parent container with id " + entity.ParentId);

                    nodeDto.Level = Convert.ToInt16(parent.Level + 1);
                    nodeDto.Path = parent.Path + "," + nodeDto.NodeId;
                }
                nodeDto.ParentId = entity.ParentId;
            }

            // note: sortOrder is NOT managed and always zero for containers

            // update
            Database.Update(nodeDto);
            
            // refresh the entity
            entity.Path = nodeDto.Path;
            entity.Level = nodeDto.Level;
            entity.SortOrder = 0;
            entity.ResetDirtyProperties();
        }

        private void EnsureContainerType(EntityContainer entity)
        {
            if (entity.ContainerObjectType != NodeObjectTypeId)
            {
                throw new InvalidOperationException("The container type does not match the repository object type");
            }
        }
    }
}