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
            if (entity is ContentTypeCompositionBase compositionBase &&
                compositionBase.RemovedContentTypeKeyTracker != null &&
                compositionBase.RemovedContentTypeKeyTracker.Any())
            {
                //TODO: Could we do the below with bulk SQL statements instead of looking everything up and then manipulating?

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
                        //TODO: This could be done with bulk SQL statements
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

            // delete the allowed content type entries before re-inserting the collection of allowed content types
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

            // Delete property types ... by excepting entries from db with entries from collections.
            // We check if the entity's own PropertyTypes has been modified and then also check
            // any of the property groups PropertyTypes has been modified.
            // This specifically tells us if any property type collections have changed.
            if (entity.IsPropertyDirty("PropertyTypes") || entity.PropertyGroups.Any(x => x.IsPropertyDirty("PropertyTypes")))
            {
                var dbPropertyTypes = Database.Fetch<PropertyTypeDto>("WHERE contentTypeId = @Id", new { entity.Id });
                var dbPropertyTypeAlias = dbPropertyTypes.Select(x => x.Id);
                var entityPropertyTypes = entity.PropertyTypes.Where(x => x.HasIdentity).Select(x => x.Id);
                var items = dbPropertyTypeAlias.Except(entityPropertyTypes);
                foreach (var item in items)
                    DeletePropertyType(entity.Id, item);
            }

            // Delete tabs ... by excepting entries from db with entries from collections.
            // We check if the entity's own PropertyGroups has been modified.
            // This specifically tells us if the property group collections have changed.
            List<int> orphanPropertyTypeIds = null;
            if (entity.IsPropertyDirty("PropertyGroups"))
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

            //check if the content type variation has been changed
            var contentTypeVariationDirty = entity.IsPropertyDirty("Variations");
            var oldContentTypeVariation = (ContentVariation) dtoPk.Variations;
            var newContentTypeVariation = entity.Variations;
            var contentTypeVariationChanging = contentTypeVariationDirty && oldContentTypeVariation != newContentTypeVariation;
            if (contentTypeVariationChanging)
            {
                MoveContentTypeVariantData(entity, oldContentTypeVariation, newContentTypeVariation);
                Clear301Redirects(entity);
                ClearScheduledPublishing(entity);
            }

            // collect property types that have a dirty variation
            List<PropertyType> propertyTypeVariationDirty = null;

            // note: this only deals with *local* property types, we're dealing w/compositions later below
            foreach (var propertyType in entity.PropertyTypes)
            {
                if (contentTypeVariationChanging)
                {
                    // content type is changing
                    switch (newContentTypeVariation)
                    {
                        case ContentVariation.Nothing: // changing to Nothing
                            // all property types must change to Nothing
                            propertyType.Variations = ContentVariation.Nothing;
                            break;
                        case ContentVariation.Culture: // changing to Culture
                            // all property types can remain Nothing
                            break;
                        case ContentVariation.CultureAndSegment:
                        case ContentVariation.Segment:
                        default:
                            throw new NotSupportedException(); //TODO: Support this
                    }
                }

                // then, track each property individually
                if (propertyType.IsPropertyDirty("Variations"))
                {
                    // allocate the list only when needed
                    if (propertyTypeVariationDirty == null)
                        propertyTypeVariationDirty = new List<PropertyType>();

                    propertyTypeVariationDirty.Add(propertyType);
                }
            }

            // figure out dirty property types that have actually changed
            // before we insert or update properties, so we can read the old variations
            var propertyTypeVariationChanges = propertyTypeVariationDirty != null
                ? GetPropertyVariationChanges(propertyTypeVariationDirty)
                : null;

            // deal with composition property types
            // add changes for property types obtained via composition, which change due
            // to this content type variations change
            if (contentTypeVariationChanging)
            {
                // must use RawComposedPropertyTypes here: only those types that are obtained
                // via composition, with their original variations (ie not filtered by this
                // content type variations - we need this true value to make decisions.

                foreach (var propertyType in ((ContentTypeCompositionBase) entity).RawComposedPropertyTypes)
                {
                    if (propertyType.VariesBySegment() || newContentTypeVariation.VariesBySegment())
                        throw new NotSupportedException(); // TODO: support this

                    if (propertyType.Variations == ContentVariation.Culture)
                    {
                        if (propertyTypeVariationChanges == null)
                            propertyTypeVariationChanges = new Dictionary<int, (ContentVariation, ContentVariation)>();

                        // if content type moves to Culture, property type becomes Culture here again
                        // if content type moves to Nothing, property type becomes Nothing here
                        if (newContentTypeVariation == ContentVariation.Culture)
                            propertyTypeVariationChanges[propertyType.Id] = (ContentVariation.Nothing, ContentVariation.Culture);
                        else if (newContentTypeVariation == ContentVariation.Nothing)
                            propertyTypeVariationChanges[propertyType.Id] = (ContentVariation.Culture, ContentVariation.Nothing);
                    }
                }
            }

            // insert or update properties
            // all of them, no-group and in-groups
            foreach (var propertyType in entity.PropertyTypes)
            {
                // if the Id of the DataType is not set, we resolve it from the db by its PropertyEditorAlias
                if (propertyType.DataTypeId == 0 || propertyType.DataTypeId == default)
                    AssignDataTypeFromPropertyEditor(propertyType);

                // validate the alias
                ValidateAlias(propertyType);

                // insert or update property
                var groupId = propertyType.PropertyGroupId?.Value ?? default;
                var propertyTypeDto = PropertyGroupFactory.BuildPropertyTypeDto(groupId, propertyType, entity.Id);
                var typeId = propertyType.HasIdentity
                    ? Database.Update(propertyTypeDto)
                    : Convert.ToInt32(Database.Insert(propertyTypeDto));
                if (propertyType.HasIdentity == false)
                    propertyType.Id = typeId;
                else
                    typeId = propertyType.Id;

                // not an orphan anymore
                orphanPropertyTypeIds?.Remove(typeId);
            }

            // must restrict property data changes to impacted content types - if changing a composing
            // type, some composed types (those that do not vary) are not impacted and should be left
            // unchanged
            //
            // getting 'all' from the cache policy is prone to race conditions - fast but dangerous
            //var all = ((FullDataSetRepositoryCachePolicy<TEntity, int>)CachePolicy).GetAllCached(PerformGetAll);
            var all = PerformGetAll();

            var impacted = GetImpactedContentTypes(entity, all);

            // if some property types have actually changed, move their variant data
            if (propertyTypeVariationChanges != null)
                MovePropertyTypeVariantData(propertyTypeVariationChanges, impacted);

            // deal with orphan properties: those that were in a deleted tab,
            // and have not been re-mapped to another tab or to 'generic properties'
            if (orphanPropertyTypeIds != null)
                foreach (var id in orphanPropertyTypeIds)
                    DeletePropertyType(entity.Id, id);
        }

        private IEnumerable<IContentTypeComposition> GetImpactedContentTypes(IContentTypeComposition contentType, IEnumerable<IContentTypeComposition> all)
        {
            var impact = new List<IContentTypeComposition>();
            var set = new List<IContentTypeComposition> { contentType };

            var tree = new Dictionary<int, List<IContentTypeComposition>>();
            foreach (var x in all)
            foreach (var y in x.ContentTypeComposition)
            {
                if (!tree.TryGetValue(y.Id, out var list))
                    list = tree[y.Id] = new List<IContentTypeComposition>();
                list.Add(x);
            }

            var nset = new List<IContentTypeComposition>();
            do
            {
                impact.AddRange(set);

                foreach (var x in set)
                {
                    if (!tree.TryGetValue(x.Id, out var list)) continue;
                    nset.AddRange(list.Where(y => y.VariesByCulture()));
                }

                set = nset;
                nset = new List<IContentTypeComposition>();
            } while (set.Count > 0);

            return impact;
        }

        // gets property types that have actually changed, and the corresponding changes
        // returns null if no property type has actually changed
        private Dictionary<int, (ContentVariation FromVariation, ContentVariation ToVariation)> GetPropertyVariationChanges(IEnumerable<PropertyType> propertyTypes)
        {
            var propertyTypesL = propertyTypes.ToList();

            // select the current variations (before the change) from database
            var selectCurrentVariations = Sql()
                .Select<PropertyTypeDto>(x => x.Id, x => x.Variations)
                .From<PropertyTypeDto>()
                .WhereIn<PropertyTypeDto>(x => x.Id, propertyTypesL.Select(x => x.Id));

            var oldVariations = Database.Dictionary<int, byte>(selectCurrentVariations);

            // build a dictionary of actual changes
            Dictionary<int, (ContentVariation, ContentVariation)> changes = null;

            foreach (var propertyType in propertyTypesL)
            {
                // new property type, ignore
                if (!oldVariations.TryGetValue(propertyType.Id, out var oldVariationB))
                    continue;
                var oldVariation = (ContentVariation) oldVariationB; // NPoco cannot fetch directly

                // only those property types that *actually* changed
                var newVariation = propertyType.Variations;
                if (oldVariation == newVariation)
                    continue;

                // allocate the dictionary only when needed
                if (changes == null)
                    changes = new Dictionary<int, (ContentVariation, ContentVariation)>();

                changes[propertyType.Id] = (oldVariation, newVariation);
            }

            return changes;
        }

        /// <summary>
        /// Clear any redirects associated with content for a content type
        /// </summary>
        private void Clear301Redirects(IContentTypeComposition contentType)
        {
            //first clear out any existing property data that might already exists under the default lang
            var sqlSelect = Sql().Select<NodeDto>(x => x.UniqueId)
                .From<NodeDto>()
                .InnerJoin<ContentDto>().On<ContentDto, NodeDto>(x => x.NodeId, x => x.NodeId)
                .Where<ContentDto>(x => x.ContentTypeId == contentType.Id);
            var sqlDelete = Sql()
                .Delete<RedirectUrlDto>()
                .WhereIn((System.Linq.Expressions.Expression<Func<RedirectUrlDto, object>>)(x => x.ContentKey), sqlSelect);

            Database.Execute(sqlDelete);
        }

        /// <summary>
        /// Clear any scheduled publishing associated with content for a content type
        /// </summary>
        private void ClearScheduledPublishing(IContentTypeComposition contentType)
        {
            //TODO: Fill this in when scheduled publishing is enabled for variants
        }

        /// <summary>
        /// Gets the default language identifier.
        /// </summary>
        private int GetDefaultLanguageId()
        {
            var selectDefaultLanguageId = Sql()
                .Select<LanguageDto>(x => x.Id)
                .From<LanguageDto>()
                .Where<LanguageDto>(x => x.IsDefault);

            return Database.First<int>(selectDefaultLanguageId);
        }

        /// <summary>
        /// Moves variant data for property type variation changes.
        /// </summary>
        private void MovePropertyTypeVariantData(IDictionary<int, (ContentVariation FromVariation, ContentVariation ToVariation)> propertyTypeChanges, IEnumerable<IContentTypeComposition> impacted)
        {
            var defaultLanguageId = GetDefaultLanguageId();
            var impactedL = impacted.Select(x => x.Id).ToList();

            //Group by the "To" variation so we can bulk update in the correct batches
            foreach(var grouping in propertyTypeChanges.GroupBy(x => x.Value.ToVariation))
            {
                var propertyTypeIds = grouping.Select(x => x.Key).ToList();
                var toVariation = grouping.Key;

                switch (toVariation)
                {
                    case ContentVariation.Culture:
                        CopyPropertyData(null, defaultLanguageId, propertyTypeIds, impactedL);
                        CopyTagData(null, defaultLanguageId, propertyTypeIds, impactedL);
                        break;
                    case ContentVariation.Nothing:
                        CopyPropertyData(defaultLanguageId, null, propertyTypeIds, impactedL);
                        CopyTagData(defaultLanguageId, null, propertyTypeIds, impactedL);
                        break;
                    case ContentVariation.CultureAndSegment:
                    case ContentVariation.Segment:
                    default:
                        throw new NotSupportedException(); //TODO: Support this
                }
            }
        }

        /// <summary>
        /// Moves variant data for a content type variation change.
        /// </summary>
        private void MoveContentTypeVariantData(IContentTypeComposition contentType, ContentVariation fromVariation, ContentVariation toVariation)
        {
            var defaultLanguageId = GetDefaultLanguageId();

            switch (toVariation)
            {
                case ContentVariation.Culture:

                    //move the names
                    //first clear out any existing names that might already exists under the default lang
                    //there's 2x tables to update

                    //clear out the versionCultureVariation table
                    var sqlSelect = Sql().Select<ContentVersionCultureVariationDto>(x => x.Id)
                        .From<ContentVersionCultureVariationDto>()
                        .InnerJoin<ContentVersionDto>().On<ContentVersionDto, ContentVersionCultureVariationDto>(x => x.Id, x => x.VersionId)
                        .InnerJoin<ContentDto>().On<ContentDto, ContentVersionDto>(x => x.NodeId, x => x.NodeId)
                        .Where<ContentDto>(x => x.ContentTypeId == contentType.Id)
                        .Where<ContentVersionCultureVariationDto>(x => x.LanguageId == defaultLanguageId);
                    var sqlDelete = Sql()
                        .Delete<ContentVersionCultureVariationDto>()
                        .WhereIn<ContentVersionCultureVariationDto>(x => x.Id, sqlSelect);

                    Database.Execute(sqlDelete);

                    //clear out the documentCultureVariation table
                    sqlSelect = Sql().Select<DocumentCultureVariationDto>(x => x.Id)
                        .From<DocumentCultureVariationDto>()
                        .InnerJoin<ContentDto>().On<ContentDto, DocumentCultureVariationDto>(x => x.NodeId, x => x.NodeId)
                        .Where<ContentDto>(x => x.ContentTypeId == contentType.Id)
                        .Where<DocumentCultureVariationDto>(x => x.LanguageId == defaultLanguageId);
                    sqlDelete = Sql()
                        .Delete<DocumentCultureVariationDto>()
                        .WhereIn<DocumentCultureVariationDto>(x => x.Id, sqlSelect);

                    Database.Execute(sqlDelete);

                    //now we need to insert names into these 2 tables based on the invariant data

                    //insert rows into the versionCultureVariationDto table based on the data from contentVersionDto for the default lang
                    var cols = Sql().Columns<ContentVersionCultureVariationDto>(x => x.VersionId, x => x.Name, x => x.UpdateUserId, x => x.UpdateDate, x => x.LanguageId);
                    sqlSelect = Sql().Select<ContentVersionDto>(x => x.Id, x => x.Text, x => x.UserId, x => x.VersionDate)
                        .Append($", {defaultLanguageId}") //default language ID
                        .From<ContentVersionDto>()
                        .InnerJoin<ContentDto>().On<ContentDto, ContentVersionDto>(x => x.NodeId, x => x.NodeId)
                        .Where<ContentDto>(x => x.ContentTypeId == contentType.Id);
                    var sqlInsert = Sql($"INSERT INTO {ContentVersionCultureVariationDto.TableName} ({cols})").Append(sqlSelect);

                    Database.Execute(sqlInsert);

                    //insert rows into the documentCultureVariation table
                    cols = Sql().Columns<DocumentCultureVariationDto>(x => x.NodeId, x => x.Edited, x => x.Published, x => x.Name, x => x.Available, x => x.LanguageId);
                    sqlSelect = Sql().Select<DocumentDto>(x => x.NodeId, x => x.Edited, x => x.Published)
                        .AndSelect<NodeDto>(x => x.Text)
                        .Append($", 1, {defaultLanguageId}") //make Available + default language ID
                        .From<DocumentDto>()
                        .InnerJoin<NodeDto>().On<NodeDto, DocumentDto>(x => x.NodeId, x => x.NodeId)
                        .InnerJoin<ContentDto>().On<ContentDto, NodeDto>(x => x.NodeId, x => x.NodeId)
                        .Where<ContentDto>(x => x.ContentTypeId == contentType.Id);
                    sqlInsert = Sql($"INSERT INTO {DocumentCultureVariationDto.TableName} ({cols})").Append(sqlSelect);

                    Database.Execute(sqlInsert);

                    break;
                case ContentVariation.Nothing:

                    //we don't need to move the names! this is because we always keep the invariant names with the name of the default language.

                    //however, if we were to move names, we could do this: BUT this doesn't work with SQLCE, for that we'd have to update row by row :(
                    // if we want these SQL statements back, look into GIT history

                    break;
                case ContentVariation.CultureAndSegment:
                case ContentVariation.Segment:
                default:
                    throw new NotSupportedException(); //TODO: Support this
            }
        }

        ///
        private void CopyTagData(int? sourceLanguageId, int? targetLanguageId, IReadOnlyCollection<int> propertyTypeIds, IReadOnlyCollection<int> contentTypeIds = null)
        {
            // note: important to use SqlNullableEquals for nullable types, cannot directly compare language identifiers

            // fixme - should we batch then?
            var whereInArgsCount = propertyTypeIds.Count + (contentTypeIds?.Count ?? 0);
            if (whereInArgsCount > 2000)
                throw new NotSupportedException("Too many property/content types.");

            // delete existing relations (for target language)
            // do *not* delete existing tags

            var sqlTagToDelete = Sql()
                .Select<TagDto>(x => x.Id)
                .From<TagDto>()
                .InnerJoin<TagRelationshipDto>().On<TagDto, TagRelationshipDto>((tag, rel) => tag.Id == rel.TagId);

            if (contentTypeIds != null)
                sqlTagToDelete
                    .InnerJoin<ContentDto>().On<TagRelationshipDto, ContentDto>((rel, content) => rel.NodeId == content.NodeId)
                    .WhereIn<ContentDto>(x => x.ContentTypeId, contentTypeIds);

            sqlTagToDelete
                .WhereIn<TagRelationshipDto>(x => x.PropertyTypeId, propertyTypeIds)
                .Where<TagDto>(x => x.LanguageId.SqlNullableEquals(targetLanguageId, -1));

            var sqlDeleteRel = Sql()
                .Delete<TagRelationshipDto>()
                .WhereIn<TagRelationshipDto>(x => x.TagId, sqlTagToDelete);

            sqlDeleteRel.WriteToConsole();
            Database.Execute(sqlDeleteRel);

            // do *not* delete the tags - they could be used by other content types / property types
            /*
            var sqlDeleteTag = Sql()
                .Delete<TagDto>()
                .WhereIn<TagDto>(x => x.Id, sqlTagToDelete);
            Database.Execute(sqlDeleteTag);
            */

            // copy tags from source language to target language

            var targetLanguageIdS = targetLanguageId.HasValue ? targetLanguageId.ToString() : "NULL";
            var sqlSelect = Sql()
                .Select<TagDto>(x => x.Text, x => x.Group)
                .Append(", " + targetLanguageIdS)
                .From<TagDto>();

            sqlSelect
                .InnerJoin<TagRelationshipDto>().On<TagDto, TagRelationshipDto>((tag, rel) => tag.Id == rel.TagId)
                .LeftJoin<TagDto>("xtags").On<TagDto, TagDto>((tag, xtag) => tag.Text == xtag.Text && tag.Group == xtag.Group && tag.LanguageId.SqlNullableEquals(targetLanguageId, -1), aliasRight: "xtags");

            if (contentTypeIds != null)
                sqlSelect
                    .InnerJoin<ContentDto>().On<TagRelationshipDto, ContentDto>((rel, content) => rel.NodeId == content.NodeId);

            sqlSelect
                .WhereIn<TagRelationshipDto>(x => x.PropertyTypeId, propertyTypeIds)
                .WhereNull<TagDto>(x => x.Id, "xtags"); // ie, not exists

            if (contentTypeIds != null)
                sqlSelect
                    .WhereIn<ContentDto>(x => x.ContentTypeId, contentTypeIds);

            sqlSelect.Where<TagDto>(x => x.LanguageId.SqlNullableEquals(sourceLanguageId, -1));

            var cols = Sql().Columns<TagDto>(x => x.Text, x => x.Group, x => x.LanguageId);
            var sqlInsertTag = Sql($"INSERT INTO {TagDto.TableName} ({cols})").Append(sqlSelect);

            sqlInsertTag.WriteToConsole();
            Database.Execute(sqlInsertTag);

            // create relations to new tags

            var sqlFoo = Sql()
                .Select<TagRelationshipDto>(x => x.NodeId, x => x.PropertyTypeId)
                .AndSelect<TagDto>("otag", x => x.Id)
                .From<TagRelationshipDto>()
                .InnerJoin<TagDto>().On<TagRelationshipDto, TagDto>((rel, tag) => rel.TagId == tag.Id)
                .InnerJoin<TagDto>("otag").On<TagDto, TagDto>((tag, otag) => tag.Text == otag.Text && tag.Group == otag.Group && otag.LanguageId.SqlNullableEquals(targetLanguageId, -1), aliasRight: "otag")
                .Where<TagDto>(x => x.LanguageId.SqlNullableEquals(sourceLanguageId, -1));

            var cols2 = Sql().Columns<TagRelationshipDto>(x => x.NodeId, x => x.PropertyTypeId, x => x.TagId);
            var sqlInsertRel = Sql($"INSERT INTO {TagRelationshipDto.TableName} ({cols2})").Append(sqlFoo);

            sqlInsertRel.WriteToConsole();
            Database.Execute(sqlInsertRel);

            // delete original relations - *not* the tags - all of them
            // cannot really "go back" with relations, would have to do it with property values

            sqlTagToDelete = Sql()
                .Select<TagDto>(x => x.Id)
                .From<TagDto>()
                .InnerJoin<TagRelationshipDto>().On<TagDto, TagRelationshipDto>((tag, rel) => tag.Id == rel.TagId);

            if (contentTypeIds != null)
                sqlTagToDelete
                    .InnerJoin<ContentDto>().On<TagRelationshipDto, ContentDto>((rel, content) => rel.NodeId == content.NodeId)
                    .WhereIn<ContentDto>(x => x.ContentTypeId, contentTypeIds);

            sqlTagToDelete
                .WhereIn<TagRelationshipDto>(x => x.PropertyTypeId, propertyTypeIds)
                .Where<TagDto>(x => !x.LanguageId.SqlNullableEquals(targetLanguageId, -1));

            sqlDeleteRel = Sql()
                .Delete<TagRelationshipDto>()
                .WhereIn<TagRelationshipDto>(x => x.TagId, sqlTagToDelete);

            sqlDeleteRel.WriteToConsole();
            Database.Execute(sqlDeleteRel);

            // no
            /*
            var sqlDeleteTag = Sql()
                .Delete<TagDto>()
                .WhereIn<TagDto>(x => x.Id, sqlTagToDelete);
            Database.Execute(sqlDeleteTag);
            */
        }

        /// <summary>
        /// Copies property data from one language to another.
        /// </summary>
        /// <param name="sourceLanguageId">The source language (can be null ie invariant).</param>
        /// <param name="targetLanguageId">The target language (can be null ie invariant)</param>
        /// <param name="propertyTypeIds">The property type identifiers.</param>
        /// <param name="contentTypeIds">The content type identifiers.</param>
        private void CopyPropertyData(int? sourceLanguageId, int? targetLanguageId, IReadOnlyCollection<int> propertyTypeIds, IReadOnlyCollection<int> contentTypeIds = null)
        {
            // note: important to use SqlNullableEquals for nullable types, cannot directly compare language identifiers
            //
            // fixme - should we batch then?
            var whereInArgsCount = propertyTypeIds.Count + (contentTypeIds?.Count ?? 0);
            if (whereInArgsCount > 2000)
                throw new NotSupportedException("Too many property/content types.");

            //first clear out any existing property data that might already exists under the target language
            var sqlDelete = Sql()
                .Delete<PropertyDataDto>();

            // not ok for SqlCe (no JOIN in DELETE)
            //if (contentTypeIds != null)
            //    sqlDelete
            //        .From<PropertyDataDto>()
            //        .InnerJoin<ContentVersionDto>().On<PropertyDataDto, ContentVersionDto>((pdata, cversion) => pdata.VersionId == cversion.Id)
            //        .InnerJoin<ContentDto>().On<ContentVersionDto, ContentDto>((cversion, c) => cversion.NodeId == c.NodeId);

            Sql<ISqlContext> inSql = null;
            if (contentTypeIds != null)
            {
                inSql = Sql()
                    .Select<ContentVersionDto>(x => x.Id)
                    .From<ContentVersionDto>()
                    .InnerJoin<ContentDto>().On<ContentVersionDto, ContentDto>((cversion, c) => cversion.NodeId == c.NodeId)
                    .WhereIn<ContentDto>(x => x.ContentTypeId, contentTypeIds);
                sqlDelete.WhereIn<PropertyDataDto>(x => x.VersionId, inSql);
            }

            sqlDelete.Where<PropertyDataDto>(x => x.LanguageId.SqlNullableEquals(targetLanguageId, -1));

            sqlDelete
                .WhereIn<PropertyDataDto>(x => x.PropertyTypeId, propertyTypeIds);

            // see note above, not ok for SqlCe
            //if (contentTypeIds != null)
            //    sqlDelete
            //        .WhereIn<ContentDto>(x => x.ContentTypeId, contentTypeIds);

            Database.Execute(sqlDelete);

            //now insert all property data into the target language that exists under the source language
            var targetLanguageIdS = targetLanguageId.HasValue ? targetLanguageId.ToString() : "NULL";
            var cols = Sql().Columns<PropertyDataDto>(x => x.VersionId, x => x.PropertyTypeId, x => x.Segment, x => x.IntegerValue, x => x.DecimalValue, x => x.DateValue, x => x.VarcharValue, x => x.TextValue, x => x.LanguageId);
            var sqlSelectData = Sql().Select<PropertyDataDto>(x => x.VersionId, x => x.PropertyTypeId, x => x.Segment, x => x.IntegerValue, x => x.DecimalValue, x => x.DateValue, x => x.VarcharValue, x => x.TextValue)
                .Append(", " + targetLanguageIdS) //default language ID
                .From<PropertyDataDto>();

            if (contentTypeIds != null)
                sqlSelectData
                    .InnerJoin<ContentVersionDto>().On<PropertyDataDto, ContentVersionDto>((pdata, cversion) => pdata.VersionId == cversion.Id)
                    .InnerJoin<ContentDto>().On<ContentVersionDto, ContentDto>((cversion, c) => cversion.NodeId == c.NodeId);

            sqlSelectData.Where<PropertyDataDto>(x => x.LanguageId.SqlNullableEquals(sourceLanguageId, -1));

            sqlSelectData
                .WhereIn<PropertyDataDto>(x => x.PropertyTypeId, propertyTypeIds);

            if (contentTypeIds != null)
                sqlSelectData
                    .WhereIn<ContentDto>(x => x.ContentTypeId, contentTypeIds);

            var sqlInsert = Sql($"INSERT INTO {PropertyDataDto.TableName} ({cols})").Append(sqlSelectData);

            Database.Execute(sqlInsert);

            // when copying from Culture, keep the original values around in case we want to go back
            // when copying from Nothing, kill the original values, we don't want them around
            if (sourceLanguageId == null)
            {
                sqlDelete = Sql()
                    .Delete<PropertyDataDto>();

                if (contentTypeIds != null)
                    sqlDelete.WhereIn<PropertyDataDto>(x => x.VersionId, inSql);

                sqlDelete
                    .Where<PropertyDataDto>(x => x.LanguageId == null)
                    .WhereIn<PropertyDataDto>(x => x.PropertyTypeId, propertyTypeIds);

                Database.Execute(sqlDelete);
            }
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
