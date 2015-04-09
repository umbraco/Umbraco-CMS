using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Models.Rdbms;

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
    /// <typeparam name="TEntity"></typeparam>
    internal abstract class ContentTypeBaseRepository<TEntity> : PetaPocoRepositoryBase<int, TEntity>
        where TEntity : class, IContentTypeComposition
    {

        protected ContentTypeBaseRepository(IDatabaseUnitOfWork work, CacheHelper cache, ILogger logger, ISqlSyntaxProvider sqlSyntax)
            : base(work, cache, logger, sqlSyntax)
        {
        }

        /// <summary>
        /// Returns the content type ids that match the query
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
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
            return new PropertyType(propertyEditorAlias, dbType, propertyTypeAlias);
        }

        protected void PersistNewBaseContentType(ContentTypeDto dto, IContentTypeComposition entity)
        {
            //Cannot add a duplicate content type type
            var exists = Database.ExecuteScalar<int>(@"SELECT COUNT(*) FROM cmsContentType
INNER JOIN umbracoNode ON cmsContentType.nodeId = umbracoNode.id
WHERE cmsContentType." + SqlSyntax.GetQuotedColumnName("alias") + @"= @alias
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
WHERE cmsContentType." + SqlSyntax.GetQuotedColumnName("alias") + @"= @alias
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
                            var propertySql = new Sql().Select("cmsPropertyData.id")
                                                       .From<PropertyDataDto>()
                                                       .InnerJoin<PropertyTypeDto>()
                                                       .On<PropertyDataDto, PropertyTypeDto>(
                                                           left => left.PropertyTypeId, right => right.Id)
                                                       .Where<PropertyDataDto>(x => x.NodeId == nodeId)
                                                       .Where<PropertyTypeDto>(x => x.Id == propertyTypeId);

                            //Finally delete the properties that match our criteria for removing a ContentType from the composition
                            Database.Delete<PropertyDataDto>(new Sql("WHERE id IN (" + propertySql.SQL + ")", propertySql.Arguments));
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

            if (entity.IsPropertyDirty("PropertyGroups") || 
                entity.PropertyGroups.Any(x => x.IsDirty()))
            {
                //Delete Tabs/Groups by excepting entries from db with entries from collections
                var dbPropertyGroups =
                    Database.Fetch<PropertyTypeGroupDto>("WHERE contenttypeNodeId = @Id", new {Id = entity.Id})
                            .Select(x => new Tuple<int, string>(x.Id, x.Text))
                            .ToList();
                var entityPropertyGroups = entity.PropertyGroups.Select(x => new Tuple<int, string>(x.Id, x.Name)).ToList();
                var tabsToDelete = dbPropertyGroups.Select(x => x.Item1).Except(entityPropertyGroups.Select(x => x.Item1));
                var tabs = dbPropertyGroups.Where(x => tabsToDelete.Any(y => y == x.Item1));
                //Update Tab name downstream to ensure renaming is done properly
                foreach (var propertyGroup in entityPropertyGroups)
                {
                    Database.Update<PropertyTypeGroupDto>("SET Text = @TabName WHERE parentGroupId = @TabId",
                                                          new { TabName = propertyGroup.Item2, TabId = propertyGroup.Item1 });

                    var childGroups = Database.Fetch<PropertyTypeGroupDto>("WHERE parentGroupId = @TabId", new { TabId = propertyGroup.Item1 });
                    foreach (var childGroup in childGroups)
                    {
                        var sibling = Database.Fetch<PropertyTypeGroupDto>("WHERE contenttypeNodeId = @Id AND text = @Name",
                            new { Id = childGroup.ContentTypeNodeId, Name = propertyGroup.Item2 })
                            .FirstOrDefault(x => x.ParentGroupId.HasValue == false || x.ParentGroupId.Value.Equals(propertyGroup.Item1) == false);
                        //If the child group doesn't have a sibling there is no chance of duplicates and we continue
                        if (sibling == null || (sibling.ParentGroupId.HasValue && sibling.ParentGroupId.Value.Equals(propertyGroup.Item1))) continue;

                        //Since the child group has a sibling with the same name we need to point any PropertyTypes to the sibling
                        //as this child group is about to leave the party.
                        Database.Update<PropertyTypeDto>(
                            "SET propertyTypeGroupId = @PropertyTypeGroupId WHERE propertyTypeGroupId = @PropertyGroupId AND ContentTypeId = @ContentTypeId",
                            new { PropertyTypeGroupId = sibling.Id, PropertyGroupId = childGroup.Id, ContentTypeId = childGroup.ContentTypeNodeId });

                        //Since the parent group has been renamed and we have duplicates we remove this group
                        //and leave our sibling in charge of the part.
                        Database.Delete(childGroup);
                    }
                }
                //Do Tab updates
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

            //If a Composition is removed we need to update/reset references to the PropertyGroups on that ContentType
            if (entity.IsPropertyDirty("ContentTypeComposition") &&
                compositionBase != null &&
                compositionBase.RemovedContentTypeKeyTracker != null &&
                compositionBase.RemovedContentTypeKeyTracker.Any())
            {
                foreach (var compositionId in compositionBase.RemovedContentTypeKeyTracker)
                {
                    var dbPropertyGroups =
                        Database.Fetch<PropertyTypeGroupDto>("WHERE contenttypeNodeId = @Id", new { Id = compositionId })
                            .Select(x => x.Id);
                    foreach (var propertyGroup in dbPropertyGroups)
                    {
                        Database.Update<PropertyTypeGroupDto>("SET parentGroupId = NULL WHERE parentGroupId = @TabId AND contenttypeNodeId = @ContentTypeNodeId",
                                                              new { TabId = propertyGroup, ContentTypeNodeId = entity.Id });
                    }
                }
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
            return allowedContentTypeDtos.Select(x => new ContentTypeSort(new Lazy<int>(() => x.AllowedId), x.SortOrder, x.ContentTypeDto.Alias)).ToList();
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
                propType.DataTypeDefinitionId = dto.DataTypeId;
                propType.Description = dto.Description;
                propType.Id = dto.Id;
                propType.Name = dto.Name;
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

                                        Logger.Error<ContentTypeBaseRepository<TEntity>>(message, exception);
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

                                        Logger.Error<ContentTypeBaseRepository<TEntity>>(message, exception);
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
                    Logger.Warn<ContentTypeBaseRepository<TEntity>>("Could not assign a data type for the property type " + propertyType.Alias + " since no data type was found with a property editor " + propertyType.PropertyEditorAlias);
                }
            }
        }

        internal static class ContentTypeQueryMapper
        {

            public class AssociatedTemplate
            {
                public AssociatedTemplate(int templateId, string @alias, string templateName)
                {
                    TemplateId = templateId;
                    Alias = alias;
                    TemplateName = templateName;
                }

                public int TemplateId { get; set; }
                public string Alias { get; set; }
                public string TemplateName { get; set; }

                protected bool Equals(AssociatedTemplate other)
                {
                    return TemplateId == other.TemplateId;
                }

                public override bool Equals(object obj)
                {
                    if (ReferenceEquals(null, obj)) return false;
                    if (ReferenceEquals(this, obj)) return true;
                    if (obj.GetType() != this.GetType()) return false;
                    return Equals((AssociatedTemplate)obj);
                }

                public override int GetHashCode()
                {
                    return TemplateId;
                }
            }

            public static IEnumerable<IMediaType> GetMediaTypes<TRepo>(
                int[] mediaTypeIds, Database db, ISqlSyntaxProvider sqlSyntax,
                TRepo contentTypeRepository)
                where TRepo : IRepositoryQueryable<int, TEntity>
            {
                IDictionary<int, IEnumerable<int>> allParentMediaTypeIds;
                var mediaTypes = MapMediaTypes(mediaTypeIds, db, sqlSyntax, out allParentMediaTypeIds)
                    .ToArray();

                MapContentTypeChildren(mediaTypes, db, sqlSyntax, contentTypeRepository, allParentMediaTypeIds);
                
                return mediaTypes;
            }

            public static IEnumerable<IContentType> GetContentTypes<TRepo>(
                int[] contentTypeIds, Database db, ISqlSyntaxProvider sqlSyntax,
                TRepo contentTypeRepository,
                ITemplateRepository templateRepository)
                where TRepo : IRepositoryQueryable<int, TEntity>
            {
                IDictionary<int, IEnumerable<AssociatedTemplate>> allAssociatedTemplates;
                IDictionary<int, IEnumerable<int>> allParentContentTypeIds;
                var contentTypes = MapContentTypes(contentTypeIds, db, sqlSyntax, out allAssociatedTemplates, out allParentContentTypeIds)
                    .ToArray();

                if (contentTypes.Any())
                {
                    MapContentTypeTemplates(
                            contentTypes, db, contentTypeRepository, templateRepository, allAssociatedTemplates);

                    MapContentTypeChildren(
                            contentTypes, db, sqlSyntax, contentTypeRepository, allParentContentTypeIds);         
                }

                return contentTypes;
            }

            internal static void MapContentTypeChildren<TRepo>(IContentTypeComposition[] contentTypes,
                Database db, ISqlSyntaxProvider sqlSyntax,
                TRepo contentTypeRepository,
                IDictionary<int, IEnumerable<int>> allParentContentTypeIds)
                where TRepo : IRepositoryQueryable<int, TEntity>
            {
                //NOTE: SQL call #2

                var ids = contentTypes.Select(x => x.Id).ToArray();
                IDictionary<int, PropertyGroupCollection> allPropGroups;
                IDictionary<int, PropertyTypeCollection> allPropTypes;
                MapGroupsAndProperties(ids, db, sqlSyntax, out allPropTypes, out allPropGroups);

                foreach (var contentType in contentTypes)
                {
                    contentType.PropertyGroups = allPropGroups[contentType.Id];
                    ((ContentTypeBase) contentType).PropertyTypes = allPropTypes[contentType.Id];
                }
                
                //NOTE: SQL call #3++

                if (allParentContentTypeIds != null)
                {
                    var allParentIdsAsArray = allParentContentTypeIds.SelectMany(x => x.Value).Distinct().ToArray();
                    if (allParentIdsAsArray.Any())
                    {
                        var allParentContentTypes = contentTypeRepository.GetAll(allParentIdsAsArray).ToArray();
                        foreach (var contentType in contentTypes)
                        {
                            var parentContentTypes = allParentContentTypes.Where(x => allParentContentTypeIds[contentType.Id].Contains(x.Id));
                            foreach (var parentContentType in parentContentTypes)
                            {
                                var result = contentType.AddContentType(parentContentType);
                                //Do something if adding fails? (Should hopefully not be possible unless someone created a circular reference)    
                            }

                            //on initial construction we don't want to have dirty properties tracked
                            // http://issues.umbraco.org/issue/U4-1946
                            ((Entity)contentType).ResetDirtyProperties(false);
                        }
                    }
                }

                
            }

            internal static void MapContentTypeTemplates<TRepo>(IContentType[] contentTypes,
                Database db,
                TRepo contentTypeRepository,
                ITemplateRepository templateRepository,
                IDictionary<int, IEnumerable<AssociatedTemplate>> associatedTemplates)
                where TRepo : IRepositoryQueryable<int, TEntity>
            {
                if (associatedTemplates == null || associatedTemplates.Any() == false) return;

                //NOTE: SQL call #3++
                //SEE: http://issues.umbraco.org/issue/U4-5174 to fix this

                var templateIds = associatedTemplates.SelectMany(x => x.Value).Select(x => x.TemplateId)
                    .Distinct()
                    .ToArray();

                var templates = (templateIds.Any()
                    ? templateRepository.GetAll(templateIds)
                    : Enumerable.Empty<ITemplate>()).ToArray();

                foreach (var contentType in contentTypes)
                {
                    var associatedTemplateIds = associatedTemplates[contentType.Id].Select(x => x.TemplateId)
                        .Distinct()
                        .ToArray();

                    contentType.AllowedTemplates = (associatedTemplateIds.Any()
                        ? templates.Where(x => associatedTemplateIds.Contains(x.Id))
                        : Enumerable.Empty<ITemplate>()).ToArray();
                }

                
            }

            internal static IEnumerable<IMediaType> MapMediaTypes(int[] mediaTypeIds, Database db, ISqlSyntaxProvider sqlSyntax,
                out IDictionary<int, IEnumerable<int>> parentMediaTypeIds)
            {
                Mandate.That(mediaTypeIds.Any(), () => new InvalidOperationException("must be at least one content type id specified"));
                Mandate.ParameterNotNull(db, "db");

                //ensure they are unique
                mediaTypeIds = mediaTypeIds.Distinct().ToArray();

                var sql = @"SELECT cmsContentType.pk as ctPk, cmsContentType.alias as ctAlias, cmsContentType.allowAtRoot as ctAllowAtRoot, cmsContentType.description as ctDesc,
                                cmsContentType.icon as ctIcon, cmsContentType.isContainer as ctIsContainer, cmsContentType.nodeId as ctId, cmsContentType.thumbnail as ctThumb,
                                AllowedTypes.allowedId as ctaAllowedId, AllowedTypes.SortOrder as ctaSortOrder, AllowedTypes.alias as ctaAlias,		                        
                                ParentTypes.parentContentTypeId as chtParentId,
                                umbracoNode.createDate as nCreateDate, umbracoNode." + sqlSyntax.GetQuotedColumnName("level") + @" as nLevel, umbracoNode.nodeObjectType as nObjectType, umbracoNode.nodeUser as nUser,
		                        umbracoNode.parentID as nParentId, umbracoNode." + sqlSyntax.GetQuotedColumnName("path") + @" as nPath, umbracoNode.sortOrder as nSortOrder, umbracoNode." + sqlSyntax.GetQuotedColumnName("text") + @" as nName, umbracoNode.trashed as nTrashed,
                                umbracoNode.uniqueID as nUniqueId
                        FROM cmsContentType
                        INNER JOIN umbracoNode
                        ON cmsContentType.nodeId = umbracoNode.id
                        LEFT JOIN (
                            SELECT cmsContentTypeAllowedContentType.Id, cmsContentTypeAllowedContentType.AllowedId, cmsContentType.alias, cmsContentTypeAllowedContentType.SortOrder 
                            FROM cmsContentTypeAllowedContentType	
                            INNER JOIN cmsContentType
                            ON cmsContentTypeAllowedContentType.AllowedId = cmsContentType.nodeId
                        ) AllowedTypes
                        ON AllowedTypes.Id = cmsContentType.nodeId
                        LEFT JOIN cmsContentType2ContentType as ParentTypes
                        ON ParentTypes.childContentTypeId = cmsContentType.nodeId	
                        WHERE (umbracoNode.nodeObjectType = @nodeObjectType)
                        AND (umbracoNode.id IN (@contentTypeIds))";

                //NOTE: we are going to assume there's not going to be more than 2100 content type ids since that is the max SQL param count!
                if ((mediaTypeIds.Length - 1) > 2000)
                    throw new InvalidOperationException("Cannot perform this lookup, too many sql parameters");

                var result = db.Fetch<dynamic>(sql, new { nodeObjectType = new Guid(Constants.ObjectTypes.MediaType), contentTypeIds = mediaTypeIds });

                if (result.Any() == false)
                {
                    parentMediaTypeIds = null;
                    return Enumerable.Empty<IMediaType>();
                }

                parentMediaTypeIds = new Dictionary<int, IEnumerable<int>>();
                var mappedMediaTypes = new List<IMediaType>();

                foreach (var contentTypeId in mediaTypeIds)
                {
                    //the current content type id that we're working with

                    var currentCtId = contentTypeId;

                    //first we want to get the main content type data this is 1 : 1 with umbraco node data

                    var ct = result
                        .Where(x => x.ctId == currentCtId)
                        .Select(x => new { x.ctPk, x.ctId, x.ctAlias, x.ctAllowAtRoot, x.ctDesc, x.ctIcon, x.ctIsContainer, x.ctThumb, x.nName, x.nCreateDate, x.nLevel, x.nObjectType, x.nUser, x.nParentId, x.nPath, x.nSortOrder, x.nTrashed, x.nUniqueId })
                        .DistinctBy(x => (int)x.ctId)
                        .FirstOrDefault();

                    if (ct == null)
                    {
                        continue;
                    }

                    var contentTypeDto = new ContentTypeDto
                    {
                        Alias = ct.ctAlias,
                        AllowAtRoot = ct.ctAllowAtRoot,
                        Description = ct.ctDesc,
                        Icon = ct.ctIcon,
                        IsContainer = ct.ctIsContainer,
                        NodeId = ct.ctId,
                        PrimaryKey = ct.ctPk,
                        Thumbnail = ct.ctThumb,
                        //map the underlying node dto
                        NodeDto = new NodeDto
                        {
                            CreateDate = ct.nCreateDate,
                            Level = (short) ct.nLevel,
                            NodeId = ct.ctId,
                            NodeObjectType = ct.nObjectType,
                            ParentId = ct.nParentId,
                            Path = ct.nPath,
                            SortOrder = ct.nSortOrder,
                            Text = ct.nName,
                            Trashed = ct.nTrashed,
                            UniqueId = ct.nUniqueId,
                            UserId = ct.nUser
                        }
                    };
                 
                    //now create the media type object

                    var factory = new MediaTypeFactory(new Guid(Constants.ObjectTypes.MediaType));
                    var mediaType = factory.BuildEntity(contentTypeDto);

                    //map the allowed content types
                    //map the child content type ids
                    MapCommonContentTypeObjects(mediaType, currentCtId, result, parentMediaTypeIds);

                    mappedMediaTypes.Add(mediaType);
                }

                return mappedMediaTypes;
            }

            internal static IEnumerable<IContentType> MapContentTypes(int[] contentTypeIds, Database db, ISqlSyntaxProvider sqlSyntax,
                out IDictionary<int, IEnumerable<AssociatedTemplate>> associatedTemplates,
                out IDictionary<int, IEnumerable<int>> parentContentTypeIds)
            {
                Mandate.ParameterNotNull(db, "db");

                //ensure they are unique
                contentTypeIds = contentTypeIds.Distinct().ToArray();

                var sql = @"SELECT cmsDocumentType.IsDefault as dtIsDefault, cmsDocumentType.templateNodeId as dtTemplateId,
                                cmsContentType.pk as ctPk, cmsContentType.alias as ctAlias, cmsContentType.allowAtRoot as ctAllowAtRoot, cmsContentType.description as ctDesc,
                                cmsContentType.icon as ctIcon, cmsContentType.isContainer as ctIsContainer, cmsContentType.nodeId as ctId, cmsContentType.thumbnail as ctThumb,
                                AllowedTypes.allowedId as ctaAllowedId, AllowedTypes.SortOrder as ctaSortOrder, AllowedTypes.alias as ctaAlias,		                        
                                ParentTypes.parentContentTypeId as chtParentId,
                                umbracoNode.createDate as nCreateDate, umbracoNode." + sqlSyntax.GetQuotedColumnName("level") + @" as nLevel, umbracoNode.nodeObjectType as nObjectType, umbracoNode.nodeUser as nUser,
		                        umbracoNode.parentID as nParentId, umbracoNode." + sqlSyntax.GetQuotedColumnName("path") + @" as nPath, umbracoNode.sortOrder as nSortOrder, umbracoNode." + sqlSyntax.GetQuotedColumnName("text") + @" as nName, umbracoNode.trashed as nTrashed,
                                umbracoNode.uniqueID as nUniqueId,                                
                                Template.alias as tAlias, Template.nodeId as tId,Template.text as tText
                        FROM cmsContentType
                        INNER JOIN umbracoNode
                        ON cmsContentType.nodeId = umbracoNode.id
                        LEFT JOIN cmsDocumentType
                        ON cmsDocumentType.contentTypeNodeId = cmsContentType.nodeId
                        LEFT JOIN (
                            SELECT cmsContentTypeAllowedContentType.Id, cmsContentTypeAllowedContentType.AllowedId, cmsContentType.alias, cmsContentTypeAllowedContentType.SortOrder 
                            FROM cmsContentTypeAllowedContentType	
                            INNER JOIN cmsContentType
                            ON cmsContentTypeAllowedContentType.AllowedId = cmsContentType.nodeId
                        ) AllowedTypes
                        ON AllowedTypes.Id = cmsContentType.nodeId
                        LEFT JOIN (
                            SELECT * FROM cmsTemplate
                            INNER JOIN umbracoNode
                            ON cmsTemplate.nodeId = umbracoNode.id
                        ) as Template
                        ON Template.nodeId = cmsDocumentType.templateNodeId
                        LEFT JOIN cmsContentType2ContentType as ParentTypes
                        ON ParentTypes.childContentTypeId = cmsContentType.nodeId	
                        WHERE (umbracoNode.nodeObjectType = @nodeObjectType)";
                if(contentTypeIds.Any())                        
                    sql = sql + " AND (umbracoNode.id IN (@contentTypeIds))";

                //NOTE: we are going to assume there's not going to be more than 2100 content type ids since that is the max SQL param count!
                if ((contentTypeIds.Length - 1) > 2000)
                    throw new InvalidOperationException("Cannot perform this lookup, too many sql parameters");

                var result = db.Fetch<dynamic>(sql, new { nodeObjectType = new Guid(Constants.ObjectTypes.DocumentType), contentTypeIds = contentTypeIds });

                if (result.Any() == false)
                {
                    parentContentTypeIds = null;
                    associatedTemplates = null;
                    return Enumerable.Empty<IContentType>();
                }

                parentContentTypeIds = new Dictionary<int, IEnumerable<int>>();
                associatedTemplates = new Dictionary<int, IEnumerable<AssociatedTemplate>>();
                var mappedContentTypes = new List<IContentType>();

                foreach (var contentTypeId in contentTypeIds)
                {
                    //the current content type id that we're working with

                    var currentCtId = contentTypeId;

                    //first we want to get the main content type data this is 1 : 1 with umbraco node data

                    var ct = result
                        .Where(x => x.ctId == currentCtId)
                        .Select(x => new { x.ctPk, x.ctId, x.ctAlias, x.ctAllowAtRoot, x.ctDesc, x.ctIcon, x.ctIsContainer, x.ctThumb, x.nName, x.nCreateDate, x.nLevel, x.nObjectType, x.nUser, x.nParentId, x.nPath, x.nSortOrder, x.nTrashed, x.nUniqueId })
                        .DistinctBy(x => (int)x.ctId)
                        .FirstOrDefault();

                    if (ct == null)
                    {
                        continue;
                    }

                    //get the unique list of associated templates
                    var defaultTemplates = result
                        .Where(x => x.ctId == currentCtId)
                        //use a tuple so that distinct checks both values (in some rare cases the dtIsDefault will not compute as bool?, so we force it with Convert.ToBoolean)
                        .Select(x => new Tuple<bool?, int?>(Convert.ToBoolean(x.dtIsDefault), x.dtTemplateId))
                        .Where(x => x.Item1.HasValue && x.Item2.HasValue)
                        .Distinct()
                        .OrderByDescending(x => x.Item1.Value)
                        .ToArray();
                    //if there isn't one set to default explicitly, we'll pick the first one
                    var defaultTemplate = defaultTemplates.FirstOrDefault(x => x.Item1.Value)
                        ?? defaultTemplates.FirstOrDefault();

                    var dtDto = new DocumentTypeDto
                    {
                        //create the content type dto
                        ContentTypeDto = new ContentTypeDto
                        {
                            Alias = ct.ctAlias,
                            AllowAtRoot = ct.ctAllowAtRoot,
                            Description = ct.ctDesc,
                            Icon = ct.ctIcon,
                            IsContainer = ct.ctIsContainer,
                            NodeId = ct.ctId,
                            PrimaryKey = ct.ctPk,
                            Thumbnail = ct.ctThumb,
                            //map the underlying node dto
                            NodeDto = new NodeDto
                            {
                                CreateDate = ct.nCreateDate,
                                Level = (short)ct.nLevel,
                                NodeId = ct.ctId,
                                NodeObjectType = ct.nObjectType,
                                ParentId = ct.nParentId,
                                Path = ct.nPath,
                                SortOrder = ct.nSortOrder,
                                Text = ct.nName,
                                Trashed = ct.nTrashed,
                                UniqueId = ct.nUniqueId,
                                UserId = ct.nUser
                            }
                        },
                        ContentTypeNodeId = ct.ctId,
                        IsDefault = defaultTemplate != null,
                        TemplateNodeId = defaultTemplate != null ? defaultTemplate.Item2.Value : 0,
                    };

                    // We will map a subset of the associated template - alias, id, name

                    associatedTemplates.Add(currentCtId, result
                        .Where(x => x.ctId == currentCtId)
                        .Where(x => x.tId != null)
                        .Select(x => new AssociatedTemplate(x.tId, x.tAlias, x.tText))
                        .Distinct()
                        .ToArray());

                    //now create the content type object

                    var factory = new ContentTypeFactory(new Guid(Constants.ObjectTypes.DocumentType));
                    var contentType = factory.BuildEntity(dtDto);

                    //map the allowed content types
                    //map the child content type ids
                    MapCommonContentTypeObjects(contentType, currentCtId, result, parentContentTypeIds);
                    
                    mappedContentTypes.Add(contentType);
                }

                return mappedContentTypes;
            }

            private static void MapCommonContentTypeObjects<T>(T contentType, int currentCtId, List<dynamic> result, IDictionary<int, IEnumerable<int>> parentContentTypeIds)
                where T: IContentTypeBase
            {
                //map the allowed content types
                contentType.AllowedContentTypes = result
                    .Where(x => x.ctId == currentCtId)
                    //use tuple so we can use distinct on all vals
                    .Select(x => new Tuple<int?, int?, string>(x.ctaAllowedId, x.ctaSortOrder, x.ctaAlias))
                    .Where(x => x.Item1.HasValue && x.Item2.HasValue && x.Item3 != null)
                    .Distinct()
                    .Select(x => new ContentTypeSort(new Lazy<int>(() => x.Item1.Value), x.Item2.Value, x.Item3))
                    .ToList();

                //map the child content type ids
                parentContentTypeIds.Add(currentCtId, result
                    .Where(x => x.ctId == currentCtId)
                    .Select(x => (int?)x.chtParentId)
                    .Where(x => x.HasValue)
                    .Distinct()
                    .Select(x => x.Value).ToList());
            }

            internal static void MapGroupsAndProperties(int[] contentTypeIds, Database db, ISqlSyntaxProvider sqlSyntax,
                out IDictionary<int, PropertyTypeCollection> allPropertyTypeCollection,
                out IDictionary<int, PropertyGroupCollection> allPropertyGroupCollection)
            {   

                // first part Gets all property groups including property type data even when no property type exists on the group
                // second part Gets all property types including ones that are not on a group
                // therefore the union of the two contains all of the property type and property group information we need
                // NOTE: MySQL requires a SELECT * FROM the inner union in order to be able to sort . lame.

                var sqlBuilder = new StringBuilder(@"SELECT PG.contenttypeNodeId as contentTypeId,
                            PT.ptId, PT.ptAlias, PT.ptDesc,PT.ptMandatory,PT.ptName,PT.ptSortOrder,PT.ptRegExp, 
                            PT.dtId,PT.dtDbType,PT.dtPropEdAlias,
                            PG.id as pgId, PG.parentGroupId as pgParentGroupId, PG.sortorder as pgSortOrder, PG." + sqlSyntax.GetQuotedColumnName("text") + @" as pgText
                        FROM cmsPropertyTypeGroup as PG
                        LEFT JOIN
                        (
                            SELECT PT.id as ptId, PT.Alias as ptAlias, PT." + sqlSyntax.GetQuotedColumnName("Description") + @" as ptDesc, 
                                    PT.mandatory as ptMandatory, PT.Name as ptName, PT.sortOrder as ptSortOrder, PT.validationRegExp as ptRegExp,
                                    PT.propertyTypeGroupId as ptGroupId,
                                    DT.dbType as dtDbType, DT.nodeId as dtId, DT.propertyEditorAlias as dtPropEdAlias
                            FROM cmsPropertyType as PT
                            INNER JOIN cmsDataType as DT
                            ON PT.dataTypeId = DT.nodeId
                        )  as  PT
                        ON PT.ptGroupId = PG.id
                        WHERE (PG.contenttypeNodeId in (@contentTypeIds))
                
                        UNION

                        SELECT  PT.contentTypeId as contentTypeId,
                                PT.id as ptId, PT.Alias as ptAlias, PT." + sqlSyntax.GetQuotedColumnName("Description") + @" as ptDesc, 
                                PT.mandatory as ptMandatory, PT.Name as ptName, PT.sortOrder as ptSortOrder, PT.validationRegExp as ptRegExp,
                                DT.nodeId as dtId, DT.dbType as dtDbType, DT.propertyEditorAlias as dtPropEdAlias,
                                PG.id as pgId, PG.parentGroupId as pgParentGroupId, PG.sortorder as pgSortOrder, PG." + sqlSyntax.GetQuotedColumnName("text") + @" as pgText
                        FROM cmsPropertyType as PT
                        INNER JOIN cmsDataType as DT
                        ON PT.dataTypeId = DT.nodeId
                        LEFT JOIN cmsPropertyTypeGroup as PG
                        ON PG.id = PT.propertyTypeGroupId");

                if(contentTypeIds.Any())                        
                    sqlBuilder.AppendLine(" WHERE (PT.contentTypeId in (@contentTypeIds))");

                sqlBuilder.AppendLine(" ORDER BY (pgId)");

                //NOTE: we are going to assume there's not going to be more than 2100 content type ids since that is the max SQL param count!
                // Since there are 2 groups of params, it will be half!
                if (((contentTypeIds.Length / 2) - 1) > 2000)
                    throw new InvalidOperationException("Cannot perform this lookup, too many sql parameters");

                var result = db.Fetch<dynamic>(sqlBuilder.ToString(), new { contentTypeIds = contentTypeIds });

                allPropertyGroupCollection = new Dictionary<int, PropertyGroupCollection>();
                allPropertyTypeCollection = new Dictionary<int, PropertyTypeCollection>();

                foreach (var contentTypeId in contentTypeIds)
                {
                    //from this we need to make :
                    // * PropertyGroupCollection - Contains all property groups along with all property types associated with a group
                    // * PropertyTypeCollection - Contains all property types that do not belong to a group

                    //create the property group collection first, this means all groups (even empty ones) and all groups with properties

                    int currId = contentTypeId;

                    var propertyGroupCollection = new PropertyGroupCollection(result                        
                        //get all rows that have a group id
                        .Where(x => x.pgId != null)
                        //filter based on the current content type
                        .Where(x => x.contentTypeId == currId)
                        //turn that into a custom object containing only the group info
                        .Select(x => new { GroupId = x.pgId, ParentGroupId = x.pgParentGroupId, SortOrder = x.pgSortOrder, Text = x.pgText })
                        //get distinct data by id
                        .DistinctBy(x => (int)x.GroupId)
                        //for each of these groups, create a group object with it's associated properties
                        .Select(group => new PropertyGroup(new PropertyTypeCollection(
                            result
                                .Where(row => row.pgId == group.GroupId && row.ptId != null)
                                .Select(row => new PropertyType(row.dtPropEdAlias, Enum<DataTypeDatabaseType>.Parse(row.dtDbType), row.ptAlias)
                                {
                                    //fill in the rest of the property type properties
                                    Description = row.ptDesc,
                                    DataTypeDefinitionId = row.dtId,
                                    Id = row.ptId,
                                    Mandatory = Convert.ToBoolean(row.ptMandatory),
                                    Name = row.ptName,
                                    PropertyGroupId = new Lazy<int>(() => group.GroupId, false),
                                    SortOrder = row.ptSortOrder,
                                    ValidationRegExp = row.ptRegExp
                                })))
                        {
                            //fill in the rest of the group properties
                            Id = group.GroupId,
                            Name = group.Text,
                            ParentId = group.ParentGroupId,
                            SortOrder = group.SortOrder
                        }).ToArray());

                    allPropertyGroupCollection[currId] = propertyGroupCollection;

                    //Create the property type collection now (that don't have groups)

                    var propertyTypeCollection = new PropertyTypeCollection(result                        
                        .Where(x => x.pgId == null)
                        //filter based on the current content type
                        .Where(x => x.contentTypeId == currId)
                        .Select(row => new PropertyType(row.dtPropEdAlias, Enum<DataTypeDatabaseType>.Parse(row.dtDbType), row.ptAlias)
                        {
                            //fill in the rest of the property type properties
                            Description = row.ptDesc,
                            DataTypeDefinitionId = row.dtId,
                            Id = row.ptId,
                            Mandatory = Convert.ToBoolean(row.ptMandatory),
                            Name = row.ptName,
                            PropertyGroupId = null,
                            SortOrder = row.ptSortOrder,
                            ValidationRegExp = row.ptRegExp
                        }).ToArray());

                    allPropertyTypeCollection[currId] = propertyTypeCollection;
                }

                
            }

        }
    }
}