using Microsoft.Extensions.Logging;
using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

/// <summary>
///     An internal repository for managing entity containers such as doc type, media type, data type containers.
/// </summary>
internal class EntityContainerRepository : EntityRepositoryBase<int, EntityContainer>, IEntityContainerRepository
{
    public EntityContainerRepository(IScopeAccessor scopeAccessor, AppCaches cache,
        ILogger<EntityContainerRepository> logger, Guid containerObjectType)
        : base(scopeAccessor, cache, logger)
    {
        Guid[] allowedContainers =
        {
            Constants.ObjectTypes.DocumentTypeContainer, Constants.ObjectTypes.MediaTypeContainer,
            Constants.ObjectTypes.DataTypeContainer,
        };
        NodeObjectTypeId = containerObjectType;
        if (allowedContainers.Contains(NodeObjectTypeId) == false)
        {
            throw new InvalidOperationException("No container type exists with ID: " + NodeObjectTypeId);
        }
    }

    protected Guid NodeObjectTypeId { get; }

    // temp - so we don't have to implement GetByQuery
    public EntityContainer? Get(Guid id)
    {
        Sql<ISqlContext> sql = GetBaseQuery(false).Where("UniqueId=@uniqueId", new { uniqueId = id });

        NodeDto? nodeDto = Database.Fetch<NodeDto>(sql).FirstOrDefault();
        return nodeDto == null ? null : CreateEntity(nodeDto);
    }

    public IEnumerable<EntityContainer> Get(string name, int level)
    {
        Sql<ISqlContext> sql = GetBaseQuery(false)
            .Where(
                "text=@name AND level=@level AND nodeObjectType=@umbracoObjectTypeId",
                new { name, level, umbracoObjectTypeId = NodeObjectTypeId });
        return Database.Fetch<NodeDto>(sql).Select(CreateEntity);
    }

    // never cache
    protected override IRepositoryCachePolicy<EntityContainer, int> CreateCachePolicy() =>
        NoCacheRepositoryCachePolicy<EntityContainer, int>.Instance;

    protected override EntityContainer? PerformGet(int id)
    {
        Sql<ISqlContext> sql = GetBaseQuery(false)
            .Where(GetBaseWhereClause(), new { id, NodeObjectType = NodeObjectTypeId });

        NodeDto? nodeDto = Database.Fetch<NodeDto>(SqlSyntax.SelectTop(sql, 1)).FirstOrDefault();
        return nodeDto == null ? null : CreateEntity(nodeDto);
    }

    protected override IEnumerable<EntityContainer> PerformGetAll(params int[]? ids)
    {
        if (ids?.Any() ?? false)
        {
            return Database.FetchByGroups<NodeDto, int>(ids, Constants.Sql.MaxParameterCount, batch =>
                    GetBaseQuery(false)
                        .Where<NodeDto>(x => x.NodeObjectType == NodeObjectTypeId)
                        .WhereIn<NodeDto>(x => x.NodeId, batch))
                .Select(CreateEntity);
        }

        // else
        Sql<ISqlContext> sql = GetBaseQuery(false)
            .Where("nodeObjectType=@umbracoObjectTypeId", new { umbracoObjectTypeId = NodeObjectTypeId })
            .OrderBy<NodeDto>(x => x.Level);

        return Database.Fetch<NodeDto>(sql).Select(CreateEntity);
    }

    protected override IEnumerable<EntityContainer> PerformGetByQuery(IQuery<EntityContainer> query) =>
        throw new NotImplementedException();

    protected override Sql<ISqlContext> GetBaseQuery(bool isCount)
    {
        Sql<ISqlContext> sql = Sql();
        if (isCount)
        {
            sql.SelectCount();
        }
        else
        {
            sql.SelectAll();
        }

        sql.From<NodeDto>();
        return sql;
    }

    private static EntityContainer CreateEntity(NodeDto nodeDto)
    {
        if (nodeDto.NodeObjectType.HasValue == false)
        {
            throw new InvalidOperationException("Node with id " + nodeDto.NodeId + " has no object type.");
        }

        // throws if node is not a container
        Guid containedObjectType = EntityContainer.GetContainedObjectType(nodeDto.NodeObjectType.Value);

        var entity = new EntityContainer(nodeDto.NodeId, nodeDto.UniqueId,
            nodeDto.ParentId, nodeDto.Path, nodeDto.Level, nodeDto.SortOrder,
            containedObjectType,
            nodeDto.Text, nodeDto.UserId ?? Constants.Security.UnknownUserId);

        // reset dirty initial properties (U4-1946)
        entity.ResetDirtyProperties(false);

        return entity;
    }

    protected override string GetBaseWhereClause() => "umbracoNode.id = @id and nodeObjectType = @NodeObjectType";

    protected override IEnumerable<string> GetDeleteClauses() => throw new NotImplementedException();

