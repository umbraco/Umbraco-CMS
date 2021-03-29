using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using NPoco;
using Umbraco.Core.Cache;
using Umbraco.Core.Events;
using Umbraco.Core.Exceptions;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Entities;
using Umbraco.Core.Persistence.Dtos;
using Umbraco.Core.Persistence.Factories;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Scoping;
using Umbraco.Core.Services;
using static Umbraco.Core.Persistence.NPocoSqlExtensions.Statics;

namespace Umbraco.Core.Persistence.Repositories.Implement
{
    /// <summary>
    /// Represents a repository for doing CRUD operations for <see cref="DataType"/>
    /// </summary>
    internal class DataTypeRepository : NPocoRepositoryBase<int, IDataType>, IDataTypeRepository
    {
        private readonly Lazy<PropertyEditorCollection> _editors;

        // TODO: https://github.com/umbraco/Umbraco-CMS/issues/4237 - get rid of Lazy injection and fix circular dependencies
        public DataTypeRepository(IScopeAccessor scopeAccessor, AppCaches cache, Lazy<PropertyEditorCollection> editors, ILogger logger)
            : base(scopeAccessor, cache, logger)
        {
            _editors = editors;
        }

        #region Overrides of RepositoryBase<int,DataTypeDefinition>

        protected override IDataType PerformGet(int id)
        {
            return GetMany(id).FirstOrDefault();
        }

        protected override IEnumerable<IDataType> PerformGetAll(params int[] ids)
        {
            var dataTypeSql = GetBaseQuery(false);

            if (ids.Any())
            {
                dataTypeSql.Where("umbracoNode.id in (@ids)", new { ids });
            }
            else
            {
                dataTypeSql.Where<NodeDto>(x => x.NodeObjectType == NodeObjectTypeId);
            }

            var dtos = Database.Fetch<DataTypeDto>(dataTypeSql);
            return dtos.Select(x => DataTypeFactory.BuildEntity(x, _editors.Value, Logger)).ToArray();
        }

        protected override IEnumerable<IDataType> PerformGetByQuery(IQuery<IDataType> query)
        {
            var sqlClause = GetBaseQuery(false);
            var translator = new SqlTranslator<IDataType>(sqlClause, query);
            var sql = translator.Translate();

            var dtos = Database.Fetch<DataTypeDto>(sql);

            return dtos.Select(x => DataTypeFactory.BuildEntity(x, _editors.Value, Logger)).ToArray();
        }

        #endregion

        #region Overrides of NPocoRepositoryBase<int,DataTypeDefinition>

        protected override Sql<ISqlContext> GetBaseQuery(bool isCount)
        {
            var sql = Sql();

            sql = isCount
                ? sql.SelectCount()
                : sql.Select<DataTypeDto>(r => r.Select(x => x.NodeDto));

            sql
               .From<DataTypeDto>()
               .InnerJoin<NodeDto>()
               .On<DataTypeDto, NodeDto>(left => left.NodeId, right => right.NodeId)
               .Where<NodeDto>(x => x.NodeObjectType == NodeObjectTypeId);
            return sql;
        }

        protected override string GetBaseWhereClause()
        {
            return "umbracoNode.id = @id";
        }

        protected override IEnumerable<string> GetDeleteClauses()
        {
            return Array.Empty<string>();
        }

        protected override Guid NodeObjectTypeId => Constants.ObjectTypes.DataType;

        #endregion

        #region Unit of Work Implementation

        protected override void PersistNewItem(IDataType entity)
        {
            entity.AddingEntity();

            //ensure a datatype has a unique name before creating it
            entity.Name = EnsureUniqueNodeName(entity.Name);

            // TODO: should the below be removed?
            //Cannot add a duplicate data type
            var existsSql = Sql()
                .SelectCount()
                .From<DataTypeDto>()
                .InnerJoin<NodeDto>().On<DataTypeDto, NodeDto>((left, right) => left.NodeId == right.NodeId)
                .Where<NodeDto>(x => x.Text == entity.Name);
            var exists = Database.ExecuteScalar<int>(existsSql) > 0;
            if (exists)
            {
                throw new DuplicateNameException("A data type with the name " + entity.Name + " already exists");
            }

            var dto = DataTypeFactory.BuildDto(entity);

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
            var o = Database.IsNew<NodeDto>(nodeDto) ? Convert.ToInt32(Database.Insert(nodeDto)) : Database.Update(nodeDto);

            //Update with new correct path
            nodeDto.Path = string.Concat(parent.Path, ",", nodeDto.NodeId);
            Database.Update(nodeDto);

            //Update entity with correct values
            entity.Id = nodeDto.NodeId; //Set Id on entity to ensure an Id is set
            entity.Path = nodeDto.Path;
            entity.SortOrder = sortOrder;
            entity.Level = level;

            dto.NodeId = nodeDto.NodeId;
            Database.Insert(dto);

            entity.ResetDirtyProperties();
        }

        protected override void PersistUpdatedItem(IDataType entity)
        {

            entity.Name = EnsureUniqueNodeName(entity.Name, entity.Id);

            //Cannot change to a duplicate alias
            var existsSql = Sql()
                .SelectCount()
                .From<DataTypeDto>()
                .InnerJoin<NodeDto>().On<DataTypeDto, NodeDto>((left, right) => left.NodeId == right.NodeId)
                .Where<NodeDto>(x => x.Text == entity.Name && x.NodeId != entity.Id);
            var exists = Database.ExecuteScalar<int>(existsSql) > 0;
            if (exists)
            {
                throw new DuplicateNameException("A data type with the name " + entity.Name + " already exists");
            }

            //Updates Modified date
            entity.UpdatingEntity();

            //Look up parent to get and set the correct Path if ParentId has changed
            if (entity.IsPropertyDirty("ParentId"))
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

            var dto = DataTypeFactory.BuildDto(entity);

            //Updates the (base) node data - umbracoNode
            var nodeDto = dto.NodeDto;
            Database.Update(nodeDto);
            Database.Update(dto);

            entity.ResetDirtyProperties();
        }

