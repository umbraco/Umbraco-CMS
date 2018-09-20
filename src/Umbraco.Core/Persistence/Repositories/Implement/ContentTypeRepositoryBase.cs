using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
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
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Scoping;
using Umbraco.Core.Services;

namespace Umbraco.Core.Persistence.Repositories.Implement
{
    /// <summary>
    /// Represent an abstract Repository for ContentType based repositories
    /// </summary>
    /// <remarks>Exposes shared functionality</remarks>
    /// <typeparam name="TEntity"></typeparam>
    internal abstract class ContentTypeRepositoryBase<TEntity> : NPocoRepositoryBase<int, TEntity>, IReadRepository<Guid, TEntity>
        where TEntity : class, IContentTypeComposition
    {
        protected ContentTypeRepositoryBase(IScopeAccessor scopeAccessor, CacheHelper cache, ILogger logger)
            : base(scopeAccessor, cache, logger)
        { }

        protected abstract bool IsPublishing { get; }

        public IEnumerable<MoveEventInfo<TEntity>> Move(TEntity moving, EntityContainer container)
        {
            var parentId = Constants.System.Root;
            if (container != null)
            {
                // check path
                if ((string.Format(",{0},", container.Path)).IndexOf(string.Format(",{0},", moving.Id), StringComparison.Ordinal) > -1)
                    throw new DataOperationException<MoveOperationStatusType>(MoveOperationStatusType.FailedNotAllowedByPath);

                parentId = container.Id;
            }

            // track moved entities
            var moveInfo = new List<MoveEventInfo<TEntity>>
            {
                new MoveEventInfo<TEntity>(moving, moving.Path, parentId)
            };


            // get the level delta (old pos to new pos)
            var levelDelta = container == null
                ? 1 - moving.Level
                : container.Level + 1 - moving.Level;

            // move to parent (or -1), update path, save
            moving.ParentId = parentId;
            var movingPath = moving.Path + ","; // save before changing
            moving.Path = (container == null ? Constants.System.Root.ToString() : container.Path) + "," + moving.Id;
            moving.Level = container == null ? 1 : container.Level + 1;
            Save(moving);

            //update all descendants, update in order of level
            var descendants = Get(Query<TEntity>().Where(type => type.Path.StartsWith(movingPath)));
            var paths = new Dictionary<int, string>();
            paths[moving.Id] = moving.Path;

            foreach (var descendant in descendants.OrderBy(x => x.Level))
            {
                moveInfo.Add(new MoveEventInfo<TEntity>(descendant, descendant.Path, descendant.ParentId));

                descendant.Path = paths[descendant.Id] = paths[descendant.ParentId] + "," + descendant.Id;
                descendant.Level += levelDelta;

                Save(descendant);
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
            // used by DataTypeDefinitionRepository to remove properties
            // from content types if they have a deleted data type - see
            // notes in DataTypeDefinitionRepository.Delete as it's a bit
            // weird

            var sqlClause = Sql()
                .SelectAll()
                .From<PropertyTypeGroupDto>()
                .RightJoin<PropertyTypeDto>()
                .On<PropertyTypeGroupDto, PropertyTypeDto>(left => left.Id, right => right.PropertyTypeGroupId)
                .InnerJoin<DataTypeDto>()
                .On<PropertyTypeDto, DataTypeDto>(left => left.DataTypeId, right => right.NodeId);

            var translator = new SqlTranslator<PropertyType>(sqlClause, query);
            // fixme v8 are we sorting only for 7.6 relators?
            var sql = translator.Translate()
                .OrderBy<PropertyTypeDto>(x => x.PropertyTypeGroupId);

            return Database
                .FetchOneToMany<PropertyTypeGroupDto>(x => x.PropertyTypeDtos, sql)
                .Select(x => x.ContentTypeNodeId).Distinct();
        }

        protected virtual PropertyType CreatePropertyType(string propertyEditorAlias, ValueStorageType dbType, string propertyTypeAlias)
        {
            return new PropertyType(propertyEditorAlias, dbType, propertyTypeAlias);
        }

        protected void PersistNewBaseContentType(IContentTypeComposition entity)
        {

            var dto = ContentTypeFactory.BuildContentTypeDto(entity);

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
            var o = Database.IsNew<NodeDto>(nodeDto) ? Convert.ToInt32(Database.Insert(nodeDto)) : Database.Update(nodeDto);

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
            
        
            //Insert Tabs
            foreach (var propertyGroup in entity.PropertyGroups)
            {
                var tabDto = PropertyGroupFactory.BuildGroupDto(propertyGroup, nodeDto.NodeId);
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
                if (propertyType.DataTypeId == 0 || propertyType.DataTypeId == default(int))
                {
                    AssignDataTypeFromPropertyEditor(propertyType);
                }
                var propertyTypeDto = PropertyGroupFactory.BuildPropertyTypeDto(tabId, propertyType, nodeDto.NodeId);
                int typePrimaryKey = Convert.ToInt32(Database.Insert(propertyTypeDto));
                propertyType.Id = typePrimaryKey; //Set Id on new PropertyType

                //Update the current PropertyType with correct PropertyEditorAlias and DatabaseType
                var dataTypeDto = Database.FirstOrDefault<DataTypeDto>("WHERE nodeId = @Id", new { Id = propertyTypeDto.DataTypeId });
                propertyType.PropertyEditorAlias = dataTypeDto.EditorAlias;
                propertyType.ValueStorageType = dataTypeDto.DbType.EnumParse<ValueStorageType>(true);
            }
        }

        protected void PersistUpdatedBaseContentType(IContentTypeComposition entity)
        {

            var dto = ContentTypeFactory.BuildContentTypeDto(entity);

            // ensure the alias is not used already
            var exists = Database.ExecuteScalar<int>(@"SELECT COUNT(*) FROM cmsContentType
INNER JOIN umbracoNode ON cmsContentType.nodeId = umbracoNode.id
WHERE cmsContentType." + SqlSyntax.GetQuotedColumnName("alias") + @"= @alias
AND umbracoNode.nodeObjectType = @objectType
AND umbracoNode.id <> @id",
                new { id = dto.NodeId, alias = dto.Alias, objectType = NodeObjectTypeId });
            if (exists > 0)
            {
                throw new DuplicateNameException("An item with the alias " + dto.Alias + " already exists");
            }

            // repository should be write-locked when doing this, so we are safe from race-conds
            // handle (update) the node
            var nodeDto = dto.NodeDto;
            Database.Update(nodeDto);

            // we NEED this: updating, so the .PrimaryKey already exists, but the entity does
            // not carry it and therefore the dto does not have it yet - must get it from db,
            // look up ContentType entry to get PrimaryKey for updating the DTO
            var dtoPk = Database.First<ContentTypeDto>("WHERE nodeId = @Id", new { entity.Id });
            dto.PrimaryKey = dtoPk.PrimaryKey;
            Database.Update(dto);

            // handle (delete then recreate) compositions
            Database.Delete<ContentType2ContentTypeDto>("WHERE childContentTypeId = @Id", new { entity.Id });
            foreach (var composition in entity.ContentTypeComposition)
                Database.Insert(new ContentType2ContentTypeDto { ParentId = composition.Id, ChildId = entity.Id });

            // removing a ContentType from a composition (U4-1690)
            // 1. Find content based on the current ContentType: entity.Id
            // 2. Find all PropertyTypes on the ContentType that was removed - tracked id (key)
            // 3. Remove properties based on property types from the removed content type where the content ids correspond to those found in step one
            var compositionBase = entity as ContentTypeCompositionBase;
            if (compositionBase != null && compositionBase.RemovedContentTypeKeyTracker != null &&
                compositionBase.RemovedContentTypeKeyTracker.Any())
            {
                // find Content based on the current ContentType
                var sql = Sql()
                    .SelectAll()
                    .From<ContentDto>()
                    .InnerJoin<NodeDto>().On<ContentDto, NodeDto>(left => left.NodeId, right => right.NodeId)
                    .Where<NodeDto>(x => x.NodeObjectType == Constants.ObjectTypes.Document)
                    .Where<ContentDto>(x => x.ContentTypeId == entity.Id);
                var contentDtos = Database.Fetch<ContentDto>(sql);

                // loop through all tracked keys, which corresponds to the ContentTypes that has been removed from the composition
                foreach (var key in compositionBase.RemovedContentTypeKeyTracker)
                {
                    // find PropertyTypes for the removed ContentType
                    var propertyTypes = Database.Fetch<PropertyTypeDto>("WHERE contentTypeId = @Id", new { Id = key });
                    // loop through the Content that is based on the current ContentType in order to remove the Properties that are
                    // based on the PropertyTypes that belong to the removed ContentType.
                    foreach (var contentDto in contentDtos)
                    {
                        foreach (var propertyType in propertyTypes)
                        {
                            var nodeId = contentDto.NodeId;
                            var propertyTypeId = propertyType.Id;
                            var propertySql = Sql()
                                .Select<PropertyDataDto>(x => x.Id)
                                .From<PropertyDataDto>()
                                .InnerJoin<PropertyTypeDto>().On<PropertyDataDto, PropertyTypeDto>((left, right) => left.PropertyTypeId == right.Id)
                                .InnerJoin<ContentVersionDto>().On<PropertyDataDto, ContentVersionDto>((left, right) => left.VersionId == right.Id)
                                .Where<ContentVersionDto>(x => x.NodeId == nodeId)
                                .Where<PropertyTypeDto>(x => x.Id == propertyTypeId);

                            // finally delete the properties that match our criteria for removing a ContentType from the composition
                            Database.Delete<PropertyDataDto>(new Sql("WHERE id IN (" + propertySql.SQL + ")", propertySql.Arguments));
                        }
                    }
                }
            }

            // delete the allowed content type entries before re-inserting the collectino of allowed content types
            Database.Delete<ContentTypeAllowedContentTypeDto>("WHERE Id = @Id", new { entity.Id });
            foreach (var allowedContentType in entity.AllowedContentTypes)
            {
                Database.Insert(new ContentTypeAllowedContentTypeDto
                {
                    Id = entity.Id,
                    AllowedId = allowedContentType.Id.Value,
                    SortOrder = allowedContentType.SortOrder
                });
            }

            // fixme below, manage the property type

            // delete ??? fixme wtf is this?
            // ... by excepting entries from db with entries from collections
            if (entity.IsPropertyDirty("PropertyTypes") || entity.PropertyTypes.Any(x => x.IsDirty()))
            {
                var dbPropertyTypes = Database.Fetch<PropertyTypeDto>("WHERE contentTypeId = @Id", new { entity.Id });
                var dbPropertyTypeAlias = dbPropertyTypes.Select(x => x.Id);
                var entityPropertyTypes = entity.PropertyTypes.Where(x => x.HasIdentity).Select(x => x.Id);
                var items = dbPropertyTypeAlias.Except(entityPropertyTypes);
                foreach (var item in items)
                    DeletePropertyType(entity.Id, item);
            }

            // delete tabs
            // ... by excepting entries from db with entries from collections
            List<int> orphanPropertyTypeIds = null;
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
                // get the tabs that are currently existing (in the db), get the tabs that we want,
                // now, and derive the tabs that we want to delete
                var existingPropertyGroups = Database.Fetch<PropertyTypeGroupDto>("WHERE contentTypeNodeId = @id", new { id = entity.Id })
                    .Select(x => x.Id)
                    .ToList();
                var newPropertyGroups = entity.PropertyGroups.Select(x => x.Id).ToList();
                var groupsToDelete = existingPropertyGroups
                    .Except(newPropertyGroups)
                    .ToArray();

                // delete the tabs
                if (groupsToDelete.Length > 0)
                {
                    // if the tab contains properties, take care of them
                    // - move them to 'generic properties' so they remain consistent
                    // - keep track of them, later on we'll figure out what to do with them
                    // see http://issues.umbraco.org/issue/U4-8663
                    orphanPropertyTypeIds = Database.Fetch<PropertyTypeDto>("WHERE propertyTypeGroupId IN (@ids)", new { ids = groupsToDelete })
                        .Select(x => x.Id).ToList();
                    Database.Update<PropertyTypeDto>("SET propertyTypeGroupId=NULL WHERE propertyTypeGroupId IN (@ids)", new { ids = groupsToDelete });

                    // now we can delete the tabs
                    Database.Delete<PropertyTypeGroupDto>("WHERE id IN (@ids)", new { ids = groupsToDelete });
                }
            }

            // insert or update groups, assign properties
            foreach (var propertyGroup in entity.PropertyGroups)
            {
                // insert or update group
                var groupDto = PropertyGroupFactory.BuildGroupDto(propertyGroup,entity.Id);
                var groupId = propertyGroup.HasIdentity
                    ? Database.Update(groupDto)
                    : Convert.ToInt32(Database.Insert(groupDto));
                if (propertyGroup.HasIdentity == false)
                    propertyGroup.Id = groupId;
                else
                    groupId = propertyGroup.Id;

                // assign properties to the group
                // (all of them, even those that have .IsPropertyDirty("PropertyGroupId") == true,
                //  because it should have been set to this group anyways and better be safe)
                foreach (var propertyType in propertyGroup.PropertyTypes)
                    propertyType.PropertyGroupId = new Lazy<int>(() => groupId);
            }

            // insert or update properties
            // all of them, no-group and in-groups
            foreach (var propertyType in entity.PropertyTypes)
            {
                var groupId = propertyType.PropertyGroupId?.Value ?? default(int);
                // if the Id of the DataType is not set, we resolve it from the db by its PropertyEditorAlias
                if (propertyType.DataTypeId == 0 || propertyType.DataTypeId == default(int))
                    AssignDataTypeFromPropertyEditor(propertyType);

                // validate the alias
                ValidateAlias(propertyType);

                // insert or update property
                var propertyTypeDto = PropertyGroupFactory.BuildPropertyTypeDto(groupId, propertyType, entity.Id);
                var typeId = propertyType.HasIdentity
                    ? Database.Update(propertyTypeDto)
                    : Convert.ToInt32(Database.Insert(propertyTypeDto));
                if (propertyType.HasIdentity == false)
                    propertyType.Id = typeId;
                else
                    typeId = propertyType.Id;

                // not an orphan anymore
                if (orphanPropertyTypeIds != null)
                    orphanPropertyTypeIds.Remove(typeId);
            }

            // deal with orphan properties: those that were in a deleted tab,
            // and have not been re-mapped to another tab or to 'generic properties'
            if (orphanPropertyTypeIds != null)
                foreach (var id in orphanPropertyTypeIds)
                    DeletePropertyType(entity.Id, id);
        }

        private void DeletePropertyType(int contentTypeId, int propertyTypeId)
        {
            // first clear dependencies
            Database.Delete<TagRelationshipDto>("WHERE propertyTypeId = @Id", new { Id = propertyTypeId });
            Database.Delete<PropertyDataDto>("WHERE propertytypeid = @Id", new { Id = propertyTypeId });

            // then delete the property type
            Database.Delete<PropertyTypeDto>("WHERE contentTypeId = @Id AND id = @PropertyTypeId",
                new { Id = contentTypeId, PropertyTypeId = propertyTypeId });
        }

        protected IEnumerable<ContentTypeSort> GetAllowedContentTypeIds(int id)
        {
            var sql = Sql()
                .SelectAll()
                .From<ContentTypeAllowedContentTypeDto>()
                .LeftJoin<ContentTypeDto>()
                .On<ContentTypeAllowedContentTypeDto, ContentTypeDto>(left => left.AllowedId, right => right.NodeId)
                .Where<ContentTypeAllowedContentTypeDto>(x => x.Id == id);

            var allowedContentTypeDtos = Database.Fetch<ContentTypeAllowedContentTypeDto>(sql);
            return allowedContentTypeDtos.Select(x => new ContentTypeSort(new Lazy<int>(() => x.AllowedId), x.SortOrder, x.ContentTypeDto.Alias)).ToList();
        }

        protected PropertyGroupCollection GetPropertyGroupCollection(int id, DateTime createDate, DateTime updateDate)
        {
            var sql = Sql()
                .SelectAll()
                .From<PropertyTypeGroupDto>()
                .LeftJoin<PropertyTypeDto>()
                .On<PropertyTypeGroupDto, PropertyTypeDto>(left => left.Id, right => right.PropertyTypeGroupId)
                .LeftJoin<DataTypeDto>()
                .On<PropertyTypeDto, DataTypeDto>(left => left.DataTypeId, right => right.NodeId)
                .Where<PropertyTypeGroupDto>(x => x.ContentTypeNodeId == id)
                .OrderBy<PropertyTypeGroupDto>(x => x.Id);


            var dtos = Database
                .Fetch<PropertyTypeGroupDto>(sql);

            var propertyGroups = PropertyGroupFactory.BuildEntity(dtos, IsPublishing, id, createDate, updateDate,CreatePropertyType);

            return new PropertyGroupCollection(propertyGroups);
        }

        protected PropertyTypeCollection GetPropertyTypeCollection(int id, DateTime createDate, DateTime updateDate)
        {
            var sql = Sql()
                .SelectAll()
                .From<PropertyTypeDto>()
                .InnerJoin<DataTypeDto>()
                .On<PropertyTypeDto, DataTypeDto>(left => left.DataTypeId, right => right.NodeId)
                .Where<PropertyTypeDto>(x => x.ContentTypeId == id);

            var dtos = Database.Fetch<PropertyTypeDto>(sql);

            //TODO Move this to a PropertyTypeFactory
            var list = new List<PropertyType>();
            foreach (var dto in dtos.Where(x => x.PropertyTypeGroupId <= 0))
            {
                var propType = CreatePropertyType(dto.DataTypeDto.EditorAlias, dto.DataTypeDto.DbType.EnumParse<ValueStorageType>(true), dto.Alias);
                propType.DataTypeId = dto.DataTypeId;
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

            return new PropertyTypeCollection(IsPublishing, list);
        }

        protected void ValidateAlias(PropertyType pt)
        {
            if (string.IsNullOrWhiteSpace(pt.Alias))
            {
                var ex = new InvalidOperationException($"Property Type '{pt.Name}' cannot have an empty Alias. This is most likely due to invalid characters stripped from the Alias.");

                Logger.Error<ContentTypeRepositoryBase<TEntity>>("Property Type '{PropertyTypeName}' cannot have an empty Alias. This is most likely due to invalid characters stripped from the Alias.",
                    pt.Name);

                throw ex;
            }
        }

        protected void ValidateAlias(TEntity entity)
        {
            if (string.IsNullOrWhiteSpace(entity.Alias))
            {
                var ex = new InvalidOperationException($"{typeof(TEntity).Name} '{entity.Name}' cannot have an empty Alias. This is most likely due to invalid characters stripped from the Alias.");

                Logger.Error<ContentTypeRepositoryBase<TEntity>>("{EntityTypeName} '{EntityName}' cannot have an empty Alias. This is most likely due to invalid characters stripped from the Alias.",
                    typeof(TEntity).Name,
                    entity.Name);

                throw ex;
            }
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
                var sql = Sql()
                    .SelectAll()
                    .From<DataTypeDto>()
                    .Where("propertyEditorAlias = @propertyEditorAlias", new { propertyEditorAlias = propertyType.PropertyEditorAlias })
                    .OrderBy<DataTypeDto>(typeDto => typeDto.NodeId);
                var datatype = Database.FirstOrDefault<DataTypeDto>(sql);
                //we cannot assign a data type if one was not found
                if (datatype != null)
                {
                    propertyType.DataTypeId = datatype.NodeId;
                }
                else
                {
                    Logger.Warn<ContentTypeRepositoryBase<TEntity>>("Could not assign a data type for the property type {PropertyTypeAlias} since no data type was found with a property editor {PropertyEditorAlias}", propertyType.Alias, propertyType.PropertyEditorAlias);
                }
            }
        }

        public IEnumerable<TEntity> GetTypesDirectlyComposedOf(int id)
        {
            var sql = Sql()
                .SelectAll()
                .From<NodeDto>()
                .InnerJoin<ContentType2ContentTypeDto>()
                .On<NodeDto, ContentType2ContentTypeDto>(left => left.NodeId, right => right.ChildId)
                .Where<NodeDto>(x => x.NodeObjectType == NodeObjectTypeId)
                .Where<ContentType2ContentTypeDto>(x => x.ParentId == id);
            var dtos = Database.Fetch<NodeDto>(sql);
            return dtos.Any()
                ? GetMany(dtos.DistinctBy(x => x.NodeId).Select(x => x.NodeId).ToArray())
                : Enumerable.Empty<TEntity>();
        }

        internal static class ContentTypeQueryMapper
        {
            public class AssociatedTemplate
            {
                public AssociatedTemplate(int templateId, string alias, string templateName)
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
                IDatabase db, ISqlSyntaxProvider sqlSyntax, bool isPublishing,
                TRepo contentTypeRepository)
                where TRepo : IReadRepository<int, TEntity>
            {
                IDictionary<int, List<int>> allParentMediaTypeIds;
                var mediaTypes = MapMediaTypes(db, sqlSyntax, out allParentMediaTypeIds)
                    .ToArray();

                MapContentTypeChildren(mediaTypes, db, sqlSyntax, isPublishing, contentTypeRepository, allParentMediaTypeIds);

                return mediaTypes;
            }

            public static IEnumerable<IContentType> GetContentTypes<TRepo>(
                IDatabase db, ISqlSyntaxProvider sqlSyntax, bool isPublishing,
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

                    MapContentTypeChildren(contentTypes, db, sqlSyntax, isPublishing, contentTypeRepository, allParentContentTypeIds);
                }

                return contentTypes;
            }

            internal static void MapContentTypeChildren<TRepo>(IContentTypeComposition[] contentTypes,
                IDatabase db, ISqlSyntaxProvider sqlSyntax, bool isPublishing,
                TRepo contentTypeRepository,
                IDictionary<int, List<int>> allParentContentTypeIds)
                where TRepo : IReadRepository<int, TEntity>
            {
                //NOTE: SQL call #2

                var ids = contentTypes.Select(x => x.Id).ToArray();
                IDictionary<int, PropertyGroupCollection> allPropGroups;
                IDictionary<int, PropertyTypeCollection> allPropTypes;
                MapGroupsAndProperties(ids, db, sqlSyntax, isPublishing, out allPropTypes, out allPropGroups);

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

                            // reset dirty initial properties (U4-1946)
                            ((EntityBase)contentType).ResetDirtyProperties(false);
                        }
                    }
                }


            }

            internal static void MapContentTypeTemplates<TRepo>(IContentType[] contentTypes,
                IDatabase db,
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
                    ? templateRepository.GetMany(templateIds)
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

            internal static IEnumerable<IMediaType> MapMediaTypes(IDatabase db, ISqlSyntaxProvider sqlSyntax,
                out IDictionary<int, List<int>> parentMediaTypeIds)
            {
                if (db == null) throw new ArgumentNullException(nameof(db));

                var sql = @"SELECT cmsContentType.pk as ctPk, cmsContentType.alias as ctAlias, cmsContentType.allowAtRoot as ctAllowAtRoot, cmsContentType.description as ctDesc, cmsContentType.variations as ctVariations,
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

                var result = db.Fetch<dynamic>(sql, new { nodeObjectType = Constants.ObjectTypes.MediaType });

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
                    Variations = (byte) currCt.ctVariations,
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

                //now create the content type object;
                var mediaType = ContentTypeFactory.BuildMediaTypeEntity(contentTypeDto);

                //map the allowed content types
                mediaType.AllowedContentTypes = currAllowedContentTypes;

                return mediaType;
            }

            internal static IEnumerable<IContentType> MapContentTypes(IDatabase db, ISqlSyntaxProvider sqlSyntax,
                out IDictionary<int, List<AssociatedTemplate>> associatedTemplates,
                out IDictionary<int, List<int>> parentContentTypeIds)
            {
                if (db == null) throw new ArgumentNullException(nameof(db));

                var sql = @"SELECT cmsDocumentType.IsDefault as dtIsDefault, cmsDocumentType.templateNodeId as dtTemplateId,
                                cmsContentType.pk as ctPk, cmsContentType.alias as ctAlias, cmsContentType.allowAtRoot as ctAllowAtRoot, cmsContentType.description as ctDesc, cmsContentType.variations as ctVariations,
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

                var result = db.Fetch<dynamic>(sql, new { nodeObjectType = Constants.ObjectTypes.DocumentType });

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
                        Variations = (byte) currCt.ctVariations,
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
                var contentType = ContentTypeFactory.BuildContentTypeEntity(dtDto.ContentTypeDto);

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

            internal static void MapGroupsAndProperties(int[] contentTypeIds, IDatabase db, ISqlSyntaxProvider sqlSyntax, bool isPublishing,
                out IDictionary<int, PropertyTypeCollection> allPropertyTypeCollection,
                out IDictionary<int, PropertyGroupCollection> allPropertyGroupCollection)
            {
                allPropertyGroupCollection = new Dictionary<int, PropertyGroupCollection>();
                allPropertyTypeCollection = new Dictionary<int, PropertyTypeCollection>();

                // query below is not safe + pointless if array is empty
                if (contentTypeIds.Length == 0) return;

                var sqlGroups = @"SELECT
    pg.contenttypeNodeId AS contentTypeId,
    pg.id AS id, pg.uniqueID AS " + sqlSyntax.GetQuotedColumnName("key") + @",
    pg.sortOrder AS sortOrder, pg." + sqlSyntax.GetQuotedColumnName("text") + @" AS text
FROM cmsPropertyTypeGroup pg
WHERE pg.contenttypeNodeId IN (@ids)
ORDER BY contentTypeId, id";

                var sqlProps = @"SELECT
    pt.contentTypeId AS contentTypeId,
    pt.id AS id, pt.uniqueID AS " + sqlSyntax.GetQuotedColumnName("key") + @",
    pt.propertyTypeGroupId AS groupId,
    pt.Alias AS alias, pt." + sqlSyntax.GetQuotedColumnName("Description") + @" AS " + sqlSyntax.GetQuotedColumnName("desc") + $@", pt.mandatory AS mandatory,
    pt.Name AS name, pt.sortOrder AS sortOrder, pt.validationRegExp AS regexp, pt.variations as variations,
    dt.nodeId as dataTypeId, dt.dbType as dbType, dt.propertyEditorAlias as editorAlias
FROM cmsPropertyType pt
INNER JOIN {Constants.DatabaseSchema.Tables.DataType} as dt ON pt.dataTypeId = dt.nodeId
WHERE pt.contentTypeId IN (@ids)
ORDER BY contentTypeId, groupId, id";

                if (contentTypeIds.Length > 2000)
                    throw new InvalidOperationException("Cannot perform this lookup, too many sql parameters");

                var groups = db.Fetch<dynamic>(sqlGroups, new { ids = contentTypeIds });
                var groupsEnumerator = groups.GetEnumerator();
                var group = groupsEnumerator.MoveNext() ? groupsEnumerator.Current : null;

                var props = db.Fetch<dynamic>(sqlProps, new { ids = contentTypeIds });
                var propsEnumerator = props.GetEnumerator();
                var prop = propsEnumerator.MoveNext() ? propsEnumerator.Current : null;

                // groups are ordered by content type, group id
                // props are ordered by content type, group id, prop id

                foreach (var contentTypeId in contentTypeIds)
                {
                    var propertyTypeCollection = allPropertyTypeCollection[contentTypeId] = new PropertyTypeCollection(isPublishing);
                    var propertyGroupCollection = allPropertyGroupCollection[contentTypeId] = new PropertyGroupCollection();

                    while (prop != null && prop.contentTypeId == contentTypeId && prop.groupId == null)
                    {
                        AddPropertyType(propertyTypeCollection, prop);
                        prop = propsEnumerator.MoveNext() ? propsEnumerator.Current : null;
                    }

                    while (group != null && group.contentTypeId == contentTypeId)
                    {
                        var propertyGroup = new PropertyGroup(new PropertyTypeCollection(isPublishing))
                        {
                            Id = group.id,
                            Name = group.text,
                            SortOrder = group.sortOrder,
                            Key = group.key
                        };
                        propertyGroupCollection.Add(propertyGroup);

                        while (prop != null && prop.groupId == group.id)
                        {
                            AddPropertyType(propertyGroup.PropertyTypes, prop, propertyGroup);
                            prop = propsEnumerator.MoveNext() ? propsEnumerator.Current : null;
                        }

                        group = groupsEnumerator.MoveNext() ? groupsEnumerator.Current : null;
                    }
                }

                propsEnumerator.Dispose();
                groupsEnumerator.Dispose();
            }

            private static void AddPropertyType(PropertyTypeCollection propertyTypes, dynamic prop, PropertyGroup propertyGroup = null)
            {
                var propertyType = new PropertyType(prop.editorAlias, Enum<ValueStorageType>.Parse(prop.dbType), prop.alias)
                {
                    Description = prop.desc,
                    DataTypeId = prop.dataTypeId,
                    Id = prop.id,
                    Key = prop.key,
                    Mandatory = Convert.ToBoolean(prop.mandatory),
                    Name = prop.name,
                    PropertyGroupId = propertyGroup == null ? null : new Lazy<int>(() => propertyGroup.Id),
                    SortOrder = prop.sortOrder,
                    ValidationRegExp = prop.regexp,
                    Variations = (ContentVariation) prop.variations
                };
                propertyTypes.Add(propertyType);
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
        IEnumerable<TEntity> IReadRepository<Guid, TEntity>.GetMany(params Guid[] ids)
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

        /// <summary>
        /// Given the path of a content item, this will return true if the content item exists underneath a list view content item
        /// </summary>
        /// <param name="contentPath"></param>
        /// <returns></returns>
        public bool HasContainerInPath(string contentPath)
        {
            var ids = contentPath.Split(',').Select(int.Parse);
            var sql = new Sql($@"SELECT COUNT(*) FROM cmsContentType
INNER JOIN {Constants.DatabaseSchema.Tables.Content} ON cmsContentType.nodeId={Constants.DatabaseSchema.Tables.Content}.contentTypeId
WHERE {Constants.DatabaseSchema.Tables.Content}.nodeId IN (@ids) AND cmsContentType.isContainer=@isContainer", new { ids, isContainer = true });
            return Database.ExecuteScalar<int>(sql) > 0;
        }

        protected override IEnumerable<string> GetDeleteClauses()
        {
            // in theory, services should have ensured that content items of the given content type
            // have been deleted and therefore PropertyData has been cleared, so PropertyData
            // is included here just to be 100% sure since it has a FK on cmsPropertyType.

            var list = new List<string>
            {
                "DELETE FROM umbracoUser2NodeNotify WHERE nodeId = @id",
                "DELETE FROM umbracoUserGroup2NodePermission WHERE nodeId = @id",
                "DELETE FROM cmsTagRelationship WHERE nodeId = @id",
                "DELETE FROM cmsContentTypeAllowedContentType WHERE Id = @id",
                "DELETE FROM cmsContentTypeAllowedContentType WHERE AllowedId = @id",
                "DELETE FROM cmsContentType2ContentType WHERE parentContentTypeId = @id",
                "DELETE FROM cmsContentType2ContentType WHERE childContentTypeId = @id",
                "DELETE FROM " + Constants.DatabaseSchema.Tables.PropertyData + " WHERE propertyTypeId IN (SELECT id FROM cmsPropertyType WHERE contentTypeId = @id)",
                "DELETE FROM cmsPropertyType WHERE contentTypeId = @id",
                "DELETE FROM cmsPropertyTypeGroup WHERE contenttypeNodeId = @id",
            };
            return list;
        }
    }
}
