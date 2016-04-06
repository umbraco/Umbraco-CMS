using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Events;
using Umbraco.Core.Exceptions;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Models.Rdbms;

using Umbraco.Core.Persistence.Factories;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.Relators;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Persistence.UnitOfWork;
using Umbraco.Core.Services;

namespace Umbraco.Core.Persistence.Repositories
{
    /// <summary>
    /// Represent an abstract Repository for ContentType based repositories
    /// </summary>
    /// <remarks>Exposes shared functionality</remarks>
    /// <typeparam name="TEntity"></typeparam>
    internal abstract class ContentTypeBaseRepository<TEntity> : PetaPocoRepositoryBase<int, TEntity>, IReadRepository<Guid, TEntity>
        where TEntity : class, IContentTypeComposition
    {
        protected ContentTypeBaseRepository(IDatabaseUnitOfWork work, CacheHelper cache, ILogger logger, ISqlSyntaxProvider sqlSyntax)
            : base(work, cache, logger, sqlSyntax)
        {
        }

        public IEnumerable<MoveEventInfo<TEntity>> Move(TEntity toMove, EntityContainer container)
        {
            var parentId = Constants.System.Root;
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
            var moveInfo = new List<MoveEventInfo<TEntity>>
            {
                new MoveEventInfo<TEntity>(toMove, toMove.Path, parentId)
            };


            // get the level delta (old pos to new pos)
            var levelDelta = container == null
                ? 1 - toMove.Level
                : container.Level + 1 - toMove.Level;

            // move to parent (or -1), update path, save
            toMove.ParentId = parentId;
            var toMovePath = toMove.Path + ","; // save before changing
            toMove.Path = (container == null ? Constants.System.Root.ToString() : container.Path) + "," + toMove.Id;
            toMove.Level = container == null ? 1 : container.Level + 1;
            AddOrUpdate(toMove);

            //update all descendants, update in order of level
            var descendants = GetByQuery(new Query<TEntity>().Where(type => type.Path.StartsWith(toMovePath)));
            var paths = new Dictionary<int, string>();
            paths[toMove.Id] = toMove.Path;

            foreach (var descendant in descendants.OrderBy(x => x.Level))
            {
                moveInfo.Add(new MoveEventInfo<TEntity>(descendant, descendant.Path, descendant.ParentId));

                descendant.Path = paths[descendant.Id] = paths[descendant.ParentId] + "," + descendant.Id;
                descendant.Level += levelDelta;

                AddOrUpdate(descendant);
            }

            return moveInfo;
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
               .From<PropertyTypeGroupDto>(SqlSyntax)
               .RightJoin<PropertyTypeDto>(SqlSyntax)
               .On<PropertyTypeGroupDto, PropertyTypeDto>(SqlSyntax, left => left.Id, right => right.PropertyTypeGroupId)
               .InnerJoin<DataTypeDto>(SqlSyntax)
               .On<PropertyTypeDto, DataTypeDto>(SqlSyntax, left => left.DataTypeId, right => right.DataTypeId);

            var translator = new SqlTranslator<PropertyType>(sqlClause, query);
            var sql = translator.Translate()
                                .OrderBy<PropertyTypeDto>(x => x.PropertyTypeGroupId, SqlSyntax);

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

        protected void PersistNewBaseContentType(IContentTypeComposition entity)
        {
            var factory = new ContentTypeFactory();
            var dto = factory.BuildContentTypeDto(entity);

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
                    var contentTypeDto = Database.FirstOrDefault<ContentTypeDto>("WHERE alias = @Alias", new { Alias = composition.Alias });
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

        protected void PersistUpdatedBaseContentType(IContentTypeComposition entity)
        {
            var factory = new ContentTypeFactory();
            var dto = factory.BuildContentTypeDto(entity);

            // ensure the alias is not used already
            var exists = Database.ExecuteScalar<int>(@"SELECT COUNT(*) FROM cmsContentType
INNER JOIN umbracoNode ON cmsContentType.nodeId = umbracoNode.id
WHERE cmsContentType." + SqlSyntax.GetQuotedColumnName("alias") + @"= @alias
AND umbracoNode.nodeObjectType = @objectType
AND umbracoNode.id <> @id",
                new { id = dto.NodeId, alias = dto.Alias, objectType = NodeObjectTypeId });
            if (exists > 0)
                throw new DuplicateNameException("An item with the alias " + dto.Alias + " already exists");

            // handle (update) the node
            var nodeDto = dto.NodeDto;
            Database.Update(nodeDto);

            // fixme - why? we are UPDATING so we should ALREADY have a PK!
            //Look up ContentType entry to get PrimaryKey for updating the DTO
            var dtoPk = Database.First<ContentTypeDto>("WHERE nodeId = @Id", new { Id = entity.Id });
            dto.PrimaryKey = dtoPk.PrimaryKey;
            Database.Update(dto);

            // handle (delete then recreate) compositions
            Database.Delete<ContentType2ContentTypeDto>("WHERE childContentTypeId = @Id", new { Id = entity.Id });
            foreach (var composition in entity.ContentTypeComposition)
                Database.Insert(new ContentType2ContentTypeDto { ParentId = composition.Id, ChildId = entity.Id });

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
                    var propertyTypes = Database.Fetch<PropertyTypeDto>("WHERE contentTypeId = @Id", new { Id = key });
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
            Database.Delete<ContentTypeAllowedContentTypeDto>("WHERE Id = @Id", new { Id = entity.Id });
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


            if (((ICanBeDirty)entity).IsPropertyDirty("PropertyTypes") || entity.PropertyTypes.Any(x => x.IsDirty()))
            {
                //Delete PropertyTypes by excepting entries from db with entries from collections
                var dbPropertyTypes = Database.Fetch<PropertyTypeDto>("WHERE contentTypeId = @Id", new { Id = entity.Id });
                var dbPropertyTypeAlias = dbPropertyTypes.Select(x => x.Id);
                var entityPropertyTypes = entity.PropertyTypes.Where(x => x.HasIdentity).Select(x => x.Id);
                var items = dbPropertyTypeAlias.Except(entityPropertyTypes);
                foreach (var item in items)
                {
                    //Before a PropertyType can be deleted, all Properties based on that PropertyType should be deleted.
                    Database.Delete<TagRelationshipDto>("WHERE propertyTypeId = @Id", new { Id = item });
                    Database.Delete<PropertyDataDto>("WHERE propertytypeid = @Id", new { Id = item });
                    Database.Delete<PropertyTypeDto>("WHERE contentTypeId = @Id AND id = @PropertyTypeId",
                                                     new { Id = entity.Id, PropertyTypeId = item });
                }
            }

            if (entity.IsPropertyDirty("PropertyGroups") || entity.PropertyGroups.Any(x => x.IsDirty()))
            {
                // todo
                // we used to try to propagate tabs renaming downstream, relying on ParentId, but
                // 1) ParentId makes no sense (if a tab can be inherited from multiple composition
                //    types) so we would need to figure things out differently, visiting downstream
                //    content types and looking for tabs with the same name...
                // 2) It was not deployable as changing a content type changes other content types
                //    that was not deterministic, because it would depend on the order of the changes.
                // That last point could be fixed if (1) is fixed, but then it still is an issue with
                // deploy because changing a content type changes other content types that are not
                // dependencies but dependents, and then what?
                //
                // So... for the time being, all renaming propagation is disabled. We just don't do it.

                // (all gone)

                // delete tabs that do not exist anymore
                // get the tabs that are currently existing (in the db)
                // get the tabs that we want, now
                // and derive the tabs that we want to delete
                var existingPropertyGroups = Database.Fetch<PropertyTypeGroupDto>("WHERE contentTypeNodeId = @id", new { id = entity.Id })
                    .Select(x => x.Id)
                    .ToList();
                var newPropertyGroups = entity.PropertyGroups.Select(x => x.Id).ToList();
                var tabsToDelete = existingPropertyGroups
                    .Except(newPropertyGroups)
                    .ToArray();

                // move properties to generic properties, and delete the tabs
                if (tabsToDelete.Length > 0)
                {
                    Database.Update<PropertyTypeDto>("SET propertyTypeGroupId=NULL WHERE propertyTypeGroupId IN (@ids)", new { ids = tabsToDelete });
                    Database.Delete<PropertyTypeGroupDto>("WHERE id IN (@ids)", new { ids = tabsToDelete });
                }
            }
            var propertyGroupFactory = new PropertyGroupFactory(entity.Id);

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
                propType.Key = dto.UniqueId;
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
                Database db, ISqlSyntaxProvider sqlSyntax,
                TRepo contentTypeRepository)
                where TRepo : IReadRepository<int, TEntity>
            {
                IDictionary<int, List<int>> allParentMediaTypeIds;
                var mediaTypes = MapMediaTypes(db, sqlSyntax, out allParentMediaTypeIds)
                    .ToArray();

                MapContentTypeChildren(mediaTypes, db, sqlSyntax, contentTypeRepository, allParentMediaTypeIds);

                return mediaTypes;
            }

            public static IEnumerable<IContentType> GetContentTypes<TRepo>(
                Database db, ISqlSyntaxProvider sqlSyntax,
                TRepo contentTypeRepository,
                ITemplateRepository templateRepository)
                where TRepo : IReadRepository<int, TEntity>
            {
                IDictionary<int, List<AssociatedTemplate>> allAssociatedTemplates;
                IDictionary<int, List<int>> allParentContentTypeIds;
                var contentTypes = MapContentTypes(db, sqlSyntax, out allAssociatedTemplates, out allParentContentTypeIds)
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
                IDictionary<int, List<int>> allParentContentTypeIds)
                where TRepo : IReadRepository<int, TEntity>
            {
                //NOTE: SQL call #2

                var ids = contentTypes.Select(x => x.Id).ToArray();
                IDictionary<int, PropertyGroupCollection> allPropGroups;
                IDictionary<int, PropertyTypeCollection> allPropTypes;
                MapGroupsAndProperties(ids, db, sqlSyntax, out allPropTypes, out allPropGroups);

                foreach (var contentType in contentTypes)
                {
                    contentType.PropertyGroups = allPropGroups[contentType.Id];
                    contentType.NoGroupPropertyTypes = allPropTypes[contentType.Id];
                }

                //NOTE: SQL call #3++

                if (allParentContentTypeIds != null)
                {
                    var allParentIdsAsArray = allParentContentTypeIds.SelectMany(x => x.Value).Distinct().ToArray();
                    if (allParentIdsAsArray.Any())
                    {
                        var allParentContentTypes = contentTypes.Where(x => allParentIdsAsArray.Contains(x.Id)).ToArray();

                        foreach (var contentType in contentTypes)
                        {                            
                            var entityId = contentType.Id;

                            var parentContentTypes = allParentContentTypes.Where(x =>
                            {
                                var parentEntityId = x.Id;

                                return allParentContentTypeIds[entityId].Contains(parentEntityId);
                            });
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
                IDictionary<int, List<AssociatedTemplate>> associatedTemplates)
                where TRepo : IReadRepository<int, TEntity>
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
                    var entityId = contentType.Id;

                    var associatedTemplateIds = associatedTemplates[entityId].Select(x => x.TemplateId)
                        .Distinct()
                        .ToArray();

                    contentType.AllowedTemplates = (associatedTemplateIds.Any()
                        ? templates.Where(x => associatedTemplateIds.Contains(x.Id))
                        : Enumerable.Empty<ITemplate>()).ToArray();
                }


            }

            internal static IEnumerable<IMediaType> MapMediaTypes(Database db, ISqlSyntaxProvider sqlSyntax,
                out IDictionary<int, List<int>> parentMediaTypeIds)
            {
                Mandate.ParameterNotNull(db, "db");
                
                var sql = @"SELECT cmsContentType.pk as ctPk, cmsContentType.alias as ctAlias, cmsContentType.allowAtRoot as ctAllowAtRoot, cmsContentType.description as ctDesc,
                                cmsContentType.icon as ctIcon, cmsContentType.isContainer as ctIsContainer, cmsContentType.nodeId as ctId, cmsContentType.thumbnail as ctThumb,
                                AllowedTypes.AllowedId as ctaAllowedId, AllowedTypes.SortOrder as ctaSortOrder, AllowedTypes.alias as ctaAlias,		                        
                                ParentTypes.parentContentTypeId as chtParentId, ParentTypes.parentContentTypeKey as chtParentKey,
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
                        LEFT JOIN (
                            SELECT cmsContentType2ContentType.parentContentTypeId, umbracoNode.uniqueID AS parentContentTypeKey, cmsContentType2ContentType.childContentTypeId
                            FROM cmsContentType2ContentType
                            INNER JOIN umbracoNode
                            ON cmsContentType2ContentType.parentContentTypeId = umbracoNode." + sqlSyntax.GetQuotedColumnName("id") + @"
                        ) ParentTypes
                        ON ParentTypes.childContentTypeId = cmsContentType.nodeId
                        WHERE (umbracoNode.nodeObjectType = @nodeObjectType)
                        ORDER BY ctId";
                
                var result = db.Fetch<dynamic>(sql, new { nodeObjectType = new Guid(Constants.ObjectTypes.MediaType) });

                if (result.Any() == false)
                {
                    parentMediaTypeIds = null;
                    return Enumerable.Empty<IMediaType>();
                }

                parentMediaTypeIds = new Dictionary<int, List<int>>();
                var mappedMediaTypes = new List<IMediaType>();

                //loop through each result and fill in our required values, each row will contain different requried data than the rest.
                // it is much quicker to iterate each result and populate instead of looking up the values over and over in the result like
                // we used to do.
                var queue = new Queue<dynamic>(result);
                var currAllowedContentTypes = new List<ContentTypeSort>();

                while (queue.Count > 0)
                {
                    var ct = queue.Dequeue();

                    //check for allowed content types
                    int? allowedCtId = ct.ctaAllowedId;
                    int? allowedCtSort = ct.ctaSortOrder;
                    string allowedCtAlias = ct.ctaAlias;
                    if (allowedCtId.HasValue && allowedCtSort.HasValue && allowedCtAlias != null)
                    {
                        var ctSort = new ContentTypeSort(new Lazy<int>(() => allowedCtId.Value), allowedCtSort.Value, allowedCtAlias);
                        if (currAllowedContentTypes.Contains(ctSort) == false)
                        {
                            currAllowedContentTypes.Add(ctSort);
                        }
                    }

                    //always ensure there's a list for this content type
                    if (parentMediaTypeIds.ContainsKey(ct.ctId) == false)
                        parentMediaTypeIds[ct.ctId] = new List<int>();

                    //check for parent ids and assign to the outgoing collection
                    int? parentId = ct.chtParentId;
                    if (parentId.HasValue)
                    {
                        var associatedParentIds = parentMediaTypeIds[ct.ctId];
                        if (associatedParentIds.Contains(parentId.Value) == false)
                            associatedParentIds.Add(parentId.Value);
                    }

                    if (queue.Count == 0 || queue.Peek().ctId != ct.ctId)
                    {
                        //it's the last in the queue or the content type is changing (moving to the next one)
                        var mediaType = CreateForMapping(ct, currAllowedContentTypes);
                        mappedMediaTypes.Add(mediaType);

                        //Here we need to reset the current variables, we're now collecting data for a different content type
                        currAllowedContentTypes = new List<ContentTypeSort>();
                    }
                }

                return mappedMediaTypes;
            }

            private static IMediaType CreateForMapping(dynamic currCt, List<ContentTypeSort> currAllowedContentTypes)
            {
                // * create the DTO object
                // * create the content type object
                // * map the allowed content types
                // * add to the outgoing list

                var contentTypeDto = new ContentTypeDto
                {
                    Alias = currCt.ctAlias,
                    AllowAtRoot = currCt.ctAllowAtRoot,
                    Description = currCt.ctDesc,
                    Icon = currCt.ctIcon,
                    IsContainer = currCt.ctIsContainer,
                    NodeId = currCt.ctId,
                    PrimaryKey = currCt.ctPk,
                    Thumbnail = currCt.ctThumb,
                    //map the underlying node dto
                    NodeDto = new NodeDto
                    {
                        CreateDate = currCt.nCreateDate,
                        Level = (short)currCt.nLevel,
                        NodeId = currCt.ctId,
                        NodeObjectType = currCt.nObjectType,
                        ParentId = currCt.nParentId,
                        Path = currCt.nPath,
                        SortOrder = currCt.nSortOrder,
                        Text = currCt.nName,
                        Trashed = currCt.nTrashed,
                        UniqueId = currCt.nUniqueId,
                        UserId = currCt.nUser
                    }
                };

                //now create the content type object

                var factory = new ContentTypeFactory();
                var mediaType = factory.BuildMediaTypeEntity(contentTypeDto);

                //map the allowed content types
                mediaType.AllowedContentTypes = currAllowedContentTypes;

                return mediaType;
            }

            internal static IEnumerable<IContentType> MapContentTypes(Database db, ISqlSyntaxProvider sqlSyntax,                
                out IDictionary<int, List<AssociatedTemplate>> associatedTemplates,
                out IDictionary<int, List<int>> parentContentTypeIds)
            {
                Mandate.ParameterNotNull(db, "db");
                
                var sql = @"SELECT cmsDocumentType.IsDefault as dtIsDefault, cmsDocumentType.templateNodeId as dtTemplateId,
                                cmsContentType.pk as ctPk, cmsContentType.alias as ctAlias, cmsContentType.allowAtRoot as ctAllowAtRoot, cmsContentType.description as ctDesc,
                                cmsContentType.icon as ctIcon, cmsContentType.isContainer as ctIsContainer, cmsContentType.nodeId as ctId, cmsContentType.thumbnail as ctThumb,
                                AllowedTypes.AllowedId as ctaAllowedId, AllowedTypes.SortOrder as ctaSortOrder, AllowedTypes.alias as ctaAlias,		                        
                                ParentTypes.parentContentTypeId as chtParentId,ParentTypes.parentContentTypeKey as chtParentKey,
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
                        LEFT JOIN (
                            SELECT cmsContentType2ContentType.parentContentTypeId, umbracoNode.uniqueID AS parentContentTypeKey, cmsContentType2ContentType.childContentTypeId
                            FROM cmsContentType2ContentType
                            INNER JOIN umbracoNode
                            ON cmsContentType2ContentType.parentContentTypeId = umbracoNode." + sqlSyntax.GetQuotedColumnName("id") + @"
                        ) ParentTypes
                        ON ParentTypes.childContentTypeId = cmsContentType.nodeId
                        WHERE (umbracoNode.nodeObjectType = @nodeObjectType)
                        ORDER BY ctId";
                
                var result = db.Fetch<dynamic>(sql, new { nodeObjectType = new Guid(Constants.ObjectTypes.DocumentType)});

                if (result.Any() == false)
                {
                    parentContentTypeIds = null;
                    associatedTemplates = null;
                    return Enumerable.Empty<IContentType>();
                }

                parentContentTypeIds = new Dictionary<int, List<int>>();
                associatedTemplates = new Dictionary<int, List<AssociatedTemplate>>();
                var mappedContentTypes = new List<IContentType>();

                var queue = new Queue<dynamic>(result);
                var currDefaultTemplate = -1;
                var currAllowedContentTypes = new List<ContentTypeSort>();
                while (queue.Count > 0)
                {
                    var ct = queue.Dequeue();

                    //check for default templates                    
                    bool? isDefaultTemplate = Convert.ToBoolean(ct.dtIsDefault);
                    int? templateId = ct.dtTemplateId;
                    if (currDefaultTemplate == -1 && isDefaultTemplate.HasValue && isDefaultTemplate.Value && templateId.HasValue)
                    {
                        currDefaultTemplate = templateId.Value;
                    }

                    //always ensure there's a list for this content type
                    if (associatedTemplates.ContainsKey(ct.ctId) == false)
                        associatedTemplates[ct.ctId] = new List<AssociatedTemplate>();

                    //check for associated templates and assign to the outgoing collection
                    if (ct.tId != null)
                    {
                        var associatedTemplate = new AssociatedTemplate(ct.tId, ct.tAlias, ct.tText);
                        var associatedList = associatedTemplates[ct.ctId];

                        if (associatedList.Contains(associatedTemplate) == false)
                            associatedList.Add(associatedTemplate);
                    }

                    //check for allowed content types
                    int? allowedCtId = ct.ctaAllowedId;
                    int? allowedCtSort = ct.ctaSortOrder;
                    string allowedCtAlias = ct.ctaAlias;
                    if (allowedCtId.HasValue && allowedCtSort.HasValue && allowedCtAlias != null)
                    {
                        var ctSort = new ContentTypeSort(new Lazy<int>(() => allowedCtId.Value), allowedCtSort.Value, allowedCtAlias);
                        if (currAllowedContentTypes.Contains(ctSort) == false)
                        {
                            currAllowedContentTypes.Add(ctSort);
                        }
                    }

                    //always ensure there's a list for this content type
                    if (parentContentTypeIds.ContainsKey(ct.ctId) == false)
                        parentContentTypeIds[ct.ctId] = new List<int>();

                    //check for parent ids and assign to the outgoing collection
                    int? parentId = ct.chtParentId;
                    if (parentId.HasValue)
                    {
                        var associatedParentIds = parentContentTypeIds[ct.ctId];

                        if (associatedParentIds.Contains(parentId.Value) == false)
                            associatedParentIds.Add(parentId.Value);
                    }

                    if (queue.Count == 0 || queue.Peek().ctId != ct.ctId)
                    {
                        //it's the last in the queue or the content type is changing (moving to the next one)
                        var contentType = CreateForMapping(ct, currAllowedContentTypes, currDefaultTemplate);
                        mappedContentTypes.Add(contentType);

                        //Here we need to reset the current variables, we're now collecting data for a different content type
                        currDefaultTemplate = -1;
                        currAllowedContentTypes = new List<ContentTypeSort>();
                    }
                }

                return mappedContentTypes;
            }

            private static IContentType CreateForMapping(dynamic currCt, List<ContentTypeSort> currAllowedContentTypes, int currDefaultTemplate)
            {
                // * set the default template to the first one if a default isn't found
                // * create the DTO object
                // * create the content type object
                // * map the allowed content types
                // * add to the outgoing list

                var dtDto = new ContentTypeTemplateDto
                {
                    //create the content type dto
                    ContentTypeDto = new ContentTypeDto
                    {
                        Alias = currCt.ctAlias,
                        AllowAtRoot = currCt.ctAllowAtRoot,
                        Description = currCt.ctDesc,
                        Icon = currCt.ctIcon,
                        IsContainer = currCt.ctIsContainer,
                        NodeId = currCt.ctId,
                        PrimaryKey = currCt.ctPk,
                        Thumbnail = currCt.ctThumb,
                        //map the underlying node dto
                        NodeDto = new NodeDto
                        {
                            CreateDate = currCt.nCreateDate,
                            Level = (short)currCt.nLevel,
                            NodeId = currCt.ctId,
                            NodeObjectType = currCt.nObjectType,
                            ParentId = currCt.nParentId,
                            Path = currCt.nPath,
                            SortOrder = currCt.nSortOrder,
                            Text = currCt.nName,
                            Trashed = currCt.nTrashed,
                            UniqueId = currCt.nUniqueId,
                            UserId = currCt.nUser
                        }
                    },
                    ContentTypeNodeId = currCt.ctId,
                    IsDefault = currDefaultTemplate != -1,
                    TemplateNodeId = currDefaultTemplate != -1 ? currDefaultTemplate : 0,
                };

                //now create the content type object

                var factory = new ContentTypeFactory();
                var contentType = factory.BuildContentTypeEntity(dtDto.ContentTypeDto);

                // NOTE
                // that was done by the factory but makes little sense, moved here, so
                // now we have to reset dirty props again (as the factory does it) and yet,
                // we are not managing allowed templates... the whole thing is weird.
                ((ContentType)contentType).DefaultTemplateId = dtDto.TemplateNodeId;
                contentType.ResetDirtyProperties(false);

                //map the allowed content types
                contentType.AllowedContentTypes = currAllowedContentTypes;

                return contentType;
            }

            internal static void MapGroupsAndProperties(int[] contentTypeIds, Database db, ISqlSyntaxProvider sqlSyntax,
                out IDictionary<int, PropertyTypeCollection> allPropertyTypeCollection,
                out IDictionary<int, PropertyGroupCollection> allPropertyGroupCollection)
            {
                allPropertyGroupCollection = new Dictionary<int, PropertyGroupCollection>();
                allPropertyTypeCollection = new Dictionary<int, PropertyTypeCollection>();

                // query below is not safe + pointless if array is empty
                if (contentTypeIds.Length == 0) return;

                // first part Gets all property groups including property type data even when no property type exists on the group
                // second part Gets all property types including ones that are not on a group
                // therefore the union of the two contains all of the property type and property group information we need
                // NOTE: MySQL requires a SELECT * FROM the inner union in order to be able to sort . lame.

                var sqlBuilder = new StringBuilder(@"SELECT PG.contenttypeNodeId as contentTypeId,
                            PT.ptUniqueId as ptUniqueID, PT.ptId, PT.ptAlias, PT.ptDesc,PT.ptMandatory,PT.ptName,PT.ptSortOrder,PT.ptRegExp,
                            PT.dtId,PT.dtDbType,PT.dtPropEdAlias,
                            PG.id as pgId, PG.uniqueID as pgKey, PG.sortorder as pgSortOrder, PG." + sqlSyntax.GetQuotedColumnName("text") + @" as pgText
                        FROM cmsPropertyTypeGroup as PG
                        LEFT JOIN
                        (
                            SELECT PT.uniqueID as ptUniqueId, PT.id as ptId, PT.Alias as ptAlias, PT." + sqlSyntax.GetQuotedColumnName("Description") + @" as ptDesc,
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
                                PT.uniqueID as ptUniqueID, PT.id as ptId, PT.Alias as ptAlias, PT." + sqlSyntax.GetQuotedColumnName("Description") + @" as ptDesc,
                                PT.mandatory as ptMandatory, PT.Name as ptName, PT.sortOrder as ptSortOrder, PT.validationRegExp as ptRegExp,
                                DT.nodeId as dtId, DT.dbType as dtDbType, DT.propertyEditorAlias as dtPropEdAlias,
                                PG.id as pgId, PG.uniqueID as pgKey, PG.sortorder as pgSortOrder, PG." + sqlSyntax.GetQuotedColumnName("text") + @" as pgText
                        FROM cmsPropertyType as PT
                        INNER JOIN cmsDataType as DT
                        ON PT.dataTypeId = DT.nodeId
                        LEFT JOIN cmsPropertyTypeGroup as PG
                        ON PG.id = PT.propertyTypeGroupId
                        WHERE (PT.contentTypeId in (@contentTypeIds))");

                sqlBuilder.AppendLine(" ORDER BY (pgId)");

                //NOTE: we are going to assume there's not going to be more than 2100 content type ids since that is the max SQL param count!
                // Since there are 2 groups of params, it will be half!
                if (((contentTypeIds.Length / 2) - 1) > 2000)
                    throw new InvalidOperationException("Cannot perform this lookup, too many sql parameters");

                var result = db.Fetch<dynamic>(sqlBuilder.ToString(), new { contentTypeIds = contentTypeIds });

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
                        .Select(x => new { GroupId = x.pgId, SortOrder = x.pgSortOrder, Text = x.pgText, Key = x.pgKey })
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
                                    Key = row.ptUniqueID,
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
                            SortOrder = group.SortOrder,
                            Key = group.Key
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
                            Key = row.ptUniqueID,
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

        protected abstract TEntity PerformGet(Guid id);
        protected abstract TEntity PerformGet(string alias);
        protected abstract IEnumerable<TEntity> PerformGetAll(params Guid[] ids);
        protected abstract bool PerformExists(Guid id);

        /// <summary>
        /// Gets an Entity by alias
        /// </summary>
        /// <param name="alias"></param>
        /// <returns></returns>
        public TEntity Get(string alias)
        {
            return PerformGet(alias);
        }

        /// <summary>
        /// Gets an Entity by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public TEntity Get(Guid id)
        {
            return PerformGet(id);
        }

        /// <summary>
        /// Gets all entities of the spefified type
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        /// <remarks>
        /// Ensure explicit implementation, we don't want to have any accidental calls to this since it is essentially the same signature as the main GetAll when there are no parameters
        /// </remarks>
        IEnumerable<TEntity> IReadRepository<Guid, TEntity>.GetAll(params Guid[] ids)
        {
            return PerformGetAll(ids);
        }

        /// <summary>
        /// Boolean indicating whether an Entity with the specified Id exists
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool Exists(Guid id)
        {
            return PerformExists(id);
        }

        public string GetUniqueAlias(string alias)
        {
            // alias is unique accross ALL content types!
            var aliasColumn = SqlSyntax.GetQuotedColumnName("alias");
            var aliases = Database.Fetch<string>(@"SELECT cmsContentType." + aliasColumn + @" FROM cmsContentType
INNER JOIN umbracoNode ON cmsContentType.nodeId = umbracoNode.id
WHERE cmsContentType." + aliasColumn + @" LIKE @pattern",
                new { pattern = alias + "%", objectType = NodeObjectTypeId });
            var i = 1;
            string test;
            while (aliases.Contains(test = alias + i)) i++;
            return test;
        }
    }
}
