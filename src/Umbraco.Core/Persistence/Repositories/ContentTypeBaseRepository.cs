using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence.Caching;
using Umbraco.Core.Persistence.Factories;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.Relators;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Persistence.Repositories
{
    /// <summary>
    /// Represent an abstract Repository for ContentType based repositories
    /// </summary>
    /// <remarks>Exposes shared functionality</remarks>
    /// <typeparam name="TId"></typeparam>
    /// <typeparam name="TEntity"></typeparam>
    internal abstract class ContentTypeBaseRepository<TId, TEntity> : PetaPocoRepositoryBase<TId, TEntity>
        where TEntity : class, IContentTypeComposition
    {
		protected ContentTypeBaseRepository(IDatabaseUnitOfWork work)
			: base(work)
        {
        }

		protected ContentTypeBaseRepository(IDatabaseUnitOfWork work, IRepositoryCacheProvider cache)
			: base(work, cache)
        {
        }

        protected IEnumerable<int> PerformGetByQuery(IQuery<PropertyType> query)
        {
            var sqlClause = new Sql();
            sqlClause.Select("*")
               .From<PropertyTypeGroupDto>()
               .RightJoin<PropertyTypeDto>()
               .On<PropertyTypeGroupDto, PropertyTypeDto>(left => left.Id, right => right.PropertyTypeGroupId)
               .InnerJoin<DataTypeDto>()
               .On<PropertyTypeDto, DataTypeDto>(left => left.DataTypeId, right => right.DataTypeId);

            var translator = new SqlTranslator<PropertyType>(sqlClause, query);
            var sql = translator.Translate()
                                .OrderBy<PropertyTypeDto>(x => x.PropertyTypeGroupId);

            var dtos = Database.Fetch<PropertyTypeGroupDto, PropertyTypeDto, DataTypeDto, PropertyTypeGroupDto>(new GroupPropertyTypeRelator().Map, sql);

            foreach (var dto in dtos.DistinctBy(x => x.ContentTypeNodeId))
            {
                yield return dto.ContentTypeNodeId;
            }
        }
        
        protected virtual PropertyType CreatePropertyType(string propertyEditorAlias, DataTypeDatabaseType dbType, string propertyTypeAlias)
        {
            return new PropertyType(propertyEditorAlias, dbType);
        }

        protected void PersistNewBaseContentType(ContentTypeDto dto, IContentTypeComposition entity)
        {
            //Cannot add a duplicate content type type
            var exists = Database.ExecuteScalar<int>(@"SELECT COUNT(*) FROM cmsContentType
INNER JOIN umbracoNode ON cmsContentType.nodeId = umbracoNode.id
WHERE cmsContentType." + SqlSyntaxContext.SqlSyntaxProvider.GetQuotedColumnName("alias") + @"= @alias
AND umbracoNode.nodeObjectType = @objectType",
                new { alias = entity.Alias, objectType = NodeObjectTypeId });
            if (exists > 0)
            {
                throw new DuplicateNameException("An item with the alias " + entity.Alias + " already exists");
            }

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
            dto.NodeId = nodeDto.NodeId;
            Database.Insert(dto);

            //Insert ContentType composition in new table
            foreach (var composition in entity.ContentTypeComposition)
            {
                if (composition.Id == entity.Id) continue;//Just to ensure that we aren't creating a reference to ourself.

                if (composition.HasIdentity)
                {
                    Database.Insert(new ContentType2ContentTypeDto { ParentId = composition.Id, ChildId = entity.Id });
                }
                else
                {
                    //Fallback for ContentTypes with no identity
                    var contentTypeDto = Database.FirstOrDefault<ContentTypeDto>("WHERE alias = @Alias", new {Alias = composition.Alias});
                    if (contentTypeDto != null)
                    {
                        Database.Insert(new ContentType2ContentTypeDto { ParentId = contentTypeDto.NodeId, ChildId = entity.Id });
                    }
                }
            }

            //Insert collection of allowed content types
            foreach (var allowedContentType in entity.AllowedContentTypes)
            {
                Database.Insert(new ContentTypeAllowedContentTypeDto
                                    {
                                        Id = entity.Id,
                                        AllowedId = allowedContentType.Id.Value,
                                        SortOrder = allowedContentType.SortOrder
                                    });
            }

            var propertyFactory = new PropertyGroupFactory(nodeDto.NodeId);

            //Insert Tabs
            foreach (var propertyGroup in entity.PropertyGroups)
            {
                var tabDto = propertyFactory.BuildGroupDto(propertyGroup);
                var primaryKey = Convert.ToInt32(Database.Insert(tabDto));
                propertyGroup.Id = primaryKey;//Set Id on PropertyGroup

                //Ensure that the PropertyGroup's Id is set on the PropertyTypes within a group
                //unless the PropertyGroupId has already been changed.
                foreach (var propertyType in propertyGroup.PropertyTypes)
                {
                    if (propertyType.IsPropertyDirty("PropertyGroupId") == false)
                    {
                        var tempGroup = propertyGroup;
                        propertyType.PropertyGroupId = new Lazy<int>(() => tempGroup.Id);
                    }
                }
            }

            //Insert PropertyTypes
            foreach (var propertyType in entity.PropertyTypes)
            {
                var tabId = propertyType.PropertyGroupId != null ? propertyType.PropertyGroupId.Value : default(int);
                //If the Id of the DataType is not set, we resolve it from the db by its PropertyEditorAlias
                if (propertyType.DataTypeDefinitionId == 0 || propertyType.DataTypeDefinitionId == default(int))
                {
                    AssignDataTypeFromPropertyEditor(propertyType);
                }
                var propertyTypeDto = propertyFactory.BuildPropertyTypeDto(tabId, propertyType);
                int typePrimaryKey = Convert.ToInt32(Database.Insert(propertyTypeDto));
                propertyType.Id = typePrimaryKey; //Set Id on new PropertyType

                //Update the current PropertyType with correct PropertyEditorAlias and DatabaseType
                var dataTypeDto = Database.FirstOrDefault<DataTypeDto>("WHERE nodeId = @Id", new { Id = propertyTypeDto.DataTypeId });
                propertyType.PropertyEditorAlias = dataTypeDto.PropertyEditorAlias;
                propertyType.DataTypeDatabaseType = dataTypeDto.DbType.EnumParse<DataTypeDatabaseType>(true);
            }
        }

        protected void PersistUpdatedBaseContentType(ContentTypeDto dto, IContentTypeComposition entity)
        {

            //Cannot update to a duplicate alias
            var exists = Database.ExecuteScalar<int>(@"SELECT COUNT(*) FROM cmsContentType
INNER JOIN umbracoNode ON cmsContentType.nodeId = umbracoNode.id
WHERE cmsContentType." + SqlSyntaxContext.SqlSyntaxProvider.GetQuotedColumnName("alias") + @"= @alias
AND umbracoNode.nodeObjectType = @objectType
AND umbracoNode.id <> @id",
                new { id = dto.NodeId, alias = entity.Alias, objectType = NodeObjectTypeId });
            if (exists > 0)
            {
                throw new DuplicateNameException("An item with the alias " + entity.Alias + " already exists");
            }

            var propertyGroupFactory = new PropertyGroupFactory(entity.Id);

            var nodeDto = dto.NodeDto;
            var o = Database.Update(nodeDto);

            //Look up ContentType entry to get PrimaryKey for updating the DTO
            var dtoPk = Database.First<ContentTypeDto>("WHERE nodeId = @Id", new {Id = entity.Id});
            dto.PrimaryKey = dtoPk.PrimaryKey;
            Database.Update(dto);

            //Delete the ContentType composition entries before adding the updated collection
            Database.Delete<ContentType2ContentTypeDto>("WHERE childContentTypeId = @Id", new {Id = entity.Id});
            //Update ContentType composition in new table
            foreach (var composition in entity.ContentTypeComposition)
            {
                Database.Insert(new ContentType2ContentTypeDto {ParentId = composition.Id, ChildId = entity.Id});
            }

            //Removing a ContentType from a composition (U4-1690)
            //1. Find content based on the current ContentType: entity.Id
            //2. Find all PropertyTypes on the ContentType that was removed - tracked id (key)
            //3. Remove properties based on property types from the removed content type where the content ids correspond to those found in step one
            var compositionBase = entity as ContentTypeCompositionBase;
            if (compositionBase != null && compositionBase.RemovedContentTypeKeyTracker != null &&
                compositionBase.RemovedContentTypeKeyTracker.Any())
            {
                //Find Content based on the current ContentType
                var sql = new Sql();
                sql.Select("*")
                   .From<ContentDto>()
                   .InnerJoin<NodeDto>()
                   .On<ContentDto, NodeDto>(left => left.NodeId, right => right.NodeId)
                   .Where<NodeDto>(x => x.NodeObjectType == new Guid(Constants.ObjectTypes.Document))
                   .Where<ContentDto>(x => x.ContentTypeId == entity.Id);

                var contentDtos = Database.Fetch<ContentDto, NodeDto>(sql);
                //Loop through all tracked keys, which corresponds to the ContentTypes that has been removed from the composition
                foreach (var key in compositionBase.RemovedContentTypeKeyTracker)
                {
                    //Find PropertyTypes for the removed ContentType
                    var propertyTypes = Database.Fetch<PropertyTypeDto>("WHERE contentTypeId = @Id", new {Id = key});
                    //Loop through the Content that is based on the current ContentType in order to remove the Properties that are 
                    //based on the PropertyTypes that belong to the removed ContentType.
                    foreach (var contentDto in contentDtos)
                    {
                        foreach (var propertyType in propertyTypes)
                        {
                            var nodeId = contentDto.NodeId;
                            var propertyTypeId = propertyType.Id;
                            var propertySql = new Sql().Select("*")
                                                       .From<PropertyDataDto>()
                                                       .InnerJoin<PropertyTypeDto>()
                                                       .On<PropertyDataDto, PropertyTypeDto>(
                                                           left => left.PropertyTypeId, right => right.Id)
                                                       .Where<PropertyDataDto>(x => x.NodeId == nodeId)
                                                       .Where<PropertyTypeDto>(x => x.Id == propertyTypeId);

                            //Finally delete the properties that match our criteria for removing a ContentType from the composition
                            Database.Delete<PropertyDataDto>(propertySql);
                        }
                    }
                }
            }

            //Delete the allowed content type entries before adding the updated collection
            Database.Delete<ContentTypeAllowedContentTypeDto>("WHERE Id = @Id", new {Id = entity.Id});
            //Insert collection of allowed content types
            foreach (var allowedContentType in entity.AllowedContentTypes)
            {
                Database.Insert(new ContentTypeAllowedContentTypeDto
                                    {
                                        Id = entity.Id,
                                        AllowedId = allowedContentType.Id.Value,
                                        SortOrder = allowedContentType.SortOrder
                                    });
            }

            if (((ICanBeDirty) entity).IsPropertyDirty("PropertyTypes") || entity.PropertyTypes.Any(x => x.IsDirty()))
            {
                //Delete PropertyTypes by excepting entries from db with entries from collections
                var dbPropertyTypes = Database.Fetch<PropertyTypeDto>("WHERE contentTypeId = @Id", new {Id = entity.Id});
                var dbPropertyTypeAlias = dbPropertyTypes.Select(x => x.Id);
                var entityPropertyTypes = entity.PropertyTypes.Where(x => x.HasIdentity).Select(x => x.Id);
                var items = dbPropertyTypeAlias.Except(entityPropertyTypes);
                foreach (var item in items)
                {
                    //Before a PropertyType can be deleted, all Properties based on that PropertyType should be deleted.
                    Database.Delete<TagRelationshipDto>("WHERE propertyTypeId = @Id", new { Id = item });
                    Database.Delete<PropertyDataDto>("WHERE propertytypeid = @Id", new {Id = item});
                    Database.Delete<PropertyTypeDto>("WHERE contentTypeId = @Id AND id = @PropertyTypeId",
                                                     new {Id = entity.Id, PropertyTypeId = item});
                }
            }

            if (((ICanBeDirty) entity).IsPropertyDirty("PropertyGroups") || entity.PropertyGroups.Any(x => x.IsDirty()))
            {
                //Delete Tabs/Groups by excepting entries from db with entries from collections
                var dbPropertyGroups =
                    Database.Fetch<PropertyTypeGroupDto>("WHERE contenttypeNodeId = @Id", new {Id = entity.Id})
                            .Select(x => new Tuple<int, string>(x.Id, x.Text));
                var entityPropertyGroups = entity.PropertyGroups.Select(x => new Tuple<int, string>(x.Id, x.Name));
                var tabs = dbPropertyGroups.Except(entityPropertyGroups);
                foreach (var tab in tabs)
                {
                    Database.Update<PropertyTypeDto>("SET propertyTypeGroupId = NULL WHERE propertyTypeGroupId = @PropertyGroupId",
                                                    new {PropertyGroupId = tab.Item1});
                    Database.Update<PropertyTypeGroupDto>("SET parentGroupId = NULL WHERE parentGroupId = @TabId",
                                                          new {TabId = tab.Item1});
                    Database.Delete<PropertyTypeGroupDto>("WHERE contenttypeNodeId = @Id AND text = @Name",
                                                          new {Id = entity.Id, Name = tab.Item2});
                }
            }

            //Run through all groups to insert or update entries
            foreach (var propertyGroup in entity.PropertyGroups)
            {
                var tabDto = propertyGroupFactory.BuildGroupDto(propertyGroup);
                int groupPrimaryKey = propertyGroup.HasIdentity
                                          ? Database.Update(tabDto)
                                          : Convert.ToInt32(Database.Insert(tabDto));
                if (propertyGroup.HasIdentity == false)
                    propertyGroup.Id = groupPrimaryKey; //Set Id on new PropertyGroup

                //Ensure that the PropertyGroup's Id is set on the PropertyTypes within a group
                //unless the PropertyGroupId has already been changed.
                foreach (var propertyType in propertyGroup.PropertyTypes)
                {
                    if (propertyType.IsPropertyDirty("PropertyGroupId") == false)
                    {
                        var tempGroup = propertyGroup;
                        propertyType.PropertyGroupId = new Lazy<int>(() => tempGroup.Id);
                    }
                }
            }

            //Run through all PropertyTypes to insert or update entries
            foreach (var propertyType in entity.PropertyTypes)
            {
                var tabId = propertyType.PropertyGroupId != null ? propertyType.PropertyGroupId.Value : default(int);
                //If the Id of the DataType is not set, we resolve it from the db by its PropertyEditorAlias
                if (propertyType.DataTypeDefinitionId == 0 || propertyType.DataTypeDefinitionId == default(int))
                {
                    AssignDataTypeFromPropertyEditor(propertyType);
                }

                //validate the alias! 
                ValidateAlias(propertyType);

                var propertyTypeDto = propertyGroupFactory.BuildPropertyTypeDto(tabId, propertyType);
                int typePrimaryKey = propertyType.HasIdentity
                                         ? Database.Update(propertyTypeDto)
                                         : Convert.ToInt32(Database.Insert(propertyTypeDto));
                if (propertyType.HasIdentity == false)
                    propertyType.Id = typePrimaryKey; //Set Id on new PropertyType
            }
        }

        protected IEnumerable<ContentTypeSort> GetAllowedContentTypeIds(int id)
        {
            var sql = new Sql();
            sql.Select("*")
               .From<ContentTypeAllowedContentTypeDto>()
               .LeftJoin<ContentTypeDto>()
               .On<ContentTypeAllowedContentTypeDto, ContentTypeDto>(left => left.AllowedId, right => right.NodeId)
               .Where<ContentTypeAllowedContentTypeDto>(x => x.Id == id);

            var allowedContentTypeDtos = Database.Fetch<ContentTypeAllowedContentTypeDto, ContentTypeDto>(sql);
            return allowedContentTypeDtos.Select(x => new ContentTypeSort { Id = new Lazy<int>(() => x.AllowedId), Alias = x.ContentTypeDto.Alias, SortOrder = x.SortOrder }).ToList();
        }

        protected PropertyGroupCollection GetPropertyGroupCollection(int id, DateTime createDate, DateTime updateDate)
        {
            var sql = new Sql();
            sql.Select("*")
               .From<PropertyTypeGroupDto>()
               .LeftJoin<PropertyTypeDto>()
               .On<PropertyTypeGroupDto, PropertyTypeDto>(left => left.Id, right => right.PropertyTypeGroupId)
               .LeftJoin<DataTypeDto>()
               .On<PropertyTypeDto, DataTypeDto>(left => left.DataTypeId, right => right.DataTypeId)
               .Where<PropertyTypeGroupDto>(x => x.ContentTypeNodeId == id)
               .OrderBy<PropertyTypeGroupDto>(x => x.Id);

            var dtos = Database.Fetch<PropertyTypeGroupDto, PropertyTypeDto, DataTypeDto, PropertyTypeGroupDto>(new GroupPropertyTypeRelator().Map, sql);

            var propertyGroupFactory = new PropertyGroupFactory(id, createDate, updateDate, CreatePropertyType);
            var propertyGroups = propertyGroupFactory.BuildEntity(dtos);
            return new PropertyGroupCollection(propertyGroups);
        }

        protected PropertyTypeCollection GetPropertyTypeCollection(int id, DateTime createDate, DateTime updateDate)
        {
            var sql = new Sql();
            sql.Select("*")
               .From<PropertyTypeDto>()
               .InnerJoin<DataTypeDto>()
               .On<PropertyTypeDto, DataTypeDto>(left => left.DataTypeId, right => right.DataTypeId)
               .Where<PropertyTypeDto>(x => x.ContentTypeId == id);

            var dtos = Database.Fetch<PropertyTypeDto, DataTypeDto>(sql);

            //TODO Move this to a PropertyTypeFactory
            var list = new List<PropertyType>();
            foreach (var dto in dtos.Where(x => (x.PropertyTypeGroupId > 0) == false))
            {
                var propType = CreatePropertyType(dto.DataTypeDto.PropertyEditorAlias, dto.DataTypeDto.DbType.EnumParse<DataTypeDatabaseType>(true), dto.Alias);
                propType.Alias = dto.Alias;
                propType.DataTypeDefinitionId = dto.DataTypeId;
                propType.Description = dto.Description;
                propType.Id = dto.Id;
                propType.Name = dto.Name;
                propType.HelpText = dto.HelpText;
                propType.Mandatory = dto.Mandatory;
                propType.SortOrder = dto.SortOrder;
                propType.ValidationRegExp = dto.ValidationRegExp;
                propType.CreateDate = createDate;
                propType.UpdateDate = updateDate;
                list.Add(propType);
            }
            //Reset dirty properties
            Parallel.ForEach(list, currentFile => currentFile.ResetDirtyProperties(false));

            return new PropertyTypeCollection(list);
        }

        protected void ValidateAlias(PropertyType pt)
        {
            Mandate.That<InvalidOperationException>(string.IsNullOrEmpty(pt.Alias) == false,
                                    () =>
                                    {
                                        var message =
                                            string.Format(
                                                "{0} '{1}' cannot have an empty Alias. This is most likely due to invalid characters stripped from the Alias.",
                                                "Property Type",
                                                pt.Name);
                                        var exception = new InvalidOperationException(message);

                                        LogHelper.Error<ContentTypeBaseRepository<TId, TEntity>>(message, exception);
                                        throw exception;
                                    });
        }

        protected void ValidateAlias(TEntity entity)
        {
            Mandate.That<InvalidOperationException>(string.IsNullOrEmpty(entity.Alias) == false,
                                    () =>
                                    {
                                        var message =
                                            string.Format(
                                                "{0} '{1}' cannot have an empty Alias. This is most likely due to invalid characters stripped from the Alias.",
                                                typeof(TEntity).Name,
                                                entity.Name);
                                        var exception = new InvalidOperationException(message);

                                        LogHelper.Error<ContentTypeBaseRepository<TId, TEntity>>(message, exception);
                                        throw exception;
                                    });
        }

        /// <summary>
        /// Try to set the data type id based on its ControlId
        /// </summary>
        /// <param name="propertyType"></param>
        private void AssignDataTypeFromPropertyEditor(PropertyType propertyType)
        {
            //we cannot try to assign a data type of it's empty
            if (propertyType.PropertyEditorAlias.IsNullOrWhiteSpace() == false)
            {
                var sql = new Sql()
                    .Select("*")
                    .From<DataTypeDto>()
                    .Where("propertyEditorAlias = @propertyEditorAlias", new { propertyEditorAlias = propertyType.PropertyEditorAlias })
                    .OrderBy<DataTypeDto>(typeDto => typeDto.DataTypeId);
                var datatype = Database.FirstOrDefault<DataTypeDto>(sql);
                //we cannot assign a data type if one was not found
                if (datatype != null)
                {
                    propertyType.DataTypeDefinitionId = datatype.DataTypeId;
                }
                else
                {
                    LogHelper.Warn<ContentTypeBaseRepository<TId, TEntity>>("Could not assign a data type for the property type " + propertyType.Alias + " since no data type was found with a property editor " + propertyType.PropertyEditorAlias);
                }
            }
        }
    }
}