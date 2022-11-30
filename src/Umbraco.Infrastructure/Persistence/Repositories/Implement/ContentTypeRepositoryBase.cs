using System.Data;
using System.Globalization;
using System.Linq.Expressions;
using Microsoft.Extensions.Logging;
using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Exceptions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Persistence.Factories;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

/// <summary>
///     Represent an abstract Repository for ContentType based repositories
/// </summary>
/// <remarks>Exposes shared functionality</remarks>
/// <typeparam name="TEntity"></typeparam>
internal abstract class ContentTypeRepositoryBase<TEntity> : EntityRepositoryBase<int, TEntity>,
    IReadRepository<Guid, TEntity>
    where TEntity : class, IContentTypeComposition
{
    private readonly IShortStringHelper _shortStringHelper;

    protected ContentTypeRepositoryBase(IScopeAccessor scopeAccessor, AppCaches cache,
        ILogger<ContentTypeRepositoryBase<TEntity>> logger, IContentTypeCommonRepository commonRepository,
        ILanguageRepository languageRepository, IShortStringHelper shortStringHelper)
        : base(scopeAccessor, cache, logger)
    {
        _shortStringHelper = shortStringHelper;
        CommonRepository = commonRepository;
        LanguageRepository = languageRepository;
    }

    protected IContentTypeCommonRepository CommonRepository { get; }

    protected ILanguageRepository LanguageRepository { get; }

    protected abstract bool SupportsPublishing { get; }

    /// <summary>
    ///     Gets the node object type for the repository's entity
    /// </summary>
    protected abstract Guid NodeObjectTypeId { get; }

    /// <summary>
    ///     Gets an Entity by Id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public TEntity? Get(Guid id) => PerformGet(id);

    /// <summary>
    ///     Gets all entities of the specified type
    /// </summary>
    /// <param name="ids"></param>
    /// <returns></returns>
    /// <remarks>
    ///     Ensure explicit implementation, we don't want to have any accidental calls to this since it is essentially the same
    ///     signature as the main GetAll when there are no parameters
    /// </remarks>
    IEnumerable<TEntity> IReadRepository<Guid, TEntity>.GetMany(params Guid[]? ids) =>
        PerformGetAll(ids) ?? Enumerable.Empty<TEntity>();

    /// <summary>
    ///     Boolean indicating whether an Entity with the specified Id exists
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public bool Exists(Guid id) => PerformExists(id);

    public IEnumerable<MoveEventInfo<TEntity>> Move(TEntity moving, EntityContainer? container)
    {
        var parentId = Constants.System.Root;
        if (container != null)
        {
            // check path
            if (string.Format(",{0},", container.Path).IndexOf(
                string.Format(",{0},", moving.Id),
                StringComparison.Ordinal) > -1)
            {
                throw new DataOperationException<MoveOperationStatusType>(MoveOperationStatusType
                    .FailedNotAllowedByPath);
            }

            parentId = container.Id;
        }

        // track moved entities
        var moveInfo = new List<MoveEventInfo<TEntity>> { new(moving, moving.Path, parentId) };

        // get the level delta (old pos to new pos)
        var levelDelta = container == null
            ? 1 - moving.Level
            : container.Level + 1 - moving.Level;

        // move to parent (or -1), update path, save
        moving.ParentId = parentId;
        var movingPath = moving.Path + ","; // save before changing
        moving.Path = (container == null ? Constants.System.RootString : container.Path) + "," + moving.Id;
        moving.Level = container == null ? 1 : container.Level + 1;
        Save(moving);

        // update all descendants, update in order of level
        IEnumerable<TEntity> descendants = Get(Query<TEntity>().Where(type => type.Path.StartsWith(movingPath)));
        var paths = new Dictionary<int, string>
        {
            [moving.Id] = moving.Path,
        };

        foreach (TEntity descendant in descendants.OrderBy(x => x.Level))
        {
            moveInfo.Add(new MoveEventInfo<TEntity>(descendant, descendant.Path, descendant.ParentId));

            descendant.Path = paths[descendant.Id] = paths[descendant.ParentId] + "," + descendant.Id;
            descendant.Level += levelDelta;

            Save(descendant);
        }

        return moveInfo;
    }

    /// <summary>
    ///     Gets an Entity by alias
    /// </summary>
    /// <param name="alias"></param>
    /// <returns></returns>
    public TEntity? Get(string alias) => PerformGet(alias);

    protected override IEnumerable<TEntity> PerformGetAll(params int[]? ids)
    {
        IEnumerable<TEntity>? result = GetAllWithFullCachePolicy();

        // By default the cache policy will always want everything
        // even GetMany(ids) gets everything and filters afterwards,
        // however if we are using No Cache, we must still be able to support
        // collections of Ids, so this is to work around that:
        if (ids?.Any() ?? false)
        {
            return result?.Where(x => ids.Contains(x.Id)) ?? Enumerable.Empty<TEntity>();
        }

        return result ?? Enumerable.Empty<TEntity>();
    }

    protected abstract IEnumerable<TEntity>? GetAllWithFullCachePolicy();

    protected virtual PropertyType CreatePropertyType(string propertyEditorAlias, ValueStorageType storageType,
        string propertyTypeAlias) =>
        new PropertyType(_shortStringHelper, propertyEditorAlias, storageType, propertyTypeAlias);

    protected override void PersistDeletedItem(TEntity entity)
    {
        base.PersistDeletedItem(entity);
        CommonRepository.ClearCache(); // always
    }

    protected void PersistNewBaseContentType(IContentTypeComposition entity)
    {
        ValidateVariations(entity);

        ContentTypeDto dto = ContentTypeFactory.BuildContentTypeDto(entity);

        // Cannot add a duplicate content type
        var exists = Database.ExecuteScalar<int>(
            @"SELECT COUNT(*) FROM cmsContentType
INNER JOIN umbracoNode ON cmsContentType.nodeId = umbracoNode.id
WHERE cmsContentType." + SqlSyntax.GetQuotedColumnName("alias") + @"= @alias
AND umbracoNode.nodeObjectType = @objectType",
            new { alias = entity.Alias, objectType = NodeObjectTypeId });
        if (exists > 0)
        {
            throw new DuplicateNameException("An item with the alias " + entity.Alias + " already exists");
        }

        // Logic for setting Path, Level and SortOrder
        NodeDto? parent = Database.First<NodeDto>("WHERE id = @ParentId", new { entity.ParentId });
        var level = parent.Level + 1;
        var sortOrder =
            Database.ExecuteScalar<int>(
                "SELECT COUNT(*) FROM umbracoNode WHERE parentID = @ParentId AND nodeObjectType = @NodeObjectType",
                new { entity.ParentId, NodeObjectType = NodeObjectTypeId });

        // Create the (base) node data - umbracoNode
        NodeDto nodeDto = dto.NodeDto;
        nodeDto.Path = parent.Path;
        nodeDto.Level = short.Parse(level.ToString(CultureInfo.InvariantCulture));
        nodeDto.SortOrder = sortOrder;
        var o = Database.IsNew(nodeDto)
            ? Convert.ToInt32(Database.Insert(nodeDto))
            : Database.Update(nodeDto);

        // Update with new correct path
        nodeDto.Path = string.Concat(parent.Path, ",", nodeDto.NodeId);
        Database.Update(nodeDto);

        // Update entity with correct values
        entity.Id = nodeDto.NodeId; // Set Id on entity to ensure an Id is set
        entity.Path = nodeDto.Path;
        entity.SortOrder = sortOrder;
        entity.Level = level;

        // Insert new ContentType entry
        dto.NodeId = nodeDto.NodeId;
        Database.Insert(dto);

        // Insert ContentType composition in new table
        foreach (IContentTypeComposition composition in entity.ContentTypeComposition)
        {
            if (composition.Id == entity.Id)
            {
                continue; // Just to ensure that we aren't creating a reference to ourself.
            }

            if (composition.HasIdentity)
            {
                Database.Insert(new ContentType2ContentTypeDto { ParentId = composition.Id, ChildId = entity.Id });
            }
            else
            {
                // Fallback for ContentTypes with no identity
                ContentTypeDto? contentTypeDto =
                    Database.FirstOrDefault<ContentTypeDto>(
                        "WHERE alias = @Alias",
                        new { composition.Alias });
                if (contentTypeDto != null)
                {
                    Database.Insert(new ContentType2ContentTypeDto
                    {
                        ParentId = contentTypeDto.NodeId,
                        ChildId = entity.Id,
                    });
                }
            }
        }

        if (entity.AllowedContentTypes is not null)
        {
            // Insert collection of allowed content types
            foreach (ContentTypeSort allowedContentType in entity.AllowedContentTypes)
            {
                Database.Insert(new ContentTypeAllowedContentTypeDto
                {
                    Id = entity.Id,
                    AllowedId = allowedContentType.Id.Value,
                    SortOrder = allowedContentType.SortOrder,
                });
            }
        }

        // Insert Tabs
        foreach (PropertyGroup propertyGroup in entity.PropertyGroups)
        {
            PropertyTypeGroupDto tabDto = PropertyGroupFactory.BuildGroupDto(propertyGroup, nodeDto.NodeId);
            var primaryKey = Convert.ToInt32(Database.Insert(tabDto));
            propertyGroup.Id = primaryKey; // Set Id on PropertyGroup

            // Ensure that the PropertyGroup's Id is set on the PropertyTypes within a group
            // unless the PropertyGroupId has already been changed.
            if (propertyGroup.PropertyTypes is not null)
            {
                foreach (IPropertyType propertyType in propertyGroup.PropertyTypes)
                {
                    if (propertyType.IsPropertyDirty("PropertyGroupId") == false)
                    {
                        PropertyGroup tempGroup = propertyGroup;
                        propertyType.PropertyGroupId = new Lazy<int>(() => tempGroup.Id);
                    }
                }
            }
        }

        // Insert PropertyTypes
        foreach (IPropertyType propertyType in entity.PropertyTypes)
        {
            var tabId = propertyType.PropertyGroupId != null ? propertyType.PropertyGroupId.Value : default;

            // If the Id of the DataType is not set, we resolve it from the db by its PropertyEditorAlias
            if (propertyType.DataTypeId == 0 || propertyType.DataTypeId == default)
            {
                AssignDataTypeFromPropertyEditor(propertyType);
            }

            PropertyTypeDto propertyTypeDto =
                PropertyGroupFactory.BuildPropertyTypeDto(tabId, propertyType, nodeDto.NodeId);
            var typePrimaryKey = Convert.ToInt32(Database.Insert(propertyTypeDto));
            propertyType.Id = typePrimaryKey; // Set Id on new PropertyType

            // Update the current PropertyType with correct PropertyEditorAlias and DatabaseType
            DataTypeDto? dataTypeDto =
                Database.FirstOrDefault<DataTypeDto>("WHERE nodeId = @Id", new { Id = propertyTypeDto.DataTypeId });
            propertyType.PropertyEditorAlias = dataTypeDto.EditorAlias;
            propertyType.ValueStorageType = dataTypeDto.DbType.EnumParse<ValueStorageType>(true);
        }

        CommonRepository.ClearCache(); // always
    }

    protected void PersistUpdatedBaseContentType(IContentTypeComposition entity)
    {
        CorrectPropertyTypeVariations(entity);
        ValidateVariations(entity);

        ContentTypeDto dto = ContentTypeFactory.BuildContentTypeDto(entity);

        // ensure the alias is not used already
        var exists = Database.ExecuteScalar<int>(
            @"SELECT COUNT(*) FROM cmsContentType
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
        NodeDto nodeDto = dto.NodeDto;
        Database.Update(nodeDto);

        // we NEED this: updating, so the .PrimaryKey already exists, but the entity does
        // not carry it and therefore the dto does not have it yet - must get it from db,
        // look up ContentType entry to get PrimaryKey for updating the DTO
        ContentTypeDto? dtoPk = Database.First<ContentTypeDto>("WHERE nodeId = @Id", new { entity.Id });
        dto.PrimaryKey = dtoPk.PrimaryKey;
        Database.Update(dto);

        // handle (delete then recreate) compositions
        Database.Delete<ContentType2ContentTypeDto>("WHERE childContentTypeId = @Id", new { entity.Id });
        foreach (IContentTypeComposition composition in entity.ContentTypeComposition)
        {
            Database.Insert(new ContentType2ContentTypeDto { ParentId = composition.Id, ChildId = entity.Id });
        }

        // removing a ContentType from a composition (U4-1690)
        // 1. Find content based on the current ContentType: entity.Id
        // 2. Find all PropertyTypes on the ContentType that was removed - tracked id (key)
        // 3. Remove properties based on property types from the removed content type where the content ids correspond to those found in step one
        if (entity.RemovedContentTypes.Any())
        {
            // TODO: Could we do the below with bulk SQL statements instead of looking everything up and then manipulating?

            // find Content based on the current ContentType
            Sql<ISqlContext> sql = Sql()
                .SelectAll()
                .From<ContentDto>()
                .InnerJoin<NodeDto>().On<ContentDto, NodeDto>(left => left.NodeId, right => right.NodeId)
                .Where<NodeDto>(x => x.NodeObjectType == Constants.ObjectTypes.Document)
                .Where<ContentDto>(x => x.ContentTypeId == entity.Id);
            List<ContentDto>? contentDtos = Database.Fetch<ContentDto>(sql);

            // loop through all tracked keys, which corresponds to the ContentTypes that has been removed from the composition
            foreach (var key in entity.RemovedContentTypes)
            {
                // find PropertyTypes for the removed ContentType
                List<PropertyTypeDto>? propertyTypes =
                    Database.Fetch<PropertyTypeDto>("WHERE contentTypeId = @Id", new { Id = key });

                // loop through the Content that is based on the current ContentType in order to remove the Properties that are
                // based on the PropertyTypes that belong to the removed ContentType.
                foreach (ContentDto? contentDto in contentDtos)
                {
                    // TODO: This could be done with bulk SQL statements
                    foreach (PropertyTypeDto? propertyType in propertyTypes)
                    {
                        var nodeId = contentDto.NodeId;
                        var propertyTypeId = propertyType.Id;
                        Sql<ISqlContext> propertySql = Sql()
                            .Select<PropertyDataDto>(x => x.Id)
                            .From<PropertyDataDto>()
                            .InnerJoin<PropertyTypeDto>()
                            .On<PropertyDataDto, PropertyTypeDto>((left, right) => left.PropertyTypeId == right.Id)
                            .InnerJoin<ContentVersionDto>()
                            .On<PropertyDataDto, ContentVersionDto>((left, right) => left.VersionId == right.Id)
                            .Where<ContentVersionDto>(x => x.NodeId == nodeId)
                            .Where<PropertyTypeDto>(x => x.Id == propertyTypeId);

                        // finally delete the properties that match our criteria for removing a ContentType from the composition
                        Database.Delete<PropertyDataDto>(new Sql(
                            "WHERE id IN (" + propertySql.SQL + ")",
                            propertySql.Arguments));
                    }
                }
            }
        }

        // delete the allowed content type entries before re-inserting the collection of allowed content types
        Database.Delete<ContentTypeAllowedContentTypeDto>("WHERE Id = @Id", new { entity.Id });
        if (entity.AllowedContentTypes is not null)
        {
            foreach (ContentTypeSort allowedContentType in entity.AllowedContentTypes)
            {
                Database.Insert(new ContentTypeAllowedContentTypeDto
                {
                    Id = entity.Id,
                    AllowedId = allowedContentType.Id.Value,
                    SortOrder = allowedContentType.SortOrder,
                });
            }
        }

        // Delete property types ... by excepting entries from db with entries from collections.
        // We check if the entity's own PropertyTypes has been modified and then also check
        // any of the property groups PropertyTypes has been modified.
        // This specifically tells us if any property type collections have changed.
        if (entity.IsPropertyDirty("NoGroupPropertyTypes") ||
            entity.PropertyGroups.Any(x => x.IsPropertyDirty("PropertyTypes")))
        {
            List<PropertyTypeDto>? dbPropertyTypes =
                Database.Fetch<PropertyTypeDto>("WHERE contentTypeId = @Id", new { entity.Id });
            IEnumerable<int> dbPropertyTypeIds = dbPropertyTypes.Select(x => x.Id);
            IEnumerable<int> entityPropertyTypes = entity.PropertyTypes.Where(x => x.HasIdentity).Select(x => x.Id);
            IEnumerable<int> propertyTypeToDeleteIds = dbPropertyTypeIds.Except(entityPropertyTypes);
            foreach (var propertyTypeId in propertyTypeToDeleteIds)
            {
                DeletePropertyType(entity.Id, propertyTypeId);
            }
        }

        // Delete tabs ... by excepting entries from db with entries from collections.
        // We check if the entity's own PropertyGroups has been modified.
        // This specifically tells us if the property group collections have changed.
        List<int>? orphanPropertyTypeIds = null;
        if (entity.IsPropertyDirty("PropertyGroups"))
        {
            // TODO: we used to try to propagate tabs renaming downstream, relying on ParentId, but
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
            var existingPropertyGroups = Database
                .Fetch<PropertyTypeGroupDto>("WHERE contentTypeNodeId = @id", new { id = entity.Id })
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
                orphanPropertyTypeIds = Database.Fetch<PropertyTypeDto>(
                    "WHERE propertyTypeGroupId IN (@ids)",
                    new { ids = groupsToDelete })
                    .Select(x => x.Id).ToList();
                Database.Update<PropertyTypeDto>(
                    "SET propertyTypeGroupId = NULL WHERE propertyTypeGroupId IN (@ids)",
                    new { ids = groupsToDelete });

                // now we can delete the tabs
                Database.Delete<PropertyTypeGroupDto>("WHERE id IN (@ids)", new { ids = groupsToDelete });
            }
        }

        // insert or update groups, assign properties
        foreach (PropertyGroup propertyGroup in entity.PropertyGroups)
        {
            // insert or update group
            PropertyTypeGroupDto groupDto = PropertyGroupFactory.BuildGroupDto(propertyGroup, entity.Id);
            var groupId = propertyGroup.HasIdentity
                ? Database.Update(groupDto)
                : Convert.ToInt32(Database.Insert(groupDto));
            if (propertyGroup.HasIdentity == false)
            {
                propertyGroup.Id = groupId;
            }
            else
            {
                groupId = propertyGroup.Id;
            }

            // assign properties to the group
            // (all of them, even those that have .IsPropertyDirty("PropertyGroupId") == true,
            //  because it should have been set to this group anyways and better be safe)
            if (propertyGroup.PropertyTypes is not null)
            {
                foreach (IPropertyType propertyType in propertyGroup.PropertyTypes)
                {
                    propertyType.PropertyGroupId = new Lazy<int>(() => groupId);
                }
            }
        }

        // check if the content type variation has been changed
        var contentTypeVariationDirty = entity.IsPropertyDirty("Variations");
        var oldContentTypeVariation = (ContentVariation)dtoPk.Variations;
        ContentVariation newContentTypeVariation = entity.Variations;
        var contentTypeVariationChanging =
            contentTypeVariationDirty && oldContentTypeVariation != newContentTypeVariation;
        if (contentTypeVariationChanging)
        {
            MoveContentTypeVariantData(entity, oldContentTypeVariation, newContentTypeVariation);
            Clear301Redirects(entity);
            ClearScheduledPublishing(entity);
        }

        // collect property types that have a dirty variation
        List<IPropertyType>? propertyTypeVariationDirty = null;

        // note: this only deals with *local* property types, we're dealing w/compositions later below
        foreach (IPropertyType propertyType in entity.PropertyTypes)
        {
            // track each property individually
            if (propertyType.IsPropertyDirty("Variations"))
            {
                // allocate the list only when needed
                if (propertyTypeVariationDirty == null)
                {
                    propertyTypeVariationDirty = new List<IPropertyType>();
                }

                propertyTypeVariationDirty.Add(propertyType);
            }
        }

        // figure out dirty property types that have actually changed
        // before we insert or update properties, so we can read the old variations
        Dictionary<int, (ContentVariation FromVariation, ContentVariation ToVariation)>? propertyTypeVariationChanges =
            propertyTypeVariationDirty != null
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
            propertyTypeVariationChanges ??= new Dictionary<int, (ContentVariation, ContentVariation)>();

            foreach (IPropertyType composedPropertyType in entity.GetOriginalComposedPropertyTypes())
            {
                if (composedPropertyType.Variations == ContentVariation.Nothing)
                {
                    continue;
                }

                // Determine target variation of the composed property type.
                // The composed property is only considered culture variant when the base content type is also culture variant.
                // The composed property is only considered segment variant when the base content type is also segment variant.
                // Example: Culture variant content type with a Culture+Segment variant property type will become ContentVariation.Culture
                ContentVariation target = newContentTypeVariation & composedPropertyType.Variations;

                // Determine the previous variation
                // We have to compare with the old content type variation because the composed property might already have changed
                // Example: A property with variations in an element type with variations is used in a document without
                //          when you enable variations the property has already enabled variations from the element type,
                //          but it's still a change from nothing because the document did not have variations, but it does now.
                ContentVariation from = oldContentTypeVariation & composedPropertyType.Variations;

                propertyTypeVariationChanges[composedPropertyType.Id] = (from, target);
            }
        }

        // insert or update properties
        // all of them, no-group and in-groups
        foreach (IPropertyType propertyType in entity.PropertyTypes)
        {
            // if the Id of the DataType is not set, we resolve it from the db by its PropertyEditorAlias
            if (propertyType.DataTypeId == 0 || propertyType.DataTypeId == default)
            {
                AssignDataTypeFromPropertyEditor(propertyType);
            }

            // validate the alias
            ValidateAlias(propertyType);

            // insert or update property
            var groupId = propertyType.PropertyGroupId?.Value ?? default;
            PropertyTypeDto propertyTypeDto =
                PropertyGroupFactory.BuildPropertyTypeDto(groupId, propertyType, entity.Id);
            var typeId = propertyType.HasIdentity
                ? Database.Update(propertyTypeDto)
                : Convert.ToInt32(Database.Insert(propertyTypeDto));
            if (propertyType.HasIdentity == false)
            {
                propertyType.Id = typeId;
            }
            else
            {
                typeId = propertyType.Id;
            }

            // not an orphan anymore
            orphanPropertyTypeIds?.Remove(typeId);
        }

        // must restrict property data changes to impacted content types - if changing a composing
        // type, some composed types (those that do not vary) are not impacted and should be left
        // unchanged
        //
        // getting 'all' from the cache policy is prone to race conditions - fast but dangerous
        // var all = ((FullDataSetRepositoryCachePolicy<TEntity, int>)CachePolicy).GetAllCached(PerformGetAll);
        IEnumerable<TEntity>? all = PerformGetAll();

        IEnumerable<IContentTypeComposition> impacted = GetImpactedContentTypes(entity, all);

        // if some property types have actually changed, move their variant data
        if (propertyTypeVariationChanges?.Count > 0)
        {
            MovePropertyTypeVariantData(propertyTypeVariationChanges, impacted);
        }

        // deal with orphan properties: those that were in a deleted tab,
        // and have not been re-mapped to another tab or to 'generic properties'
        if (orphanPropertyTypeIds != null)
        {
            foreach (var id in orphanPropertyTypeIds)
            {
                DeletePropertyType(entity.Id, id);
            }
        }

        CommonRepository.ClearCache(); // always
    }

    protected void ValidateAlias(IPropertyType pt)
    {
        if (string.IsNullOrWhiteSpace(pt.Alias))
        {
            var ex = new InvalidOperationException(
                $"Property Type '{pt.Name}' cannot have an empty Alias. This is most likely due to invalid characters stripped from the Alias.");

            Logger.LogError(
                "Property Type '{PropertyTypeName}' cannot have an empty Alias. This is most likely due to invalid characters stripped from the Alias.",
                pt.Name);

            throw ex;
        }
    }

    private static bool IsPropertyValueChanged(PropertyValueVersionDto pubRow, PropertyValueVersionDto row) =>
        (!pubRow.TextValue.IsNullOrWhiteSpace() && pubRow.TextValue != row.TextValue)
        || (!pubRow.VarcharValue.IsNullOrWhiteSpace() && pubRow.VarcharValue != row.VarcharValue)
        || (pubRow.DateValue.HasValue && pubRow.DateValue != row.DateValue)
        || (pubRow.DecimalValue.HasValue && pubRow.DecimalValue != row.DecimalValue)
        || (pubRow.IntValue.HasValue && pubRow.IntValue != row.IntValue);

    /// <summary>
    ///     Corrects the property type variations for the given entity
    ///     to make sure the property type variation is compatible with the
    ///     variation set on the entity itself.
    /// </summary>
    /// <param name="entity">Entity to correct properties for</param>
    private void CorrectPropertyTypeVariations(IContentTypeComposition entity)
    {
        // Update property variations based on the content type variation
        foreach (IPropertyType propertyType in entity.PropertyTypes)
        {
            // Determine variation for the property type.
            // The property is only considered culture variant when the base content type is also culture variant.
            // The property is only considered segment variant when the base content type is also segment variant.
            // Example: Culture variant content type with a Culture+Segment variant property type will become ContentVariation.Culture
            propertyType.Variations = entity.Variations & propertyType.Variations;
        }
    }

    /// <summary>
    ///     Ensures that no property types are flagged for a variance that is not supported by the content type itself
    /// </summary>
    /// <param name="entity">The entity for which the property types will be validated</param>
    private void ValidateVariations(IContentTypeComposition entity)
    {
        foreach (IPropertyType prop in entity.PropertyTypes)
        {
            // The variation of a property is only allowed if all its variation flags
            // are also set on the entity itself. It cannot set anything that is not also set by the content type.
            // For example, when entity.Variations is set to Culture a property cannot be set to Segment.
            var isValid = entity.Variations.HasFlag(prop.Variations);
            if (!isValid)
            {
                throw new InvalidOperationException(
                    $"The property {prop.Alias} cannot have variations of {prop.Variations} with the content type variations of {entity.Variations}");
            }
        }
    }

    private IEnumerable<IContentTypeComposition> GetImpactedContentTypes(
        IContentTypeComposition contentType,
        IEnumerable<IContentTypeComposition>? all)
    {
        if (all is null)
        {
            return Enumerable.Empty<IContentTypeComposition>();
        }

        var impact = new List<IContentTypeComposition>();
        var set = new List<IContentTypeComposition> { contentType };

        var tree = new Dictionary<int, List<IContentTypeComposition>>();
        foreach (IContentTypeComposition x in all)
        {
            foreach (IContentTypeComposition y in x.ContentTypeComposition)
            {
                if (!tree.TryGetValue(y.Id, out List<IContentTypeComposition>? list))
                {
                    list = tree[y.Id] = new List<IContentTypeComposition>();
                }

                list.Add(x);
            }
        }

        var nset = new List<IContentTypeComposition>();
        do
        {
            impact.AddRange(set);

            foreach (IContentTypeComposition x in set)
            {
                if (!tree.TryGetValue(x.Id, out List<IContentTypeComposition>? list))
                {
                    continue;
                }

                nset.AddRange(list.Where(y => y.VariesByCulture()));
            }

            set = nset;
            nset = new List<IContentTypeComposition>();
        }
        while (set.Count > 0);

        return impact;
    }

    // gets property types that have actually changed, and the corresponding changes
    // returns null if no property type has actually changed
    private Dictionary<int, (ContentVariation FromVariation, ContentVariation ToVariation)>?
        GetPropertyVariationChanges(IEnumerable<IPropertyType> propertyTypes)
    {
        var propertyTypesL = propertyTypes.ToList();

        // select the current variations (before the change) from database
        Sql<ISqlContext> selectCurrentVariations = Sql()
            .Select<PropertyTypeDto>(x => x.Id, x => x.Variations)
            .From<PropertyTypeDto>()
            .WhereIn<PropertyTypeDto>(x => x.Id, propertyTypesL.Select(x => x.Id));

        Dictionary<int, byte>? oldVariations = Database.Dictionary<int, byte>(selectCurrentVariations);

        // build a dictionary of actual changes
        Dictionary<int, (ContentVariation, ContentVariation)>? changes = null;

        foreach (IPropertyType propertyType in propertyTypesL)
        {
            // new property type, ignore
            if (!oldVariations.TryGetValue(propertyType.Id, out var oldVariationB))
            {
                continue;
            }

            var oldVariation = (ContentVariation)oldVariationB; // NPoco cannot fetch directly

            // only those property types that *actually* changed
            ContentVariation newVariation = propertyType.Variations;
            if (oldVariation == newVariation)
            {
                continue;
            }

            // allocate the dictionary only when needed
            if (changes == null)
            {
                changes = new Dictionary<int, (ContentVariation, ContentVariation)>();
            }

            changes[propertyType.Id] = (oldVariation, newVariation);
        }

        return changes;
    }

    /// <summary>
    ///     Clear any redirects associated with content for a content type
    /// </summary>
    private void Clear301Redirects(IContentTypeComposition contentType)
    {
        // first clear out any existing property data that might already exists under the default lang
        Sql<ISqlContext> sqlSelect = Sql().Select<NodeDto>(x => x.UniqueId)
            .From<NodeDto>()
            .InnerJoin<ContentDto>().On<ContentDto, NodeDto>(x => x.NodeId, x => x.NodeId)
            .Where<ContentDto>(x => x.ContentTypeId == contentType.Id);
        Sql<ISqlContext> sqlDelete = Sql()
            .Delete<RedirectUrlDto>()
            .WhereIn(
                (Expression<Func<RedirectUrlDto, object?>>)(x => x.ContentKey),
                sqlSelect);

        Database.Execute(sqlDelete);
    }

    /// <summary>
    ///     Clear any scheduled publishing associated with content for a content type
    /// </summary>
    private void ClearScheduledPublishing(IContentTypeComposition contentType)
    {
        // TODO: Fill this in when scheduled publishing is enabled for variants
    }

    /// <summary>
    ///     Gets the default language identifier.
    /// </summary>
    private int GetDefaultLanguageId()
    {
        Sql<ISqlContext> selectDefaultLanguageId = Sql()
            .Select<LanguageDto>(x => x.Id)
            .From<LanguageDto>()
            .Where<LanguageDto>(x => x.IsDefault);

        return Database.First<int>(selectDefaultLanguageId);
    }

    /// <summary>
    ///     Moves variant data for property type variation changes.
    /// </summary>
    private void MovePropertyTypeVariantData(
        IDictionary<int, (ContentVariation FromVariation, ContentVariation ToVariation)> propertyTypeChanges,
        IEnumerable<IContentTypeComposition> impacted)
    {
        var defaultLanguageId = GetDefaultLanguageId();
        var impactedL = impacted.Select(x => x.Id).ToList();

        // Group by the "To" variation so we can bulk update in the correct batches
        foreach (IGrouping<(ContentVariation FromVariation, ContentVariation ToVariation),
                     KeyValuePair<int, (ContentVariation FromVariation, ContentVariation ToVariation)>> grouping in
                 propertyTypeChanges.GroupBy(x => x.Value))
        {
            var propertyTypeIds = grouping.Select(x => x.Key).ToList();
            (ContentVariation FromVariation, ContentVariation ToVariation) = grouping.Key;

            var fromCultureEnabled = FromVariation.HasFlag(ContentVariation.Culture);
            var toCultureEnabled = ToVariation.HasFlag(ContentVariation.Culture);

            if (!fromCultureEnabled && toCultureEnabled)
            {
                // Culture has been enabled
                CopyPropertyData(null, defaultLanguageId, propertyTypeIds, impactedL);
                CopyTagData(null, defaultLanguageId, propertyTypeIds, impactedL);
                RenormalizeDocumentEditedFlags(propertyTypeIds, impactedL);
            }
            else if (fromCultureEnabled && !toCultureEnabled)
            {
                // Culture has been disabled
                CopyPropertyData(defaultLanguageId, null, propertyTypeIds, impactedL);
                CopyTagData(defaultLanguageId, null, propertyTypeIds, impactedL);
                RenormalizeDocumentEditedFlags(propertyTypeIds, impactedL);
            }
        }
    }

    /// <summary>
    ///     Moves variant data for a content type variation change.
    /// </summary>
    private void MoveContentTypeVariantData(IContentTypeComposition contentType, ContentVariation fromVariation,
        ContentVariation toVariation)
    {
        var defaultLanguageId = GetDefaultLanguageId();

        var cultureIsNotEnabled = !fromVariation.HasFlag(ContentVariation.Culture);
        var cultureWillBeEnabled = toVariation.HasFlag(ContentVariation.Culture);

        if (cultureIsNotEnabled && cultureWillBeEnabled)
        {
            // move the names
            // first clear out any existing names that might already exists under the default lang
            // there's 2x tables to update

            // clear out the versionCultureVariation table
            Sql<ISqlContext> sqlSelect = Sql().Select<ContentVersionCultureVariationDto>(x => x.Id)
                .From<ContentVersionCultureVariationDto>()
                .InnerJoin<ContentVersionDto>()
                .On<ContentVersionDto, ContentVersionCultureVariationDto>(x => x.Id, x => x.VersionId)
                .InnerJoin<ContentDto>().On<ContentDto, ContentVersionDto>(x => x.NodeId, x => x.NodeId)
                .Where<ContentDto>(x => x.ContentTypeId == contentType.Id)
                .Where<ContentVersionCultureVariationDto>(x => x.LanguageId == defaultLanguageId);
            Sql<ISqlContext> sqlDelete = Sql()
                .Delete<ContentVersionCultureVariationDto>()
                .WhereIn<ContentVersionCultureVariationDto>(x => x.Id, sqlSelect);

            Database.Execute(sqlDelete);

            // clear out the documentCultureVariation table
            sqlSelect = Sql().Select<DocumentCultureVariationDto>(x => x.Id)
                .From<DocumentCultureVariationDto>()
                .InnerJoin<ContentDto>().On<ContentDto, DocumentCultureVariationDto>(x => x.NodeId, x => x.NodeId)
                .Where<ContentDto>(x => x.ContentTypeId == contentType.Id)
                .Where<DocumentCultureVariationDto>(x => x.LanguageId == defaultLanguageId);
            sqlDelete = Sql()
                .Delete<DocumentCultureVariationDto>()
                .WhereIn<DocumentCultureVariationDto>(x => x.Id, sqlSelect);

            Database.Execute(sqlDelete);

            // now we need to insert names into these 2 tables based on the invariant data

            // insert rows into the versionCultureVariationDto table based on the data from contentVersionDto for the default lang
            var cols = Sql().ColumnsForInsert<ContentVersionCultureVariationDto>(x => x.VersionId, x => x.Name,
                x => x.UpdateUserId, x => x.UpdateDate, x => x.LanguageId);
            sqlSelect = Sql().Select<ContentVersionDto>(x => x.Id, x => x.Text, x => x.UserId, x => x.VersionDate)
                .Append($", {defaultLanguageId}") // default language ID
                .From<ContentVersionDto>()
                .InnerJoin<ContentDto>().On<ContentDto, ContentVersionDto>(x => x.NodeId, x => x.NodeId)
                .Where<ContentDto>(x => x.ContentTypeId == contentType.Id);
            Sql<ISqlContext>? sqlInsert = Sql($"INSERT INTO {ContentVersionCultureVariationDto.TableName} ({cols})")
                .Append(sqlSelect);

            Database.Execute(sqlInsert);

            // insert rows into the documentCultureVariation table
            cols = Sql().ColumnsForInsert<DocumentCultureVariationDto>(x => x.NodeId, x => x.Edited, x => x.Published,
                x => x.Name, x => x.Available, x => x.LanguageId);
            sqlSelect = Sql().Select<DocumentDto>(x => x.NodeId, x => x.Edited, x => x.Published)
                .AndSelect<NodeDto>(x => x.Text)
                .Append($", 1, {defaultLanguageId}") // make Available + default language ID
                .From<DocumentDto>()
                .InnerJoin<NodeDto>().On<NodeDto, DocumentDto>(x => x.NodeId, x => x.NodeId)
                .InnerJoin<ContentDto>().On<ContentDto, NodeDto>(x => x.NodeId, x => x.NodeId)
                .Where<ContentDto>(x => x.ContentTypeId == contentType.Id);
            sqlInsert = Sql($"INSERT INTO {DocumentCultureVariationDto.TableName} ({cols})").Append(sqlSelect);

            Database.Execute(sqlInsert);
        }
    }

    ///
    private void CopyTagData(
        int? sourceLanguageId,
        int? targetLanguageId,
        IReadOnlyCollection<int> propertyTypeIds,
        IReadOnlyCollection<int>? contentTypeIds = null)
    {
        // note: important to use SqlNullableEquals for nullable types, cannot directly compare language identifiers
        var whereInArgsCount = propertyTypeIds.Count + (contentTypeIds?.Count ?? 0);
        if (whereInArgsCount > Constants.Sql.MaxParameterCount)
        {
            throw new NotSupportedException("Too many property/content types.");
        }

        // delete existing relations (for target language)
        // do *not* delete existing tags
        Sql<ISqlContext> sqlSelectTagsToDelete = Sql()
            .Select<TagDto>(x => x.Id)
            .From<TagDto>()
            .InnerJoin<TagRelationshipDto>().On<TagDto, TagRelationshipDto>((tag, rel) => tag.Id == rel.TagId);

        if (contentTypeIds != null)
        {
            sqlSelectTagsToDelete
                .InnerJoin<ContentDto>()
                .On<TagRelationshipDto, ContentDto>((rel, content) => rel.NodeId == content.NodeId)
                .WhereIn<ContentDto>(x => x.ContentTypeId, contentTypeIds);
        }

        sqlSelectTagsToDelete
            .WhereIn<TagRelationshipDto>(x => x.PropertyTypeId, propertyTypeIds)
            .Where<TagDto>(x => x.LanguageId.SqlNullableEquals(targetLanguageId, -1));

        Sql<ISqlContext> sqlDeleteRelations = Sql()
            .Delete<TagRelationshipDto>()
            .WhereIn<TagRelationshipDto>(x => x.TagId, sqlSelectTagsToDelete);

        Database.Execute(sqlDeleteRelations);

        // do *not* delete the tags - they could be used by other content types / property types
        /*
        var sqlDeleteTag = Sql()
            .Delete<TagDto>()
            .WhereIn<TagDto>(x => x.Id, sqlTagToDelete);
        Database.Execute(sqlDeleteTag);
        */

        // copy tags from source language to target language
        // target tags may exist already, so we have to check for existence here
        //
        // select tags to insert: tags pointed to by a relation ship, for proper property/content types,
        // and of source language, and where we cannot left join to an existing tag with same text,
        // group and languageId
        var targetLanguageIdS = targetLanguageId.HasValue ? targetLanguageId.ToString() : "NULL";
        Sql<ISqlContext> sqlSelectTagsToInsert = Sql()
            .SelectDistinct<TagDto>(x => x.Text, x => x.Group)
            .Append(", " + targetLanguageIdS)
            .From<TagDto>();

        sqlSelectTagsToInsert
            .InnerJoin<TagRelationshipDto>().On<TagDto, TagRelationshipDto>((tag, rel) => tag.Id == rel.TagId)
            .LeftJoin<TagDto>("xtags")
            .On<TagDto, TagDto>(
                (tag, xtag) => tag.Text == xtag.Text && tag.Group == xtag.Group &&
                               xtag.LanguageId.SqlNullableEquals(targetLanguageId, -1), aliasRight: "xtags");

        if (contentTypeIds != null)
        {
            sqlSelectTagsToInsert
                .InnerJoin<ContentDto>()
                .On<TagRelationshipDto, ContentDto>((rel, content) => rel.NodeId == content.NodeId)
                .WhereIn<ContentDto>(x => x.ContentTypeId, contentTypeIds);
        }

        sqlSelectTagsToInsert
            .WhereIn<TagRelationshipDto>(x => x.PropertyTypeId, propertyTypeIds)
            .WhereNull<TagDto>(x => x.Id, "xtags") // ie, not exists
            .Where<TagDto>(x => x.LanguageId.SqlNullableEquals(sourceLanguageId, -1));

        var cols = Sql().ColumnsForInsert<TagDto>(x => x.Text, x => x.Group, x => x.LanguageId);
        Sql<ISqlContext>? sqlInsertTags = Sql($"INSERT INTO {TagDto.TableName} ({cols})").Append(sqlSelectTagsToInsert);

        Database.Execute(sqlInsertTags);

        // create relations to new tags
        // any existing relations have been deleted above, no need to check for existence here
        //
        // select node id and property type id from existing relations to tags of source language,
        // for proper property/content types, and select new tag id from tags, with matching text,
        // and group, but for the target language
        Sql<ISqlContext> sqlSelectRelationsToInsert = Sql()
            .SelectDistinct<TagRelationshipDto>(x => x.NodeId, x => x.PropertyTypeId)
            .AndSelect<TagDto>("otag", x => x.Id)
            .From<TagRelationshipDto>()
            .InnerJoin<TagDto>().On<TagRelationshipDto, TagDto>((rel, tag) => rel.TagId == tag.Id)
            .InnerJoin<TagDto>("otag")
            .On<TagDto, TagDto>(
                (tag, otag) => tag.Text == otag.Text && tag.Group == otag.Group &&
                               otag.LanguageId.SqlNullableEquals(targetLanguageId, -1), aliasRight: "otag");

        if (contentTypeIds != null)
        {
            sqlSelectRelationsToInsert
                .InnerJoin<ContentDto>()
                .On<TagRelationshipDto, ContentDto>((rel, content) => rel.NodeId == content.NodeId)
                .WhereIn<ContentDto>(x => x.ContentTypeId, contentTypeIds);
        }

        sqlSelectRelationsToInsert
            .Where<TagDto>(x => x.LanguageId.SqlNullableEquals(sourceLanguageId, -1))
            .WhereIn<TagRelationshipDto>(x => x.PropertyTypeId, propertyTypeIds);

        var relationColumnsToInsert =
            Sql().ColumnsForInsert<TagRelationshipDto>(x => x.NodeId, x => x.PropertyTypeId, x => x.TagId);
        Sql<ISqlContext>? sqlInsertRelations =
            Sql($"INSERT INTO {TagRelationshipDto.TableName} ({relationColumnsToInsert})")
                .Append(sqlSelectRelationsToInsert);

        Database.Execute(sqlInsertRelations);

        // delete original relations - *not* the tags - all of them
        // cannot really "go back" with relations, would have to do it with property values
        sqlSelectTagsToDelete = Sql()
            .Select<TagDto>(x => x.Id)
            .From<TagDto>()
            .InnerJoin<TagRelationshipDto>().On<TagDto, TagRelationshipDto>((tag, rel) => tag.Id == rel.TagId);

        if (contentTypeIds != null)
        {
            sqlSelectTagsToDelete
                .InnerJoin<ContentDto>()
                .On<TagRelationshipDto, ContentDto>((rel, content) => rel.NodeId == content.NodeId)
                .WhereIn<ContentDto>(x => x.ContentTypeId, contentTypeIds);
        }

        sqlSelectTagsToDelete
            .WhereIn<TagRelationshipDto>(x => x.PropertyTypeId, propertyTypeIds)
            .Where<TagDto>(x => !x.LanguageId.SqlNullableEquals(targetLanguageId, -1));

        sqlDeleteRelations = Sql()
            .Delete<TagRelationshipDto>()
            .WhereIn<TagRelationshipDto>(x => x.TagId, sqlSelectTagsToDelete);

        Database.Execute(sqlDeleteRelations);

        // no
        /*
        var sqlDeleteTag = Sql()
            .Delete<TagDto>()
            .WhereIn<TagDto>(x => x.Id, sqlTagToDelete);
        Database.Execute(sqlDeleteTag);
        */
    }

    /// <summary>
    ///     Copies property data from one language to another.
    /// </summary>
    /// <param name="sourceLanguageId">The source language (can be null ie invariant).</param>
    /// <param name="targetLanguageId">The target language (can be null ie invariant)</param>
    /// <param name="propertyTypeIds">The property type identifiers.</param>
    /// <param name="contentTypeIds">The content type identifiers.</param>
    private void CopyPropertyData(int? sourceLanguageId, int? targetLanguageId,
        IReadOnlyCollection<int> propertyTypeIds, IReadOnlyCollection<int>? contentTypeIds = null)
    {
        // note: important to use SqlNullableEquals for nullable types, cannot directly compare language identifiers
        var whereInArgsCount = propertyTypeIds.Count + (contentTypeIds?.Count ?? 0);
        if (whereInArgsCount > Constants.Sql.MaxParameterCount)
        {
            throw new NotSupportedException("Too many property/content types.");
        }

        // first clear out any existing property data that might already exists under the target language
        Sql<ISqlContext> sqlDelete = Sql()
            .Delete<PropertyDataDto>();

        // not ok for SqlCe (no JOIN in DELETE)
        // if (contentTypeIds != null)
        //    sqlDelete
        //        .From<PropertyDataDto>()
        //        .InnerJoin<ContentVersionDto>().On<PropertyDataDto, ContentVersionDto>((pdata, cversion) => pdata.VersionId == cversion.Id)
        //        .InnerJoin<ContentDto>().On<ContentVersionDto, ContentDto>((cversion, c) => cversion.NodeId == c.NodeId);
        Sql<ISqlContext>? inSql = null;
        if (contentTypeIds != null)
        {
            inSql = Sql()
                .Select<ContentVersionDto>(x => x.Id)
                .From<ContentVersionDto>()
                .InnerJoin<ContentDto>()
                .On<ContentVersionDto, ContentDto>((cversion, c) => cversion.NodeId == c.NodeId)
                .WhereIn<ContentDto>(x => x.ContentTypeId, contentTypeIds);
            sqlDelete.WhereIn<PropertyDataDto>(x => x.VersionId, inSql);
        }

        sqlDelete.Where<PropertyDataDto>(x => x.LanguageId.SqlNullableEquals(targetLanguageId, -1));

        sqlDelete
            .WhereIn<PropertyDataDto>(x => x.PropertyTypeId, propertyTypeIds);

        // see note above, not ok for SqlCe
        // if (contentTypeIds != null)
        //    sqlDelete
        //        .WhereIn<ContentDto>(x => x.ContentTypeId, contentTypeIds);
        Database.Execute(sqlDelete);

        // now insert all property data into the target language that exists under the source language
        var targetLanguageIdS = targetLanguageId.HasValue ? targetLanguageId.ToString() : "NULL";
        var cols = Sql().ColumnsForInsert<PropertyDataDto>(x => x.VersionId, x => x.PropertyTypeId, x => x.Segment,
            x => x.IntegerValue, x => x.DecimalValue, x => x.DateValue, x => x.VarcharValue, x => x.TextValue,
            x => x.LanguageId);
        Sql<ISqlContext> sqlSelectData = Sql().Select<PropertyDataDto>(x => x.VersionId, x => x.PropertyTypeId,
                x => x.Segment, x => x.IntegerValue, x => x.DecimalValue, x => x.DateValue, x => x.VarcharValue,
                x => x.TextValue)
            .Append(", " + targetLanguageIdS) // default language ID
            .From<PropertyDataDto>();

        if (contentTypeIds != null)
        {
            sqlSelectData
                .InnerJoin<ContentVersionDto>()
                .On<PropertyDataDto, ContentVersionDto>((pdata, cversion) => pdata.VersionId == cversion.Id)
                .InnerJoin<ContentDto>()
                .On<ContentVersionDto, ContentDto>((cversion, c) => cversion.NodeId == c.NodeId);
        }

        sqlSelectData.Where<PropertyDataDto>(x => x.LanguageId.SqlNullableEquals(sourceLanguageId, -1));

        sqlSelectData
            .WhereIn<PropertyDataDto>(x => x.PropertyTypeId, propertyTypeIds);

        if (contentTypeIds != null)
        {
            sqlSelectData
                .WhereIn<ContentDto>(x => x.ContentTypeId, contentTypeIds);
        }

        Sql<ISqlContext>? sqlInsert = Sql($"INSERT INTO {PropertyDataDto.TableName} ({cols})").Append(sqlSelectData);

        Database.Execute(sqlInsert);

        // when copying from Culture, keep the original values around in case we want to go back
        // when copying from Nothing, kill the original values, we don't want them around
        if (sourceLanguageId == null)
        {
            sqlDelete = Sql()
                .Delete<PropertyDataDto>();

            if (contentTypeIds != null)
            {
                sqlDelete.WhereIn<PropertyDataDto>(x => x.VersionId, inSql);
            }

            sqlDelete
                .Where<PropertyDataDto>(x => x.LanguageId == null)
                .WhereIn<PropertyDataDto>(x => x.PropertyTypeId, propertyTypeIds);

            Database.Execute(sqlDelete);
        }
    }

    /// <summary>
    ///     Re-normalizes the edited value in the umbracoDocumentCultureVariation and umbracoDocument table when variations are
    ///     changed
    /// </summary>
    /// <param name="propertyTypeIds"></param>
    /// <param name="contentTypeIds"></param>
    /// <remarks>
    ///     If this is not done, then in some cases the "edited" value for a particular culture for a document will remain true
    ///     when it should be false
    ///     if the property was changed to invariant. In order to do this we need to recalculate this value based on the values
    ///     stored for each
    ///     property, culture and current/published version.
    /// </remarks>
    private void RenormalizeDocumentEditedFlags(
        IReadOnlyCollection<int> propertyTypeIds,
        IReadOnlyCollection<int>? contentTypeIds = null)
    {
        var defaultLang = LanguageRepository.GetDefaultId();

        // This will build up a query to get the property values of both the current and the published version so that we can check
        // based on the current variance of each item to see if it's 'edited' value should be true/false.
        var whereInArgsCount = propertyTypeIds.Count + (contentTypeIds?.Count ?? 0);
        if (whereInArgsCount > Constants.Sql.MaxParameterCount)
        {
            throw new NotSupportedException("Too many property/content types.");
        }

        Sql<ISqlContext> propertySql = Sql()
            .Select<PropertyDataDto>()
            .AndSelect<ContentVersionDto>(x => x.NodeId, x => x.Current)
            .AndSelect<DocumentVersionDto>(x => x.Published)
            .AndSelect<PropertyTypeDto>(x => x.Variations)
            .From<PropertyDataDto>()
            .InnerJoin<ContentVersionDto>()
            .On<ContentVersionDto, PropertyDataDto>((left, right) => left.Id == right.VersionId)
            .InnerJoin<PropertyTypeDto>()
            .On<PropertyTypeDto, PropertyDataDto>((left, right) => left.Id == right.PropertyTypeId);

        if (contentTypeIds != null)
        {
            propertySql.InnerJoin<ContentDto>()
                .On<ContentDto, ContentVersionDto>((c, cversion) => c.NodeId == cversion.NodeId);
        }

        propertySql.LeftJoin<DocumentVersionDto>()
            .On<DocumentVersionDto, ContentVersionDto>((docversion, cversion) => cversion.Id == docversion.Id)
            .Where<DocumentVersionDto, ContentVersionDto>((docversion, cversion) =>
                cversion.Current || docversion.Published)
            .WhereIn<PropertyDataDto>(x => x.PropertyTypeId, propertyTypeIds);

        if (contentTypeIds != null)
        {
            propertySql.WhereIn<ContentDto>(x => x.ContentTypeId, contentTypeIds);
        }

        propertySql
            .OrderBy<ContentVersionDto>(x => x.NodeId)
            .OrderBy<PropertyDataDto>(x => x.PropertyTypeId, x => x.LanguageId, x => x.VersionId);

        // keep track of this node/lang to mark or unmark a culture as edited
        var editedLanguageVersions = new Dictionary<(int nodeId, int? langId), bool>();

        // keep track of which node to mark or unmark as edited
        var editedDocument = new Dictionary<int, bool>();
        var nodeId = -1;
        var propertyTypeId = -1;

        PropertyValueVersionDto? pubRow = null;

        // This is a reader (Query), we are not fetching this all into memory so we cannot make any changes during this iteration, we are just collecting data.
        // Published data will always come before Current data based on the version id sort.
        // There will only be one published row (max) and one current row per property.
        foreach (PropertyValueVersionDto? row in Database.Query<PropertyValueVersionDto>(propertySql))
        {
            // make sure to reset on each node/property change
            if (nodeId != row.NodeId || propertyTypeId != row.PropertyTypeId)
            {
                nodeId = row.NodeId;
                propertyTypeId = row.PropertyTypeId;
                pubRow = null;
            }

            if (row.Published)
            {
                pubRow = row;
            }

            if (row.Current)
            {
                var propVariations = (ContentVariation)row.Variations;

                // if this prop doesn't vary but the row has a lang assigned or vice versa, flag this as not edited
                if ((!propVariations.VariesByCulture() && row.LanguageId.HasValue)
                    || (propVariations.VariesByCulture() && !row.LanguageId.HasValue))
                {
                    // Flag this as not edited for this node/lang if the key doesn't exist
                    if (!editedLanguageVersions.TryGetValue((row.NodeId, row.LanguageId), out _))
                    {
                        editedLanguageVersions.Add((row.NodeId, row.LanguageId), false);
                    }

                    // mark as false if the item doesn't exist, else coerce to true
                    editedDocument[row.NodeId] = editedDocument.TryGetValue(row.NodeId, out var edited)
                        ? edited |= false
                        : false;
                }
                else if (pubRow == null)
                {
                    // this would mean that that this property is 'edited' since there is no published version
                    editedLanguageVersions[(row.NodeId, row.LanguageId)] = true;
                    editedDocument[row.NodeId] = true;
                }

                // compare the property values, if they differ from versions then flag the current version as edited
                else if (IsPropertyValueChanged(pubRow, row))
                {
                    // Here we would check if the property is invariant, in which case the edited language should be indicated by the default lang
                    editedLanguageVersions[
                        (row.NodeId, !propVariations.VariesByCulture() ? defaultLang : row.LanguageId)] = true;
                    editedDocument[row.NodeId] = true;
                }

                // reset
                pubRow = null;
            }
        }

        // lookup all matching rows in umbracoDocumentCultureVariation
        // fetch in batches to account for maximum parameter count (distinct languages can't exceed 2000)
        var languageIds = editedLanguageVersions.Keys.Select(x => x.langId).Distinct().ToArray();
        IEnumerable<int> nodeIds = editedLanguageVersions.Keys.Select(x => x.nodeId).Distinct();
        var docCultureVariationsToUpdate = nodeIds.InGroupsOf(Constants.Sql.MaxParameterCount - languageIds.Length)
            .SelectMany(group =>
            {
                Sql<ISqlContext> sql = Sql().Select<DocumentCultureVariationDto>().From<DocumentCultureVariationDto>()
                    .WhereIn<DocumentCultureVariationDto>(x => x.LanguageId, languageIds)
                    .WhereIn<DocumentCultureVariationDto>(x => x.NodeId, group);

                return Database.Fetch<DocumentCultureVariationDto>(sql);
            })
            .ToDictionary(
                x => (x.NodeId, (int?)x.LanguageId),
                x => x); // convert to dictionary with the same key type

        var toUpdate = new List<DocumentCultureVariationDto>();
        foreach (KeyValuePair<(int nodeId, int? langId), bool> ev in editedLanguageVersions)
        {
            if (docCultureVariationsToUpdate.TryGetValue(ev.Key, out DocumentCultureVariationDto? docVariations))
            {
                // check if it needs updating
                if (docVariations.Edited != ev.Value)
                {
                    docVariations.Edited = ev.Value;
                    toUpdate.Add(docVariations);
                }
            }
            else if (ev.Key.langId.HasValue)
            {
                // This should never happen! If a property culture is flagged as edited then the culture must exist at the document level
                throw new PanicException(
                    $"The existing DocumentCultureVariationDto was not found for node {ev.Key.nodeId} and language {ev.Key.langId}");
            }
        }

        // Now bulk update the table DocumentCultureVariationDto, once for edited = true, another for edited = false
        foreach (IGrouping<bool, DocumentCultureVariationDto> editValue in toUpdate.GroupBy(x => x.Edited))
        {
            Database.Execute(Sql().Update<DocumentCultureVariationDto>(u => u.Set(x => x.Edited, editValue.Key))
                .WhereIn<DocumentCultureVariationDto>(x => x.Id, editValue.Select(x => x.Id)));
        }

        // Now bulk update the umbracoDocument table
        foreach (IGrouping<bool, KeyValuePair<int, bool>> editValue in editedDocument.GroupBy(x => x.Value))
        {
            Database.Execute(Sql().Update<DocumentDto>(u => u.Set(x => x.Edited, editValue.Key))
                .WhereIn<DocumentDto>(x => x.NodeId, editValue.Select(x => x.Key)));
        }
    }

    private void DeletePropertyType(int contentTypeId, int propertyTypeId)
    {
        // first clear dependencies
        Database.Delete<TagRelationshipDto>("WHERE propertyTypeId = @Id", new { Id = propertyTypeId });
        Database.Delete<PropertyDataDto>("WHERE propertyTypeId = @Id", new { Id = propertyTypeId });

        // then delete the property type
        Database.Delete<PropertyTypeDto>(
            "WHERE contentTypeId = @Id AND id = @PropertyTypeId",
            new { Id = contentTypeId, PropertyTypeId = propertyTypeId });
    }

    protected void ValidateAlias(TEntity entity)
    {
        if (string.IsNullOrWhiteSpace(entity.Alias))
        {
            var ex = new InvalidOperationException(
                $"{typeof(TEntity).Name} '{entity.Name}' cannot have an empty Alias. This is most likely due to invalid characters stripped from the Alias.");

            Logger.LogError(
                "{EntityTypeName} '{EntityName}' cannot have an empty Alias. This is most likely due to invalid characters stripped from the Alias.",
                typeof(TEntity).Name,
                entity.Name);

            throw ex;
        }
    }

    protected abstract TEntity? PerformGet(Guid id);

    /// <summary>
    ///     Try to set the data type id based on its ControlId
    /// </summary>
    /// <param name="propertyType"></param>
    private void AssignDataTypeFromPropertyEditor(IPropertyType propertyType)
    {
        // we cannot try to assign a data type of it's empty
        if (propertyType.PropertyEditorAlias.IsNullOrWhiteSpace() == false)
        {
            Sql<ISqlContext> sql = Sql()
                .Select<DataTypeDto>(dt => dt.Select(x => x.NodeDto))
                .From<DataTypeDto>()
                .InnerJoin<NodeDto>().On<DataTypeDto, NodeDto>((dt, n) => dt.NodeId == n.NodeId)
                .Where(
                    "propertyEditorAlias = @propertyEditorAlias",
                    new { propertyEditorAlias = propertyType.PropertyEditorAlias })
                .OrderBy<DataTypeDto>(typeDto => typeDto.NodeId);
            DataTypeDto? datatype = Database.FirstOrDefault<DataTypeDto>(sql);

            // we cannot assign a data type if one was not found
            if (datatype != null)
            {
                propertyType.DataTypeId = datatype.NodeId;
                propertyType.DataTypeKey = datatype.NodeDto.UniqueId;
            }
            else
            {
                Logger.LogWarning(
                    "Could not assign a data type for the property type {PropertyTypeAlias} since no data type was found with a property editor {PropertyEditorAlias}",
                    propertyType.Alias, propertyType.PropertyEditorAlias);
            }
        }
    }

    protected abstract TEntity? PerformGet(string alias);

    protected abstract IEnumerable<TEntity>? PerformGetAll(params Guid[]? ids);

    protected abstract bool PerformExists(Guid id);

    public string GetUniqueAlias(string alias)
    {
        // alias is unique across ALL content types!
        var aliasColumn = SqlSyntax.GetQuotedColumnName("alias");
        List<string>? aliases = Database.Fetch<string>(
            @"SELECT cmsContentType." + aliasColumn + @" FROM cmsContentType
INNER JOIN umbracoNode ON cmsContentType.nodeId = umbracoNode.id
WHERE cmsContentType." + aliasColumn + @" LIKE @pattern",
            new { pattern = alias + "%", objectType = NodeObjectTypeId });
        var i = 1;
        string test;
        while (aliases.Contains(test = alias + i))
        {
            i++;
        }

        return test;
    }

    public bool HasContainerInPath(string contentPath)
    {
        var ids = contentPath.Split(Constants.CharArrays.Comma)
            .Select(s => int.Parse(s, CultureInfo.InvariantCulture)).ToArray();
        return HasContainerInPath(ids);
    }

    public bool HasContainerInPath(params int[] ids)
    {
        var sql = new Sql(
            $@"SELECT COUNT(*) FROM cmsContentType
INNER JOIN {Constants.DatabaseSchema.Tables.Content} ON cmsContentType.nodeId={Constants.DatabaseSchema.Tables.Content}.contentTypeId
WHERE {Constants.DatabaseSchema.Tables.Content}.nodeId IN (@ids) AND cmsContentType.isContainer=@isContainer",
            new { ids, isContainer = true });
        return Database.ExecuteScalar<int>(sql) > 0;
    }

    /// <summary>
    ///     Returns true or false depending on whether content nodes have been created based on the provided content type id.
    /// </summary>
    public bool HasContentNodes(int id)
    {
        var sql = new Sql(
            $"SELECT CASE WHEN EXISTS (SELECT * FROM {Constants.DatabaseSchema.Tables.Content} WHERE contentTypeId = @id) THEN 1 ELSE 0 END",
            new { id });
        return Database.ExecuteScalar<int>(sql) == 1;
    }

    protected override IEnumerable<string> GetDeleteClauses()
    {
        // in theory, services should have ensured that content items of the given content type
        // have been deleted and therefore PropertyData has been cleared, so PropertyData
        // is included here just to be 100% sure since it has a FK on cmsPropertyType.
        var list = new List<string>
        {
            "DELETE FROM umbracoUser2NodeNotify WHERE nodeId = @id",
            "DELETE FROM umbracoUserGroup2Node WHERE nodeId = @id",
            "DELETE FROM umbracoUserGroup2NodePermission WHERE nodeId = @id",
            "DELETE FROM cmsTagRelationship WHERE nodeId = @id",
            "DELETE FROM cmsContentTypeAllowedContentType WHERE Id = @id",
            "DELETE FROM cmsContentTypeAllowedContentType WHERE AllowedId = @id",
            "DELETE FROM cmsContentType2ContentType WHERE parentContentTypeId = @id",
            "DELETE FROM cmsContentType2ContentType WHERE childContentTypeId = @id",
            "DELETE FROM " + Constants.DatabaseSchema.Tables.PropertyData +
            " WHERE propertyTypeId IN (SELECT id FROM cmsPropertyType WHERE contentTypeId = @id)",
            "DELETE FROM " + Constants.DatabaseSchema.Tables.PropertyType +
            " WHERE contentTypeId = @id",
            "DELETE FROM " + Constants.DatabaseSchema.Tables.PropertyTypeGroup +
            " WHERE contenttypeNodeId = @id",
        };
        return list;
    }

    private class NameCompareDto
    {
        public int NodeId { get; set; }

        public int CurrentVersion { get; set; }

        public int LanguageId { get; set; }

        public string? CurrentName { get; set; }

        public string? PublishedName { get; set; }

        public int? PublishedVersion { get; set; }

        public int Id { get; set; } // the Id of the DocumentCultureVariationDto

        public bool Edited { get; set; }
    }

    private class PropertyValueVersionDto
    {
        private decimal? _decimalValue;

        public int VersionId { get; set; }

        public int PropertyTypeId { get; set; }

        public int? LanguageId { get; set; }

        public string? Segment { get; set; }

        public int? IntValue { get; set; }

        [Column("decimalValue")]
        public decimal? DecimalValue
        {
            get => _decimalValue;
            set => _decimalValue = value?.Normalize();
        }

        public DateTime? DateValue { get; set; }

        public string? VarcharValue { get; set; }

        public string? TextValue { get; set; }

        public int NodeId { get; set; }

        public bool Current { get; set; }

        public bool Published { get; set; }

        public byte Variations { get; set; }
    }
}
