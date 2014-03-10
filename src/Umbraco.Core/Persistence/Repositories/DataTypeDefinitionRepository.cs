using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using Umbraco.Core.Models;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence.Caching;
using Umbraco.Core.Persistence.Factories;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Persistence.Repositories
{
    /// <summary>
    /// Represents a repository for doing CRUD operations for <see cref="DataTypeDefinition"/>
    /// </summary>
    internal class DataTypeDefinitionRepository : PetaPocoRepositoryBase<int, IDataTypeDefinition>, IDataTypeDefinitionRepository
    {
		public DataTypeDefinitionRepository(IDatabaseUnitOfWork work)
			: base(work)
        {
        }

		public DataTypeDefinitionRepository(IDatabaseUnitOfWork work, IRepositoryCacheProvider cache)
            : base(work, cache)
        {
        }

        #region Overrides of RepositoryBase<int,DataTypeDefinition>

        protected override IDataTypeDefinition PerformGet(int id)
        {
            var dataTypeSql = GetBaseQuery(false);
            dataTypeSql.Where(GetBaseWhereClause(), new { Id = id });

            var dataTypeDto = Database.Fetch<DataTypeDto, NodeDto>(dataTypeSql).FirstOrDefault();

            if (dataTypeDto == null)
                return null;

            var factory = new DataTypeDefinitionFactory(NodeObjectTypeId);
            var definition = factory.BuildEntity(dataTypeDto);

            //on initial construction we don't want to have dirty properties tracked
            // http://issues.umbraco.org/issue/U4-1946
            ((Entity)definition).ResetDirtyProperties(false);
            return definition;
        }

        protected override IEnumerable<IDataTypeDefinition> PerformGetAll(params int[] ids)
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
                var nodeDtos = Database.Fetch<NodeDto>("WHERE nodeObjectType = @NodeObjectType", new { NodeObjectType = NodeObjectTypeId });
                foreach (var nodeDto in nodeDtos)
                {
                    yield return Get(nodeDto.NodeId);
                }
            }
        }

        protected override IEnumerable<IDataTypeDefinition> PerformGetByQuery(IQuery<IDataTypeDefinition> query)
        {
            var sqlClause = GetBaseQuery(false);
            var translator = new SqlTranslator<IDataTypeDefinition>(sqlClause, query);
            var sql = translator.Translate();

            var dataTypeDtos = Database.Fetch<DataTypeDto, NodeDto>(sql);

            foreach (var dataTypeDto in dataTypeDtos)
            {
                yield return Get(dataTypeDto.DataTypeId);
            }
        }

        #endregion

        #region Overrides of PetaPocoRepositoryBase<int,DataTypeDefinition>

        protected override Sql GetBaseQuery(bool isCount)
        {
            var sql = new Sql();
            sql.Select(isCount ? "COUNT(*)" : "*")
               .From<DataTypeDto>()
               .InnerJoin<NodeDto>()
               .On<DataTypeDto, NodeDto>(left => left.DataTypeId, right => right.NodeId)
               .Where<NodeDto>(x => x.NodeObjectType == NodeObjectTypeId);
            return sql;
        }

        protected override string GetBaseWhereClause()
        {
            return "umbracoNode.id = @Id";
        }

        protected override IEnumerable<string> GetDeleteClauses()
        {
            return new List<string>();
        }

        protected override Guid NodeObjectTypeId
        {
            get { return new Guid(Constants.ObjectTypes.DataType); }
        }

        #endregion

        #region Unit of Work Implementation

        protected override void PersistNewItem(IDataTypeDefinition entity)
        {
            ((DataTypeDefinition)entity).AddingEntity();

            //Cannot add a duplicate data type
            var exists = Database.ExecuteScalar<int>(@"SELECT COUNT(*) FROM cmsDataType
INNER JOIN umbracoNode ON cmsDataType.nodeId = umbracoNode.id
WHERE umbracoNode." + SqlSyntaxContext.SqlSyntaxProvider.GetQuotedColumnName("text") + "= @name", new {name = entity.Name});
            if (exists > 0)
            {
                throw new DuplicateNameException("A data type with the name " + entity.Name + " already exists");
            }

            var factory = new DataTypeDefinitionFactory(NodeObjectTypeId);
            var dto = factory.BuildDto(entity);

            //Logic for setting Path, Level and SortOrder
            var parent = Database.First<NodeDto>("WHERE id = @ParentId", new { ParentId = entity.ParentId });
            int level = parent.Level + 1;
            int sortOrder =
                Database.ExecuteScalar<int>("SELECT COUNT(*) FROM umbracoNode WHERE parentID = @ParentId AND nodeObjectType = @NodeObjectType",
                                                      new { ParentId = entity.ParentId, NodeObjectType = NodeObjectTypeId });

            //Create the (base) node data - umbracoNode
            var nodeDto = dto.NodeDto;
            nodeDto.Path = parent.Path;
            nodeDto.Level = short.Parse(level.ToString(CultureInfo.InvariantCulture));
            nodeDto.SortOrder = sortOrder;
            var o = Database.IsNew(nodeDto) ? Convert.ToInt32(Database.Insert(nodeDto)) : Database.Update(nodeDto);

            //Update with new correct path
            nodeDto.Path = string.Concat(parent.Path, ",", nodeDto.NodeId);
            Database.Update(nodeDto);

            //Update entity with correct values
            entity.Id = nodeDto.NodeId; //Set Id on entity to ensure an Id is set
            entity.Path = nodeDto.Path;
            entity.SortOrder = sortOrder;
            entity.Level = level;

            dto.DataTypeId = nodeDto.NodeId;
            Database.Insert(dto);

            ((ICanBeDirty)entity).ResetDirtyProperties();
        }

        protected override void PersistUpdatedItem(IDataTypeDefinition entity)
        {

            //Cannot change to a duplicate alias
            var exists = Database.ExecuteScalar<int>(@"SELECT COUNT(*) FROM cmsDataType
INNER JOIN umbracoNode ON cmsDataType.nodeId = umbracoNode.id
WHERE umbracoNode." + SqlSyntaxContext.SqlSyntaxProvider.GetQuotedColumnName("text") + @"= @name
AND umbracoNode.id <> @id", 
                    new { id = entity.Id, name = entity.Name });
            if (exists > 0)
            {
                throw new DuplicateNameException("A data type with the name " + entity.Name + " already exists");
            }

            //Updates Modified date and Version Guid
            ((DataTypeDefinition)entity).UpdatingEntity();

            //Look up parent to get and set the correct Path if ParentId has changed
            if (((ICanBeDirty)entity).IsPropertyDirty("ParentId"))
            {
                var parent = Database.First<NodeDto>("WHERE id = @ParentId", new { ParentId = entity.ParentId });
                entity.Path = string.Concat(parent.Path, ",", entity.Id);
                entity.Level = parent.Level + 1;
                var maxSortOrder =
                    Database.ExecuteScalar<int>(
                        "SELECT coalesce(max(sortOrder),0) FROM umbracoNode WHERE parentid = @ParentId AND nodeObjectType = @NodeObjectType",
                        new { ParentId = entity.ParentId, NodeObjectType = NodeObjectTypeId });
                entity.SortOrder = maxSortOrder + 1;
            }

            var factory = new DataTypeDefinitionFactory(NodeObjectTypeId);
            //Look up DataTypeDefinition entry to get Primary for updating the DTO
            var dataTypeDto = Database.SingleOrDefault<DataTypeDto>("WHERE nodeId = @Id", new { Id = entity.Id });
            factory.SetPrimaryKey(dataTypeDto.PrimaryKey);
            var dto = factory.BuildDto(entity);

            //Updates the (base) node data - umbracoNode
            var nodeDto = dto.NodeDto;
            Database.Update(nodeDto);
            Database.Update(dto);

            ((ICanBeDirty)entity).ResetDirtyProperties();
        }

        protected override void PersistDeletedItem(IDataTypeDefinition entity)
        {
            //Remove Notifications
            Database.Delete<User2NodeNotifyDto>("WHERE nodeId = @Id", new { Id = entity.Id });

            //Remove Permissions
            Database.Delete<User2NodePermissionDto>("WHERE nodeId = @Id", new { Id = entity.Id });

            //Remove associated tags
            Database.Delete<TagRelationshipDto>("WHERE nodeId = @Id", new { Id = entity.Id });

            //PropertyTypes containing the DataType being deleted
            var propertyTypeDtos = Database.Fetch<PropertyTypeDto>("WHERE dataTypeId = @Id", new { Id = entity.Id });
            //Go through the PropertyTypes and delete referenced PropertyData before deleting the PropertyType
            foreach (var dto in propertyTypeDtos)
            {
                Database.Delete<PropertyDataDto>("WHERE propertytypeid = @Id", new { Id = dto.Id });
                Database.Delete<PropertyTypeDto>("WHERE id = @Id", new { Id = dto.Id });
            }

            //Delete the pre-values
            Database.Delete<DataTypePreValueDto>("WHERE datatypeNodeId = @Id", new {Id = entity.Id});

            //Delete Content specific data
            Database.Delete<DataTypeDto>("WHERE nodeId = @Id", new { Id = entity.Id });

            //Delete (base) node data
            Database.Delete<NodeDto>("WHERE uniqueID = @Id", new { Id = entity.Key });
        }

        #endregion
    }
}