        protected override void PersistDeletedItem(IDataType entity)
        {
            //Remove Notifications
            Database.Delete<User2NodeNotifyDto>("WHERE nodeId = @Id", new { Id = entity.Id });

            //Remove Permissions
            Database.Delete<UserGroup2NodePermissionDto>("WHERE nodeId = @Id", new { Id = entity.Id });

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

            //Delete Content specific data
            Database.Delete<DataTypeDto>("WHERE nodeId = @Id", new { Id = entity.Id });

            //Delete (base) node data
            Database.Delete<NodeDto>("WHERE uniqueID = @Id", new { Id = entity.Key });

            entity.DeleteDate = DateTime.Now;
        }

        #endregion

        public IEnumerable<MoveEventInfo<IDataType>> Move(IDataType toMove, EntityContainer container)
        {
            var parentId = -1;
            if (container != null)
            {
                // Check on paths
                if ((string.Format(",{0},", container.Path)).IndexOf(string.Format(",{0},", toMove.Id), StringComparison.Ordinal) > -1)
                {
                    throw new DataOperationException<MoveOperationStatusType>(MoveOperationStatusType.FailedNotAllowedByPath);
                }
                parentId = container.Id;
            }

            //used to track all the moved entities to be given to the event
            var moveInfo = new List<MoveEventInfo<IDataType>>
            {
                new MoveEventInfo<IDataType>(toMove, toMove.Path, parentId)
            };

            var origPath = toMove.Path;

            //do the move to a new parent
            toMove.ParentId = parentId;

            //set the updated path
            toMove.Path = string.Concat(container == null ? parentId.ToInvariantString() : container.Path, ",", toMove.Id);

            //schedule it for updating in the transaction
            Save(toMove);

            //update all descendants from the original path, update in order of level
            var descendants = Get(Query<IDataType>().Where(type => type.Path.StartsWith(origPath + ",")));

            var lastParent = toMove;
            foreach (var descendant in descendants.OrderBy(x => x.Level))
            {
                moveInfo.Add(new MoveEventInfo<IDataType>(descendant, descendant.Path, descendant.ParentId));

                descendant.ParentId = lastParent.Id;
                descendant.Path = string.Concat(lastParent.Path, ",", descendant.Id);

                //schedule it for updating in the transaction
                Save(descendant);
            }

            return moveInfo;
        }

        public IReadOnlyDictionary<Udi, IEnumerable<string>> FindUsages(int id)
        {
            if (id == default)
                return new Dictionary<Udi, IEnumerable<string>>();

            var sql = Sql()
                .Select<ContentTypeDto>(ct => ct.Select(node => node.NodeDto))
                .AndSelect<PropertyTypeDto>(pt => Alias(pt.Alias, "ptAlias"), pt => Alias(pt.Name, "ptName"))
                .From<PropertyTypeDto>()
                .InnerJoin<ContentTypeDto>().On<ContentTypeDto, PropertyTypeDto>(ct => ct.NodeId, pt => pt.ContentTypeId)
                .InnerJoin<NodeDto>().On<NodeDto, ContentTypeDto>(n => n.NodeId, ct => ct.NodeId)
                .Where<PropertyTypeDto>(pt => pt.DataTypeId == id)
                .OrderBy<NodeDto>(node => node.NodeId)
                .AndBy<PropertyTypeDto>(pt => pt.Alias);

            var dtos = Database.FetchOneToMany<ContentTypeReferenceDto>(ct => ct.PropertyTypes, sql);

            return dtos.ToDictionary(
                x => (Udi)new GuidUdi(ObjectTypes.GetUdiType(x.NodeDto.NodeObjectType.Value), x.NodeDto.UniqueId).EnsureClosed(),
                x => (IEnumerable<string>)x.PropertyTypes.Select(p => p.Alias).ToList());
        }

        private string EnsureUniqueNodeName(string nodeName, int id = 0)
        {
            var template = SqlContext.Templates.Get(Constants.SqlTemplates.DataTypeRepository.EnsureUniqueNodeName, tsql => tsql
                .Select<NodeDto>(x => Alias(x.NodeId, "id"), x => Alias(x.Text, "name"))
                .From<NodeDto>()
                .Where<NodeDto>(x => x.NodeObjectType == SqlTemplate.Arg<Guid>("nodeObjectType")));

            var sql = template.Sql(NodeObjectTypeId);
            var names = Database.Fetch<SimilarNodeName>(sql);

            return SimilarNodeName.GetUniqueName(names, id, nodeName);
        }

        
        [TableName(Constants.DatabaseSchema.Tables.ContentType)]
        private class ContentTypeReferenceDto : ContentTypeDto
        {
            [ResultColumn]
            [Reference(ReferenceType.Many)]
            public List<PropertyTypeReferenceDto> PropertyTypes { get; set; }
        }

        [TableName(Constants.DatabaseSchema.Tables.PropertyType)]
        private class PropertyTypeReferenceDto
        {
            [Column("ptAlias")]
            public string Alias { get; set; }

            [Column("ptName")]
            public string Name { get; set; }
        }
    }
}
