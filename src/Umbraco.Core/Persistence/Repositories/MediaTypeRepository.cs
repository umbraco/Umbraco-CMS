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
    /// Represents a repository for doing CRUD operations for <see cref="IMediaType"/>
    /// </summary>
    internal class MediaTypeRepository : PetaPocoRepositoryBase<int, IMediaType>, IMediaTypeRepository
    {
        public MediaTypeRepository(IUnitOfWork work) : base(work)
        {
        }

        public MediaTypeRepository(IUnitOfWork work, IRepositoryCacheProvider cache) : base(work, cache)
        {
        }

        #region Overrides of RepositoryBase<int,IMedia>

        protected override IMediaType PerformGet(int id)
        {
            var contentTypeSql = GetBaseQuery(false);
            contentTypeSql.Append(GetBaseWhereClause(id));

            var dto = Database.Query<ContentTypeDto, NodeDto>(contentTypeSql).FirstOrDefault();

            if (dto == null)
                return null;

            var propertySql = new Sql();
            propertySql.Select("*");
            propertySql.From("cmsTab");
            propertySql.RightJoin("cmsPropertyType ON [cmsTab].[id] = [cmsPropertyType].[tabId]");
            propertySql.InnerJoin("cmsDataType ON [cmsPropertyType].[dataTypeId] = [cmsDataType].[nodeId]");
            propertySql.Where("[cmsPropertyType].[contentTypeId] = @Id", new { Id = id });

            var tabDtos = Database.Fetch<TabDto, PropertyTypeDto, DataTypeDto, TabDto>(new TabPropertyTypeRelator().Map, propertySql);

            var factory = new MediaTypeFactory(NodeObjectTypeId);
            var contentType = factory.BuildEntity(dto);

            var propertyFactory = new PropertyGroupFactory(id);
            var propertyGroups = propertyFactory.BuildEntity(tabDtos);
            contentType.PropertyGroups = new PropertyGroupCollection(propertyGroups);

            ((MediaType)contentType).ResetDirtyProperties();
            return contentType;
        }

        protected override IEnumerable<IMediaType> PerformGetAll(params int[] ids)
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

        protected override IEnumerable<IMediaType> PerformGetByQuery(IQuery<IMediaType> query)
        {
            var sqlClause = GetBaseQuery(false);
            var translator = new SqlTranslator<IMediaType>(sqlClause, query);
            var sql = translator.Translate();

            var documentTypeDtos = Database.Fetch<ContentTypeDto, NodeDto>(sql);

            foreach (var dto in documentTypeDtos)
            {
                yield return Get(dto.NodeId);
            }
        }

        #endregion

        #region Overrides of PetaPocoRepositoryBase<int,IMedia>

        protected override Sql GetBaseQuery(bool isCount)
        {
            var sql = new Sql();
            sql.Select(isCount ? "COUNT(*)" : "*");
            sql.From("cmsContentType");
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
                               string.Format("DELETE FROM cmsContentType WHERE NodeId = @Id"),
                               string.Format("DELETE FROM umbracoNode WHERE id = @Id")
                           };
            return list;
        }

        protected override Guid NodeObjectTypeId
        {
            get { return new Guid("4EA4382B-2F5A-4C2B-9587-AE9B3CF3602E"); }
        }

        #endregion

        #region Unit of Work Implementation

        protected override void PersistNewItem(IMediaType entity)
        {
            ((MediaType)entity).AddingEntity();

            var factory = new MediaTypeFactory(NodeObjectTypeId);
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

            //Insert new ContentType entry
            Database.Insert(dto);
        }

        protected override void PersistUpdatedItem(IMediaType entity)
        {
            //Updates Modified date
            ((MediaType)entity).UpdatingEntity();

            var propertyFactory = new PropertyGroupFactory(entity.Id);
            var factory = new MediaTypeFactory(NodeObjectTypeId);
            var dto = factory.BuildDto(entity);
            var nodeDto = dto.NodeDto;
            var o = Database.Update(nodeDto);

            //Look up ContentType entry to get PrimaryKey for updating the DTO
            var dtoPk = Database.First<ContentTypeDto>("WHERE nodeId = @Id", new { Id = entity.Id });
            dto.PrimaryKey = dtoPk.PrimaryKey;
            Database.Update(dto);

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

            ((MediaType)entity).ResetDirtyProperties();
        }

        #endregion
    }
}