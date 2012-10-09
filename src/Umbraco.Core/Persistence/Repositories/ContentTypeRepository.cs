using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Umbraco.Core.Models;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence.Caching;
using Umbraco.Core.Persistence.Factories;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.Relators;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Persistence.Repositories
{
    /// <summary>
    /// Represents a repository for doing CRUD operations for <see cref="IContentType"/>
    /// </summary>
    internal class ContentTypeRepository : PetaPocoRepositoryBase<int, IContentType>, IContentTypeRepository
    {
        public ContentTypeRepository(IUnitOfWork work) : base(work)
        {
        }

        public ContentTypeRepository(IUnitOfWork work, IRepositoryCacheProvider cache) : base(work, cache)
        {
        }

        #region Overrides of RepositoryBase<int,IContentType>

        protected override IContentType PerformGet(int id)
        {
            var contentTypeSql = GetBaseQuery(false);
            contentTypeSql.Append(GetBaseWhereClause(id));

            var documentTypeDto = Database.Query<DocumentTypeDto, ContentTypeDto, NodeDto>(contentTypeSql).FirstOrDefault();

            if (documentTypeDto == null)
                return null;

            //TODO Get ContentType composition according to new table
            //TODO Add AllowedContentTypes

            var propertySql = new Sql();
            propertySql.Select("*");
            propertySql.From("cmsTab");
            propertySql.RightJoin("cmsPropertyType ON [cmsTab].[id] = [cmsPropertyType].[tabId]");
            propertySql.InnerJoin("cmsDataType ON [cmsPropertyType].[dataTypeId] = [cmsDataType].[nodeId]");
            propertySql.Where("[cmsPropertyType].[contentTypeId] = @Id", new { Id = id });

            var tabDtos = Database.Fetch<TabDto, PropertyTypeDto, DataTypeDto, TabDto>(new TabPropertyTypeRelator().Map, propertySql);

            var factory = new ContentTypeFactory(NodeObjectTypeId);
            var contentType = factory.BuildEntity(documentTypeDto);

            var propertyFactory = new PropertyGroupFactory(id);
            var propertyGroups = propertyFactory.BuildEntity(tabDtos);
            contentType.PropertyGroups = new PropertyGroupCollection(propertyGroups);

            ((ContentType)contentType).ResetDirtyProperties();
            return contentType;
        }

        protected override IEnumerable<IContentType> PerformGetAll(params int[] ids)
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

        protected override IEnumerable<IContentType> PerformGetByQuery(IQuery<IContentType> query)
        {
            var sqlClause = GetBaseQuery(false);
            var translator = new SqlTranslator<IContentType>(sqlClause, query);
            var sql = translator.Translate();

            var documentTypeDtos = Database.Fetch<DocumentTypeDto, ContentTypeDto, NodeDto>(sql);

            foreach (var dto in documentTypeDtos)
            {
                yield return Get(dto.ContentTypeNodeId);
            }
        }

        #endregion

        #region Overrides of PetaPocoRepositoryBase<int,IContentType>

        protected override Sql GetBaseQuery(bool isCount)
        {
            var sql = new Sql();
            //NOTE: If IsDefault=true we won't get ContentTypes like File, Folder etc. but only DocumentTypes.
            //Which is why "AND cmsDocumentType.IsDefault = @IsDefault" has been removed from sql below.
            //But might need to add it if we create a MediaTypeRepository
            //NOTE: Think the above is incorrect as ContentType and MediaType have different NodeObjectTypes.
            sql.Select(isCount ? "COUNT(*)" : "*");
            sql.From("cmsDocumentType");
            sql.RightJoin("cmsContentType ON ([cmsContentType].[nodeId] = [cmsDocumentType].[contentTypeNodeId])");
            sql.InnerJoin("umbracoNode ON ([cmsContentType].[nodeId] = [umbracoNode].[id])");
            sql.Where("[umbracoNode].[nodeObjectType] = @NodeObjectType", new { NodeObjectType = NodeObjectTypeId });
            return sql;
        }

        protected override Sql GetBaseWhereClause(object id)
        {
            var sql = new Sql();
            sql.Where("[umbracoNode].[id] = @Id", new { Id = id });
            return sql;
        }

        protected override IEnumerable<string> GetDeleteClauses()
        {
            var list = new List<string>
                           {
                               string.Format("DELETE FROM umbracoUser2NodeNotify WHERE nodeId = @Id"),
                               string.Format("DELETE FROM umbracoUser2NodePermission WHERE nodeId = @Id"),
                               string.Format("DELETE FROM cmsTagRelationship WHERE nodeId = @Id"),
                               string.Format("DELETE FROM cmsContentTypeAllowedContentType WHERE Id = @Id"),
                               string.Format("DELETE FROM cmsPropertyType WHERE contentTypeId = @Id"),
                               string.Format("DELETE FROM cmsTab WHERE contenttypeNodeId = @Id"),
                               string.Format("DELETE FROM cmsDocumentType WHERE contentTypeNodeId = @Id"),
                               string.Format("DELETE FROM cmsContentType WHERE NodeId = @Id"),
                               string.Format("DELETE FROM umbracoNode WHERE id = @Id")
                           };
            return list;
        }

        protected override Guid NodeObjectTypeId
        {
            get { return new Guid("A2CB7800-F571-4787-9638-BC48539A0EFB"); }
        }

        #endregion

        #region Unit of Work Implementation

        protected override void PersistNewItem(IContentType entity)
        {
            ((ContentType)entity).AddingEntity();

            var factory = new ContentTypeFactory(NodeObjectTypeId);
            var dto = factory.BuildDto(entity);

            //Logic for setting Path, Level and SortOrder
            var parent = Database.First<NodeDto>("WHERE id = @ParentId", new { ParentId = entity.ParentId });
            int level = parent.Level + 1;
            int sortOrder =
                Database.ExecuteScalar<int>("SELECT COUNT(*) FROM umbracoNode WHERE parentID = @ParentId AND nodeObjectType = @NodeObjectType",
                                                      new { ParentId = entity.ParentId, NodeObjectType = NodeObjectTypeId });

            //Create the (base) node data - umbracoNode
            var nodeDto = dto.ContentTypeDto.NodeDto;
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

            //Insert new ContentType entry
            var contentTypeDto = dto.ContentTypeDto;
            Database.Insert(contentTypeDto);

            //TODO Insert new DocumentType entries - NOTE only seems relevant as long as Templates resides in the DB
            //TODO Insert allowed Templates and DocumentTypes
            //TODO Insert ContentType composition in new table

            var propertyFactory = new PropertyGroupFactory(nodeDto.NodeId);

            //Insert Tabs
            foreach (var propertyGroup in entity.PropertyGroups)
            {
                var tabDto = propertyFactory.BuildTabDto(propertyGroup);
                var primaryKey = Convert.ToInt32(Database.Insert(tabDto));
                propertyGroup.Id = primaryKey;//Set Id on PropertyGroup
            }

            //Insert PropertyTypes
            foreach (var propertyGroup in entity.PropertyGroups)
            {
                foreach (var propertyType in propertyGroup.PropertyTypes)
                {
                    var propertyTypeDto = propertyFactory.BuildPropertyTypeDto(propertyGroup.Id, propertyType);
                    var primaryKey = Convert.ToInt32(Database.Insert(propertyTypeDto));
                    propertyType.Id = primaryKey;//Set Id on PropertyType
                }
            }

            ((ContentType)entity).ResetDirtyProperties();
        }

        protected override void PersistUpdatedItem(IContentType entity)
        {
            //Updates Modified date
            ((ContentType)entity).UpdatingEntity();

            var propertyFactory = new PropertyGroupFactory(entity.Id);
            var factory = new ContentTypeFactory(NodeObjectTypeId);
            var dto = factory.BuildDto(entity);
            var nodeDto = dto.ContentTypeDto.NodeDto;
            var o = Database.Update(nodeDto);

            //Look up ContentType entry to get PrimaryKey for updating the DTO
            var dtoPk = Database.First<ContentTypeDto>("WHERE nodeId = @Id", new { Id = entity.Id });
            var contentTypeDto = dto.ContentTypeDto;
            contentTypeDto.PrimaryKey = dtoPk.PrimaryKey;
            Database.Update(contentTypeDto);

            //Look up DocumentType entries for updating - this could possibly be a "remove all, insert all"-approach

            //Check Dirty properties for Tabs/Groups and PropertyTypes - insert and delete accordingly
            if (((ICanBeDirty)entity).IsPropertyDirty("PropertyGroups") || entity.PropertyGroups.Any(x => x.IsDirty()))
            {
                //Delete PropertyTypes by excepting entries from db with entries from collections
                var dbPropertyTypes = Database.Fetch<PropertyTypeDto>("WHERE contentTypeId = @Id", new { Id = entity.Id }).Select(x => x.Alias);
                var entityPropertyTypes = entity.PropertyTypes.Select(x => x.Alias);
                var aliases = dbPropertyTypes.Except(entityPropertyTypes);
                foreach (var alias in aliases)
                {
                    Database.Delete<PropertyTypeDto>("WHERE contentTypeId = @Id AND Alias = @Alias", new { Id = entity.Id, Alias = alias });
                }
                //Delete Tabs/Groups by excepting entries from db with entries from collections
                var dbPropertyGroups = Database.Fetch<TabDto>("WHERE contenttypeNodeId = @Id", new { Id = entity.Id }).Select(x => x.Text);
                var entityPropertyGroups = entity.PropertyGroups.Select(x => x.Name);
                var tabs = dbPropertyGroups.Except(entityPropertyGroups);
                foreach (var tabName in tabs)
                {
                    Database.Delete<TabDto>("WHERE contenttypeNodeId = @Id AND text = @Name", new { Id = entity.Id, Name = tabName });
                }

                //Run through all groups and types to insert or update entries
                foreach (var propertyGroup in entity.PropertyGroups)
                {
                    var tabDto = propertyFactory.BuildTabDto(propertyGroup);
                    int groupPrimaryKey = propertyGroup.HasIdentity
                                              ? Database.Update(tabDto)
                                              : Convert.ToInt32(Database.Insert(tabDto));
                    if (!propertyGroup.HasIdentity)
                        propertyGroup.Id = groupPrimaryKey;//Set Id on new PropertyGroup

                    //This should indicate that neither group nor property types has been touched, but this implies a deeper 'Dirty'-lookup
                    //if(!propertyGroup.IsDirty()) continue;

                    foreach (var propertyType in propertyGroup.PropertyTypes)
                    {
                        var propertyTypeDto = propertyFactory.BuildPropertyTypeDto(propertyGroup.Id, propertyType);
                        int typePrimaryKey = propertyType.HasIdentity
                                                 ? Database.Update(propertyTypeDto)
                                                 : Convert.ToInt32(Database.Insert(propertyTypeDto));
                        if (!propertyType.HasIdentity)
                            propertyType.Id = typePrimaryKey;//Set Id on new PropertyType
                    }
                }
            }

            ((ContentType)entity).ResetDirtyProperties();
        }

        #endregion
    }
}