    protected override void PersistDeletedItem(EntityContainer entity)
    {
        if (entity == null)
        {
            throw new ArgumentNullException(nameof(entity));
        }

        EnsureContainerType(entity);

        NodeDto nodeDto = Database.FirstOrDefault<NodeDto>(Sql().SelectAll()
            .From<NodeDto>()
            .Where<NodeDto>(dto => dto.NodeId == entity.Id && dto.NodeObjectType == entity.ContainerObjectType));

        if (nodeDto == null)
        {
            return;
        }

        // move children to the parent so they are not orphans
        List<NodeDto> childDtos = Database.Fetch<NodeDto>(Sql().SelectAll()
            .From<NodeDto>()
            .Where(
                "parentID=@parentID AND (nodeObjectType=@containedObjectType OR nodeObjectType=@containerObjectType)",
                new
                {
                    parentID = entity.Id,
                    containedObjectType = entity.ContainedObjectType,
                    containerObjectType = entity.ContainerObjectType,
                }));

        foreach (NodeDto childDto in childDtos)
        {
            childDto.ParentId = nodeDto.ParentId;
            Database.Update(childDto);
        }

        // delete
        Database.Delete(nodeDto);

        entity.DeleteDate = DateTime.Now;
    }

    protected override void PersistNewItem(EntityContainer entity)
    {
        if (entity == null)
        {
            throw new ArgumentNullException(nameof(entity));
        }

        EnsureContainerType(entity);

        if (entity.Name == null)
        {
            throw new InvalidOperationException("Entity name can't be null.");
        }

        if (string.IsNullOrWhiteSpace(entity.Name))
        {
            throw new InvalidOperationException(
                "Entity name can't be empty or consist only of white-space characters.");
        }

        entity.Name = entity.Name.Trim();

        // guard against duplicates
        NodeDto nodeDto = Database.FirstOrDefault<NodeDto>(Sql().SelectAll()
            .From<NodeDto>()
            .Where<NodeDto>(dto =>
                dto.ParentId == entity.ParentId && dto.Text == entity.Name &&
                dto.NodeObjectType == entity.ContainerObjectType));
        if (nodeDto != null)
        {
            throw new InvalidOperationException("A container with the same name already exists.");
        }

        // create
        var level = 0;
        var path = "-1";
        if (entity.ParentId > -1)
        {
            NodeDto parentDto = Database.FirstOrDefault<NodeDto>(Sql().SelectAll()
                .From<NodeDto>()
                .Where<NodeDto>(dto =>
                    dto.NodeId == entity.ParentId && dto.NodeObjectType == entity.ContainerObjectType));

            if (parentDto == null)
            {
                throw new InvalidOperationException("Could not find parent container with id " + entity.ParentId);
            }

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
            UniqueId = entity.Key,
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
    protected override void PersistUpdatedItem(EntityContainer entity)
    {
        if (entity == null)
        {
            throw new ArgumentNullException(nameof(entity));
        }

        EnsureContainerType(entity);

        if (entity.Name == null)
        {
            throw new InvalidOperationException("Entity name can't be null.");
        }

        if (string.IsNullOrWhiteSpace(entity.Name))
        {
            throw new InvalidOperationException(
                "Entity name can't be empty or consist only of white-space characters.");
        }

        entity.Name = entity.Name.Trim();

        // find container to update
        NodeDto nodeDto = Database.FirstOrDefault<NodeDto>(Sql().SelectAll()
            .From<NodeDto>()
            .Where<NodeDto>(dto => dto.NodeId == entity.Id && dto.NodeObjectType == entity.ContainerObjectType));
        if (nodeDto == null)
        {
            throw new InvalidOperationException("Could not find container with id " + entity.Id);
        }

        // guard against duplicates
        NodeDto dupNodeDto = Database.FirstOrDefault<NodeDto>(Sql().SelectAll()
            .From<NodeDto>()
            .Where<NodeDto>(dto =>
                dto.ParentId == entity.ParentId && dto.Text == entity.Name &&
                dto.NodeObjectType == entity.ContainerObjectType));
        if (dupNodeDto != null && dupNodeDto.NodeId != nodeDto.NodeId)
        {
            throw new InvalidOperationException("A container with the same name already exists.");
        }

        // update
        nodeDto.Text = entity.Name;
        if (nodeDto.ParentId != entity.ParentId)
        {
            nodeDto.Level = 0;
            nodeDto.Path = "-1";
            if (entity.ParentId > -1)
            {
                NodeDto parent = Database.FirstOrDefault<NodeDto>(Sql().SelectAll()
                    .From<NodeDto>()
                    .Where<NodeDto>(dto =>
                        dto.NodeId == entity.ParentId && dto.NodeObjectType == entity.ContainerObjectType));

                if (parent == null)
                {
                    throw new InvalidOperationException(
                        "Could not find parent container with id " + entity.ParentId);
                }

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
