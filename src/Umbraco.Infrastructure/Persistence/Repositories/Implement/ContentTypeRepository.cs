using System.Data;
using System.Globalization;
using System.Xml.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Persistence.EFCore;
using Umbraco.Cms.Infrastructure.Persistence.EFCore.Scoping;
using Umbraco.Cms.Infrastructure.Persistence.Factories;
using Umbraco.Cms.Infrastructure.Persistence.Querying;
using Umbraco.Cms.Infrastructure.Persistence.SqlSyntax;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;
using EfContentType2ContentTypeDto = Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore.ContentType2ContentTypeDto;
using EfContentTypeAllowedContentTypeDto = Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore.ContentTypeAllowedContentTypeDto;
using EfContentTypeDto = Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore.ContentTypeDto;
using EfContentTypeTemplateDto = Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore.ContentTypeTemplateDto;
using EfContentVersionCleanupPolicyDto = Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore.ContentVersionCleanupPolicyDto;
using EfDataTypeDto = Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore.DataTypeDto;
using EfNodeDto = Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore.NodeDto;
using EfPropertyTypeDto = Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore.PropertyTypeDto;
using EfPropertyTypeGroupDto = Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore.PropertyTypeGroupDto;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

/// <summary>
///     Represents a repository for doing CRUD operations for <see cref="IContentType" />
/// </summary>
internal sealed class ContentTypeRepository : ContentTypeRepositoryBase<IContentType>, IContentTypeRepository
{
    private readonly IRepositoryCacheVersionService _repositoryCacheVersionService;
    private readonly ICacheSyncService _cacheSyncService;
    private readonly IEFCoreScopeAccessor<UmbracoDbContext> _efCoreScopeAccessor;
    private readonly IIdKeyMap _idKeyMap;

    /// <summary>
    /// Initializes a new instance of the <see cref="ContentTypeRepository"/> class.
    /// </summary>
    /// <param name="scopeAccessor">Provides access to the current database scope.</param>
    /// <param name="cache">The application-level cache manager.</param>
    /// <param name="logger">The logger used for logging repository operations.</param>
    /// <param name="commonRepository">Repository for common content type operations.</param>
    /// <param name="languageRepository">Repository for managing languages.</param>
    /// <param name="shortStringHelper">Helper for generating and manipulating short strings.</param>
    /// <param name="repositoryCacheVersionService">Service for managing repository cache versions.</param>
    /// <param name="idKeyMap">Service for mapping between IDs and keys.</param>
    /// <param name="cacheSyncService">Service for synchronizing cache across distributed environments.</param>
    /// <param name="efCoreScopeAccessor">Provides access to the current EF Core database scope.</param>
    public ContentTypeRepository(
        IScopeAccessor scopeAccessor,
        AppCaches cache,
        ILogger<ContentTypeRepository> logger,
        IContentTypeCommonRepository commonRepository,
        ILanguageRepository languageRepository,
        IShortStringHelper shortStringHelper,
        IRepositoryCacheVersionService repositoryCacheVersionService,
        IIdKeyMap idKeyMap,
        ICacheSyncService cacheSyncService,
        IEFCoreScopeAccessor<UmbracoDbContext> efCoreScopeAccessor)
        : base(
            scopeAccessor,
            cache,
            logger,
            commonRepository,
            languageRepository,
            shortStringHelper,
            repositoryCacheVersionService,
            idKeyMap,
            cacheSyncService)
    {
        _repositoryCacheVersionService = repositoryCacheVersionService;
        _cacheSyncService = cacheSyncService;
        _efCoreScopeAccessor = efCoreScopeAccessor;
        _idKeyMap = idKeyMap;
    }

    protected override bool SupportsPublishing => ContentType.SupportsPublishingConst;

    protected override Guid NodeObjectTypeId => Constants.ObjectTypes.DocumentType;

    /// <inheritdoc />
    public IEnumerable<IContentType> GetByQuery(IQuery<PropertyType> query)
    {
        var ints = PerformGetByQuery(query).ToArray();
        return ints.Length > 0 ? GetMany(ints) : Enumerable.Empty<IContentType>();
    }

    /// <summary>
    ///     Gets all property type aliases.
    /// </summary>
    /// <returns></returns>
    public IEnumerable<string> GetAllPropertyTypeAliases()
        => ExecuteEfScope(db => db.PropertyTypes
            .Select(x => x.Alias!)
            .Distinct()
            .OrderBy(x => x)
            .ToList());

    /// <summary>
    ///     Gets all content type aliases
    /// </summary>
    /// <param name="objectTypes">
    ///     If this list is empty, it will return all content type aliases for media, members and content, otherwise
    ///     it will only return content type aliases for the object types specified
    /// </param>
    /// <returns></returns>
    public IEnumerable<string> GetAllContentTypeAliases(params Guid[] objectTypes)
        => ExecuteEfScope(db =>
        {
            IQueryable<EfContentTypeDto> query = db.ContentTypes;

            if (objectTypes.Any())
            {
                query = query.Where(x =>
                    x.NodeDto.NodeObjectType.HasValue && objectTypes.Contains(x.NodeDto.NodeObjectType.Value));
            }

            return query.Select(x => x.Alias!).ToList();
        });

    /// <summary>
    /// Retrieves the IDs of all content types that match the specified aliases.
    /// </summary>
    /// <param name="aliases">An array of content type aliases for which to retrieve the corresponding IDs.</param>
    /// <returns>An <see cref="IEnumerable{Int32}"/> containing the IDs of content types that match the provided aliases. If no aliases are specified, returns an empty collection.</returns>
    public IEnumerable<int> GetAllContentTypeIds(string[] aliases)
    {
        if (aliases.Length == 0)
        {
            return Enumerable.Empty<int>();
        }

        return ExecuteEfScope(db => db.ContentTypes
            .Where(x => aliases.Contains(x.Alias))
            .Select(x => x.NodeId)
            .ToList());
    }

    protected override IRepositoryCachePolicy<IContentType, int> CreateCachePolicy() =>
        new FullDataSetRepositoryCachePolicy<IContentType, int>(GlobalIsolatedCache, ScopeAccessor, _repositoryCacheVersionService, _cacheSyncService, GetEntityId, /*expires:*/ true);

    // Note: PerformGet(int) is passed as a callback to the cache policy's Get(TId) method,
    // but FullDataSetRepositoryCachePolicy.Get() never invokes it — it uses GetAllCached()
    // internally and clones only the matched entity. This override exists only as a required
    // implementation of the abstract base and as a fallback for non-FullDataSet policies.
    protected override IContentType? PerformGet(int id)
        => GetMany().FirstOrDefault(x => x.Id == id);

    protected override IEnumerable<IContentType>? GetAllWithFullCachePolicy() =>
        CommonRepository.GetAllTypes()?.OfType<IContentType>();

    // The IQuery<T> read path deliberately stays on NPoco: IQuery translates predicates straight to SQL and
    // discards the expression, so it cannot be evaluated in-memory against the cached set, and the queries
    // are read-only (they fetch ids that are then hydrated from the FullDataSet cache). GetBaseQuery /
    // GetBaseWhereClause / PerformGetByQuery / GetByQuery remain NPoco for this reason.
    protected override IEnumerable<IContentType> PerformGetByQuery(IQuery<IContentType> query)
    {
        Sql<ISqlContext> baseQuery = GetBaseQuery(false);
        var translator = new SqlTranslator<IContentType>(baseQuery, query);
        Sql<ISqlContext> sql = translator.Translate();
        var ids = Database.Fetch<int>(sql).Distinct().ToArray();

        return ids.Length > 0
            ? GetMany(ids).OrderBy(x => x.Name)
            : Enumerable.Empty<IContentType>();
    }

    private IEnumerable<int> PerformGetByQuery(IQuery<PropertyType> query)
    {
        // used by DataTypeService to remove properties
        // from content types if they have a deleted data type - see
        // notes in DataTypeService.Delete as it's a bit weird
        Sql<ISqlContext> sqlClause = Sql()
            .SelectAll()
            .From<PropertyTypeDto>()
            .LeftJoin<PropertyTypeGroupDto>()
            .On<PropertyTypeGroupDto, PropertyTypeDto>(left => left.Id, right => right.PropertyTypeGroupId)
            .InnerJoin<DataTypeDto>()
            .On<PropertyTypeDto, DataTypeDto>(left => left.DataTypeId, right => right.NodeId);

        var translator = new SqlTranslator<PropertyType>(sqlClause, query);
        Sql<ISqlContext> sql = translator.Translate()
            .OrderBy<PropertyTypeDto>(x => x.PropertyTypeGroupId);

        return Database
            .FetchOneToMany<PropertyTypeGroupDto>(x => x.PropertyTypeDtos, sql)
            .Select(x => x.ContentTypeNodeId).Distinct();
    }

    protected override Sql<ISqlContext> GetBaseQuery(bool isCount)
    {
        Sql<ISqlContext> sql = Sql();

        sql = isCount
            ? sql.SelectCount()
            : sql.Select<ContentTypeDto>(x => x.NodeId);

        sql
            .From<ContentTypeDto>()
            .InnerJoin<NodeDto>().On<ContentTypeDto, NodeDto>(left => left.NodeId, right => right.NodeId)
            .LeftJoin<ContentTypeTemplateDto>()
            .On<ContentTypeTemplateDto, ContentTypeDto>(left => left.ContentTypeNodeId, right => right.NodeId)
            .Where<NodeDto>(x => x.NodeObjectType == NodeObjectTypeId);

        return sql;
    }

    protected override string GetBaseWhereClause() => $"{QuoteTableName(NodeDto.TableName)}.id = @id";

    /// <summary>
    ///     Deletes a content type
    /// </summary>
    /// <param name="entity"></param>
    /// <remarks>
    ///     First checks for children and removes those first
    /// </remarks>
    protected override void PersistDeletedItem(IContentType entity)
    {
        IQuery<IContentType> query = Query<IContentType>().Where(x => x.ParentId == entity.Id);
        IEnumerable<IContentType> children = Get(query);
        foreach (IContentType child in children)
        {
            PersistDeletedItem(child);
        }

        // The cascade tables below (property data, tags, notifications, granular permissions) belong to other
        // repositories and are not modelled in EF Core, so their cleanup stays on NPoco. They share the bridged
        // scope/transaction with the EF Core definition-table deletes that follow.

        // Before deleting the definition tables, clear any leftover property data linked to this content type.
        // Normally the ContentTypeService deletes the associated content first, but a document-type switch can
        // leave property data pointing at the previous type, so we ensure it is removed (FK on cmsPropertyType).
        Sql<ISqlContext> sql = Sql()
            .SelectDistinct<PropertyDataDto>(c => c.PropertyTypeId)
            .From<PropertyDataDto>()
            .InnerJoin<PropertyTypeDto>()
            .On<PropertyDataDto, PropertyTypeDto>(dto => dto.PropertyTypeId, dto => dto.Id)
            .InnerJoin<ContentTypeDto>()
            .On<ContentTypeDto, PropertyTypeDto>(dto => dto.NodeId, dto => dto.ContentTypeId)
            .Where<ContentTypeDto>(dto => dto.NodeId == entity.Id);

        // Delete all PropertyData where propertytypeid EXISTS in the subquery above
        Database.Execute(SqlSyntax.GetDeleteSubquery(PropertyDataDto.TableName, "propertyTypeId", sql));

        Database.Execute(
            $"DELETE FROM {QuoteTableName(User2NodeNotifyDto.TableName)} WHERE {QuoteColumnName(User2NodeNotifyDto.NodeIdColumnName)} = @id",
            new { id = entity.Id });
        Database.Execute(
            $"DELETE FROM {QuoteTableName(TagRelationshipDto.TableName)} WHERE {QuoteColumnName(TagRelationshipDto.NodeIdColumnName)} = @id",
            new { id = entity.Id });

        // delete all granular permissions for this content type
        Database.Delete<UserGroup2GranularPermissionDto>(Sql().Where<UserGroup2GranularPermissionDto>(dto => dto.UniqueId == entity.Key));

        // Definition tables modelled in EF Core.
        PersistDeletedBaseContentTypeEFCore(entity);

        entity.DeleteDate = DateTime.UtcNow;
        CommonRepository.ClearCache(); // always
    }

    /// <summary>
    /// EF Core implementation of the content-type-definition delete path. Removes the cmsPropertyType +
    /// cmsPropertyTypeGroup + cmsContentTypeAllowedContentType + cmsContentType2ContentType + cmsDocumentType +
    /// umbracoContentVersionCleanupPolicy + cmsContentType + umbracoNode rows through the EF Core
    /// <see cref="UmbracoDbContext"/>, in FK-safe order (children before parents). The cascade tables not
    /// modelled in EF Core are removed by the caller via NPoco. <c>ExecuteDelete</c> is set-based and bypasses
    /// the change tracker, so it never conflicts with entities the shared bridged context already tracks.
    /// </summary>
    private void PersistDeletedBaseContentTypeEFCore(IContentType entity)
    {
        var id = entity.Id;
        ExecuteEfScope(db =>
        {
            db.PropertyTypes.Where(x => x.ContentTypeId == id).ExecuteDelete();
            db.PropertyTypeGroups.Where(x => x.ContentTypeNodeId == id).ExecuteDelete();
            db.ContentTypeAllowedContentTypes.Where(x => x.Id == id || x.AllowedId == id).ExecuteDelete();
            db.ContentTypeComposition.Where(x => x.ParentId == id || x.ChildId == id).ExecuteDelete();
            db.ContentTypeTemplates.Where(x => x.ContentTypeNodeId == id).ExecuteDelete();
            db.ContentVersionCleanupPolicies.Where(x => x.ContentTypeId == id).ExecuteDelete();
            db.ContentTypes.Where(x => x.NodeId == id).ExecuteDelete();
            db.Nodes.Where(x => x.NodeId == id).ExecuteDelete();
        });
    }

    protected override void PersistNewItem(IContentType entity)
    {
        if (string.IsNullOrWhiteSpace(entity.Alias))
        {
            var ex = new Exception(
                $"ContentType '{entity.Name}' cannot have an empty Alias. This is most likely due to invalid characters stripped from the Alias.");
            Logger.LogError(
                "ContentType '{EntityName}' cannot have an empty Alias. This is most likely due to invalid characters stripped from the Alias.",
                entity.Name);
            throw ex;
        }

        entity.AddingEntity();

        PersistNewBaseContentTypeEFCore(entity);
        PersistTemplates(entity, false);
        PersistHistoryCleanup(entity);

        entity.ResetDirtyProperties();
    }

    private void PersistTemplates(IContentType entity, bool clearAll)
    {
        var defaultTemplateId = entity.DefaultTemplateId;
        ITemplate[] allowedTemplates = entity.AllowedTemplates?.Where(x => x.Id != defaultTemplateId).ToArray()
                                       ?? Array.Empty<ITemplate>();

        ExecuteEfScope(db =>
        {
            // remove and re-insert. ExecuteDelete is set-based and bypasses the change tracker.
            db.ContentTypeTemplates.Where(x => x.ContentTypeNodeId == entity.Id).ExecuteDelete();

            // we could do it all in foreach if we assume that the default template is an allowed template??
            if (defaultTemplateId > 0)
            {
                db.ContentTypeTemplates.Add(new EfContentTypeTemplateDto
                {
                    ContentTypeNodeId = entity.Id,
                    TemplateNodeId = defaultTemplateId,
                    IsDefault = true,
                });
            }

            foreach (ITemplate template in allowedTemplates)
            {
                db.ContentTypeTemplates.Add(new EfContentTypeTemplateDto
                {
                    ContentTypeNodeId = entity.Id,
                    TemplateNodeId = template.Id,
                    IsDefault = false,
                });
            }

            db.SaveChanges();
        });
    }

    protected override void PersistUpdatedItem(IContentType entity)
    {
        ValidateAlias(entity);

        // Updates Modified date
        entity.UpdatingEntity();

        // Look up parent to get and set the correct Path if ParentId has changed
        if (entity.IsPropertyDirty("ParentId"))
        {
            var parent = ExecuteEfScope(db => db.Nodes
                .Where(x => x.NodeId == entity.ParentId)
                .Select(x => new { x.Path, x.Level })
                .First());
            entity.Path = string.Concat(parent.Path, ",", entity.Id);
            entity.Level = parent.Level + 1;

            var maxSortOrder = ExecuteEfScope(db => db.Nodes
                .Where(x => x.ParentId == entity.ParentId && x.NodeObjectType == NodeObjectTypeId)
                .Select(x => (int?)x.SortOrder)
                .Max()) ?? 0;
            entity.SortOrder = maxSortOrder + 1;
        }

        PersistUpdatedBaseContentTypeEFCore(entity);
        PersistTemplates(entity, true);
        PersistHistoryCleanup(entity);

        entity.ResetDirtyProperties();
    }

    private void PersistHistoryCleanup(IContentType entity)
    {
        // historyCleanup property is not mandatory for api endpoint, handle the case where it's not present.
        // DocumentTypeSave doesn't handle this for us like ContentType constructors do.
        if (entity is IContentType entityWithHistoryCleanup)
        {
            var updated = DateTime.UtcNow;
            var preventCleanup = entityWithHistoryCleanup.HistoryCleanup?.PreventCleanup ?? false;
            var keepAllVersionsNewerThanDays = entityWithHistoryCleanup.HistoryCleanup?.KeepAllVersionsNewerThanDays;
            var keepLatestVersionPerDayForDays = entityWithHistoryCleanup.HistoryCleanup?.KeepLatestVersionPerDayForDays;

            ExecuteEfScope(db =>
            {
                // Upsert: ExecuteUpdate is set-based (no change-tracker conflict); insert only if absent.
                var affected = db.ContentVersionCleanupPolicies
                    .Where(x => x.ContentTypeId == entity.Id)
                    .ExecuteUpdate(s => s
                        .SetProperty(x => x.Updated, updated)
                        .SetProperty(x => x.PreventCleanup, preventCleanup)
                        .SetProperty(x => x.KeepAllVersionsNewerThanDays, keepAllVersionsNewerThanDays)
                        .SetProperty(x => x.KeepLatestVersionPerDayForDays, keepLatestVersionPerDayForDays));

                if (affected == 0)
                {
                    db.ContentVersionCleanupPolicies.Add(new EfContentVersionCleanupPolicyDto
                    {
                        ContentTypeId = entity.Id,
                        Updated = updated,
                        PreventCleanup = preventCleanup,
                        KeepAllVersionsNewerThanDays = keepAllVersionsNewerThanDays,
                        KeepLatestVersionPerDayForDays = keepLatestVersionPerDayForDays,
                    });
                    db.SaveChanges();
                }
            });
        }
    }

    /// <summary>
    /// EF Core implementation of the content-type-definition insert path. Mirrors
    /// <see cref="ContentTypeRepositoryBase{TEntity}.PersistNewBaseContentType"/> but writes the
    /// umbracoNode + cmsContentType + composition + allowed-type + property group/type rows through
    /// the EF Core <see cref="UmbracoDbContext"/>. Runs in the ambient (bridged) scope so it shares the
    /// same transaction as the NPoco writes that follow (templates, history cleanup).
    /// </summary>
    private void PersistNewBaseContentTypeEFCore(IContentTypeComposition entity)
    {
        ValidateVariations(entity);

        // Reuse the existing NPoco factory to build the DTO shape, then translate to EF Core DTOs.
        ContentTypeDto npocoDto = ContentTypeFactory.BuildContentTypeDto(entity);
        (IContentType Entity, int SortOrder)[] allowedContentTypes = GetAllowedContentTypes(entity);

        ExecuteEfScope(db =>
        {
            // Cannot add a duplicate content type
            var exists = db.ContentTypes
                .Count(ct => ct.Alias == entity.Alias && ct.NodeDto.NodeObjectType == NodeObjectTypeId);
            if (exists > 0)
            {
                throw new DuplicateNameException("An item with the alias " + entity.Alias + " already exists");
            }

            // Logic for setting Path, Level and SortOrder
            EfNodeDto? parent = db.Nodes.FirstOrDefault(x => x.NodeId == entity.ParentId);
            var level = (parent?.Level ?? 0) + 1;
            var sortOrder = db.Nodes.Count(x => x.ParentId == entity.ParentId && x.NodeObjectType == NodeObjectTypeId);

            // Create the (base) node data - umbracoNode
            EfNodeDto nodeDto = ToEfNodeDto(npocoDto.NodeDto);
            nodeDto.Path = parent?.Path ?? Constants.System.RootString;
            nodeDto.Level = short.Parse(level.ToString(CultureInfo.InvariantCulture));
            nodeDto.SortOrder = sortOrder;
            db.Nodes.Add(nodeDto);
            db.SaveChanges();

            // Update with new correct path
            nodeDto.Path = string.Concat(nodeDto.Path, ",", nodeDto.NodeId);
            db.SaveChanges();

            // Update entity with correct values
            entity.Id = nodeDto.NodeId;
            entity.Path = nodeDto.Path;
            entity.SortOrder = sortOrder;
            entity.Level = level;

            // Insert new ContentType entry
            EfContentTypeDto contentTypeDto = ToEfContentTypeDto(npocoDto);
            contentTypeDto.NodeId = nodeDto.NodeId;
            db.ContentTypes.Add(contentTypeDto);
            db.SaveChanges();

            // Insert ContentType composition
            foreach (IContentTypeComposition composition in entity.ContentTypeComposition)
            {
                if (composition.Id == entity.Id)
                {
                    continue; // Just to ensure that we aren't creating a reference to ourself.
                }

                if (composition.HasIdentity)
                {
                    db.ContentTypeComposition.Add(new EfContentType2ContentTypeDto { ParentId = composition.Id, ChildId = entity.Id });
                }
                else
                {
                    // Fallback for ContentTypes with no identity
                    var parentId = db.ContentTypes
                        .Where(x => x.Alias == composition.Alias)
                        .Select(x => (int?)x.NodeId)
                        .FirstOrDefault();
                    if (parentId.HasValue)
                    {
                        db.ContentTypeComposition.Add(new EfContentType2ContentTypeDto { ParentId = parentId.Value, ChildId = entity.Id });
                    }
                }
            }

            db.SaveChanges();

            // Insert collection of allowed content types
            foreach ((IContentType allowedEntity, int allowedSortOrder) in allowedContentTypes)
            {
                db.ContentTypeAllowedContentTypes.Add(new EfContentTypeAllowedContentTypeDto
                {
                    Id = entity.Id,
                    AllowedId = allowedEntity.Id,
                    SortOrder = allowedSortOrder,
                });
            }

            db.SaveChanges();

            // Insert Tabs
            foreach (PropertyGroup propertyGroup in entity.PropertyGroups)
            {
                EfPropertyTypeGroupDto tabDto = ToEfPropertyTypeGroupDto(PropertyGroupFactory.BuildGroupDto(propertyGroup, nodeDto.NodeId));
                db.PropertyTypeGroups.Add(tabDto);
                db.SaveChanges();
                propertyGroup.Id = tabDto.Id; // Set Id on PropertyGroup

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
                    AssignDataTypeIdFromProvidedKeyOrPropertyEditor(db, propertyType);
                }

                EfPropertyTypeDto propertyTypeDto = ToEfPropertyTypeDto(PropertyGroupFactory.BuildPropertyTypeDto(tabId, propertyType, nodeDto.NodeId));
                db.PropertyTypes.Add(propertyTypeDto);
                db.SaveChanges();
                propertyType.Id = propertyTypeDto.Id; // Set Id on new PropertyType

                // Update the current PropertyType with correct PropertyEditorAlias and DatabaseType
                EfDataTypeDto? dataTypeDto = db.DataTypes.FirstOrDefault(x => x.NodeId == propertyType.DataTypeId);
                if (dataTypeDto is not null)
                {
                    propertyType.PropertyEditorAlias = dataTypeDto.EditorAlias;
                    propertyType.ValueStorageType = dataTypeDto.DbType.EnumParse<ValueStorageType>(true);
                }
            }
        });

        CommonRepository.ClearCache(); // always
    }

    /// <summary>
    /// EF Core implementation of the content-type-definition update path. Mirrors
    /// <see cref="ContentTypeRepositoryBase{TEntity}.PersistUpdatedBaseContentType"/> but writes the
    /// umbracoNode + cmsContentType + composition + allowed-type + property group/type rows through EF Core.
    /// The variation cascade and content-instance cleanup (removed-composition property data, variant data
    /// movement, redirects, scheduled publishing, property-type deletion) stay on NPoco via the base helpers,
    /// sharing the same bridged scope/transaction.
    /// </summary>
    private void PersistUpdatedBaseContentTypeEFCore(IContentTypeComposition entity)
    {
        CorrectPropertyTypeVariations(entity);
        ValidateVariations(entity);

        // Reuse the existing NPoco factory to build the DTO shape, then translate to EF Core DTOs.
        ContentTypeDto dto = ContentTypeFactory.BuildContentTypeDto(entity);

        ContentVariation oldContentTypeVariation = default;

        ExecuteEfScope(db =>
        {
            // The bridged context is shared across repository operations within the ambient scope and
            // accumulates tracked entities from prior inserts/saves (everything prior has already been
            // saved). Clear it so the junction-table re-inserts below (compositions, allowed types) don't
            // collide with already-tracked instances carrying the same composite key.
            db.ChangeTracker.Clear();

            // ensure the alias is not used already
            var exists = db.ContentTypes.Count(ct =>
                ct.Alias == dto.Alias
                && ct.NodeDto.NodeObjectType == NodeObjectTypeId
                && ct.NodeDto.NodeId != dto.NodeId);
            if (exists > 0)
            {
                throw new DuplicateNameException("An item with the alias " + dto.Alias + " already exists");
            }

            // capture the current (old) variation, needed below to decide whether the content type
            // variation is changing.
            oldContentTypeVariation = (ContentVariation)db.ContentTypes
                .Where(x => x.NodeId == dto.NodeId)
                .Select(x => x.Variations)
                .First();

            // handle (update) the node. ExecuteUpdate is set-based and does not use the change tracker,
            // so it never conflicts with entities the shared bridged context already tracks (e.g. from a
            // prior insert in the same scope).
            NodeDto nodeDto = dto.NodeDto;
            db.Nodes.Where(x => x.NodeId == nodeDto.NodeId).ExecuteUpdate(s => s
                .SetProperty(x => x.UniqueId, nodeDto.UniqueId)
                .SetProperty(x => x.ParentId, nodeDto.ParentId)
                .SetProperty(x => x.Level, (short)nodeDto.Level)
                .SetProperty(x => x.Path, nodeDto.Path)
                .SetProperty(x => x.SortOrder, nodeDto.SortOrder)
                .SetProperty(x => x.Trashed, nodeDto.Trashed)
                .SetProperty(x => x.UserId, nodeDto.UserId)
                .SetProperty(x => x.Text, nodeDto.Text)
                .SetProperty(x => x.NodeObjectType, nodeDto.NodeObjectType)
                .SetProperty(x => x.CreateDate, nodeDto.CreateDate));

            // handle (update) the ContentType
            db.ContentTypes.Where(x => x.NodeId == dto.NodeId).ExecuteUpdate(s => s
                .SetProperty(x => x.Alias, dto.Alias)
                .SetProperty(x => x.Icon, dto.Icon)
                .SetProperty(x => x.Thumbnail, dto.Thumbnail)
                .SetProperty(x => x.Description, dto.Description)
                .SetProperty(x => x.ListView, dto.ListView)
                .SetProperty(x => x.IsElement, dto.IsElement)
                .SetProperty(x => x.AllowedInLibrary, dto.AllowedInLibrary)
                .SetProperty(x => x.AllowAtRoot, dto.AllowAtRoot)
                .SetProperty(x => x.Variations, dto.Variations));

            // handle (delete then recreate) compositions
            db.ContentTypeComposition.Where(x => x.ChildId == entity.Id).ExecuteDelete();
            foreach (IContentTypeComposition composition in entity.ContentTypeComposition)
            {
                db.ContentTypeComposition.Add(new EfContentType2ContentTypeDto { ParentId = composition.Id, ChildId = entity.Id });
            }

            db.SaveChanges();
        });

        // removing a ContentType from a composition (U4-1690): content-instance cleanup stays on NPoco
        ClearPropertyDataForRemovedContentTypes(entity);

        // delete the allowed content type entries before re-inserting the collection of allowed content types
        ExecuteEfScope(db => db.ContentTypeAllowedContentTypes.Where(x => x.Id == entity.Id).ExecuteDelete());

        (IContentType Entity, int SortOrder)[] allowedContentTypes = GetAllowedContentTypes(entity);
        if (allowedContentTypes.Any())
        {
            ExecuteEfScope(db =>
            {
                foreach ((IContentType allowedEntity, int sortOrder) in allowedContentTypes)
                {
                    db.ContentTypeAllowedContentTypes.Add(new EfContentTypeAllowedContentTypeDto
                    {
                        Id = entity.Id,
                        AllowedId = allowedEntity.Id,
                        SortOrder = sortOrder,
                    });
                }

                db.SaveChanges();
            });
        }

        // Delete property types that have been removed from the entity's collections.
        if (entity.IsPropertyDirty("NoGroupPropertyTypes") ||
            entity.PropertyGroups.Any(x => x.IsPropertyDirty("PropertyTypes")))
        {
            List<int> dbPropertyTypeIds = ExecuteEfScope(db => db.PropertyTypes
                .Where(x => x.ContentTypeId == entity.Id)
                .Select(x => x.Id)
                .ToList());
            IEnumerable<int> entityPropertyTypes = entity.PropertyTypes.Where(x => x.HasIdentity).Select(x => x.Id);
            IEnumerable<int> propertyTypeToDeleteIds = dbPropertyTypeIds.Except(entityPropertyTypes);
            foreach (var propertyTypeId in propertyTypeToDeleteIds)
            {
                DeletePropertyType(entity, propertyTypeId); // NPoco: clears tag/property-data/permission dependencies too
            }
        }

        // Delete tabs that do not exist anymore (orphaning their property types onto 'generic properties').
        List<int>? orphanPropertyTypeIds = null;
        if (entity.IsPropertyDirty("PropertyGroups"))
        {
            ExecuteEfScope(db =>
            {
                List<int> existingPropertyGroups = db.PropertyTypeGroups
                    .Where(x => x.ContentTypeNodeId == entity.Id)
                    .Select(x => x.Id)
                    .ToList();

                var newPropertyGroups = entity.PropertyGroups.Select(x => x.Id).ToList();
                var groupsToDelete = existingPropertyGroups.Except(newPropertyGroups).ToArray();

                if (groupsToDelete.Length > 0)
                {
                    // if the tab contains properties, move them to 'generic properties' (group = null)
                    // and keep track of them so leftover orphans can be removed later.
                    orphanPropertyTypeIds = db.PropertyTypes
                        .Where(x => x.PropertyTypeGroupId != null && groupsToDelete.Contains(x.PropertyTypeGroupId.Value))
                        .Select(x => x.Id)
                        .ToList();
                    db.PropertyTypes
                        .Where(x => x.PropertyTypeGroupId != null && groupsToDelete.Contains(x.PropertyTypeGroupId.Value))
                        .ExecuteUpdate(s => s.SetProperty(x => x.PropertyTypeGroupId, (int?)null));
                    db.PropertyTypeGroups.Where(x => groupsToDelete.Contains(x.Id)).ExecuteDelete();
                }
            });
        }

        // insert or update groups, assign properties
        ExecuteEfScope(db =>
        {
            foreach (PropertyGroup propertyGroup in entity.PropertyGroups)
            {
                EfPropertyTypeGroupDto groupDto = ToEfPropertyTypeGroupDto(PropertyGroupFactory.BuildGroupDto(propertyGroup, entity.Id));
                int groupId;
                if (propertyGroup.HasIdentity)
                {
                    db.PropertyTypeGroups.Where(x => x.Id == propertyGroup.Id).ExecuteUpdate(s => s
                        .SetProperty(x => x.UniqueId, groupDto.UniqueId)
                        .SetProperty(x => x.ContentTypeNodeId, groupDto.ContentTypeNodeId)
                        .SetProperty(x => x.Type, groupDto.Type)
                        .SetProperty(x => x.Text, groupDto.Text)
                        .SetProperty(x => x.Alias, groupDto.Alias)
                        .SetProperty(x => x.SortOrder, groupDto.SortOrder));
                    groupId = propertyGroup.Id;
                }
                else
                {
                    db.PropertyTypeGroups.Add(groupDto);
                    db.SaveChanges();
                    groupId = groupDto.Id;
                    propertyGroup.Id = groupId;
                }

                // assign properties to the group
                if (propertyGroup.PropertyTypes is not null)
                {
                    foreach (IPropertyType propertyType in propertyGroup.PropertyTypes)
                    {
                        propertyType.PropertyGroupId = new Lazy<int>(() => groupId);
                    }
                }
            }
        });

        // check if the content type variation has been changed (content-instance cascade stays on NPoco)
        var contentTypeVariationDirty = entity.IsPropertyDirty("Variations");
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
        foreach (IPropertyType propertyType in entity.PropertyTypes)
        {
            if (propertyType.IsPropertyDirty("Variations"))
            {
                propertyTypeVariationDirty ??= new List<IPropertyType>();
                propertyTypeVariationDirty.Add(propertyType);
            }
        }

        // figure out dirty property types that have actually changed, before we insert or update
        // properties, so we can read the old variations
        Dictionary<int, (ContentVariation FromVariation, ContentVariation ToVariation)>? propertyTypeVariationChanges =
            propertyTypeVariationDirty != null
                ? GetPropertyVariationChanges(propertyTypeVariationDirty)
                : null;

        // deal with composition property types that change due to this content type variations change
        if (contentTypeVariationChanging)
        {
            propertyTypeVariationChanges ??= new Dictionary<int, (ContentVariation, ContentVariation)>();

            foreach (IPropertyType composedPropertyType in entity.GetOriginalComposedPropertyTypes())
            {
                if (composedPropertyType.Variations == ContentVariation.Nothing)
                {
                    continue;
                }

                ContentVariation target = newContentTypeVariation & composedPropertyType.Variations;
                ContentVariation from = oldContentTypeVariation & composedPropertyType.Variations;

                propertyTypeVariationChanges[composedPropertyType.Id] = (from, target);
            }
        }

        // insert or update properties (all of them, no-group and in-groups)
        ExecuteEfScope(db =>
        {
            foreach (IPropertyType propertyType in entity.PropertyTypes)
            {
                // if the Id of the DataType is not set, we resolve it from the db by its PropertyEditorAlias
                if (propertyType.DataTypeId == 0 || propertyType.DataTypeId == default)
                {
                    AssignDataTypeIdFromProvidedKeyOrPropertyEditor(db, propertyType);
                }

                ValidateAlias(propertyType);

                var groupId = propertyType.PropertyGroupId?.Value ?? default;
                EfPropertyTypeDto propertyTypeDto = ToEfPropertyTypeDto(PropertyGroupFactory.BuildPropertyTypeDto(groupId, propertyType, entity.Id));
                int typeId;
                if (propertyType.HasIdentity)
                {
                    db.PropertyTypes.Where(x => x.Id == propertyType.Id).ExecuteUpdate(s => s
                        .SetProperty(x => x.DataTypeId, propertyTypeDto.DataTypeId)
                        .SetProperty(x => x.ContentTypeId, propertyTypeDto.ContentTypeId)
                        .SetProperty(x => x.PropertyTypeGroupId, propertyTypeDto.PropertyTypeGroupId)
                        .SetProperty(x => x.Alias, propertyTypeDto.Alias)
                        .SetProperty(x => x.Name, propertyTypeDto.Name)
                        .SetProperty(x => x.SortOrder, propertyTypeDto.SortOrder)
                        .SetProperty(x => x.Mandatory, propertyTypeDto.Mandatory)
                        .SetProperty(x => x.MandatoryMessage, propertyTypeDto.MandatoryMessage)
                        .SetProperty(x => x.ValidationRegExp, propertyTypeDto.ValidationRegExp)
                        .SetProperty(x => x.ValidationRegExpMessage, propertyTypeDto.ValidationRegExpMessage)
                        .SetProperty(x => x.Description, propertyTypeDto.Description)
                        .SetProperty(x => x.LabelOnTop, propertyTypeDto.LabelOnTop)
                        .SetProperty(x => x.Variations, propertyTypeDto.Variations)
                        .SetProperty(x => x.UniqueId, propertyTypeDto.UniqueId));
                    typeId = propertyType.Id;
                }
                else
                {
                    db.PropertyTypes.Add(propertyTypeDto);
                    db.SaveChanges();
                    typeId = propertyTypeDto.Id;
                    propertyType.Id = typeId;
                }

                // not an orphan anymore
                orphanPropertyTypeIds?.Remove(typeId);
            }
        });

        // restrict property data changes to impacted content types (content-instance cascade stays on NPoco)
        IEnumerable<IContentType>? all = PerformGetAll();
        IEnumerable<IContentTypeComposition> impacted = GetImpactedContentTypes(entity, all);

        if (propertyTypeVariationChanges?.Count > 0)
        {
            MovePropertyTypeVariantData(propertyTypeVariationChanges, impacted);
        }

        // deal with orphan properties: those that were in a deleted tab and have not been re-mapped
        if (orphanPropertyTypeIds != null)
        {
            foreach (var id in orphanPropertyTypeIds)
            {
                DeletePropertyType(entity, id);
            }
        }

        CommonRepository.ClearCache(); // always
    }

    /// <summary>
    /// EF Core variant of the base's data-type resolution: tries the key via <see cref="IIdKeyMap"/>,
    /// then falls back to looking up a data type by its property editor alias.
    /// </summary>
    private void AssignDataTypeIdFromProvidedKeyOrPropertyEditor(UmbracoDbContext db, IPropertyType propertyType)
    {
        // If a key is provided, use that.
        if (propertyType.DataTypeKey != Guid.Empty)
        {
            Attempt<int> dataTypeIdAttempt = _idKeyMap.GetIdForKey(propertyType.DataTypeKey, UmbracoObjectTypes.DataType);
            if (dataTypeIdAttempt.Success)
            {
                propertyType.DataTypeId = dataTypeIdAttempt.Result;
                return;
            }

            Logger.LogWarning(
                "Could not assign a data type for the property type {PropertyTypeAlias} since no integer Id was found matching the key {DataTypeKey}. Falling back to look up via the property editor alias.",
                propertyType.Alias,
                propertyType.DataTypeKey);
        }

        // Otherwise if a property editor alias is provided, try to find a data type that uses that alias.
        if (propertyType.PropertyEditorAlias.IsNullOrWhiteSpace())
        {
            // We cannot try to assign a data type if it's empty.
            return;
        }

        var dataType = db.DataTypes
            .Where(x => x.EditorAlias == propertyType.PropertyEditorAlias)
            .OrderBy(x => x.NodeId)
            .Select(x => new { x.NodeId, x.NodeDto.UniqueId })
            .FirstOrDefault();

        if (dataType is not null)
        {
            propertyType.DataTypeId = dataType.NodeId;
            propertyType.DataTypeKey = dataType.UniqueId;
        }
        else
        {
            Logger.LogWarning(
                "Could not assign a data type for the property type {PropertyTypeAlias} since no data type was found with a property editor {PropertyEditorAlias}",
                propertyType.Alias,
                propertyType.PropertyEditorAlias);
        }
    }

    /// <summary>
    /// Executes the given action against the ambient EF Core <see cref="UmbracoDbContext"/>, bridging the
    /// synchronous repository contract onto the async-only scope API. The action runs synchronously so
    /// there is no real await and no deadlock risk.
    /// </summary>
    private void ExecuteEfScope(Action<UmbracoDbContext> action)
        => ExecuteEfScope(db =>
        {
            action(db);
            return true;
        });

    private T ExecuteEfScope<T>(Func<UmbracoDbContext, T> func)
    {
        IEFCoreScope<UmbracoDbContext> scope = _efCoreScopeAccessor.AmbientScope
            ?? throw new InvalidOperationException("Cannot run a repository without an ambient EF Core scope.");

        return scope.ExecuteWithContextAsync(db => Task.FromResult(func(db))).GetAwaiter().GetResult();
    }

    private static EfNodeDto ToEfNodeDto(NodeDto dto) => new()
    {
        NodeId = dto.NodeId,
        UniqueId = dto.UniqueId,
        ParentId = dto.ParentId,
        Level = dto.Level,
        Path = dto.Path,
        SortOrder = dto.SortOrder,
        Trashed = dto.Trashed,
        UserId = dto.UserId,
        Text = dto.Text,
        NodeObjectType = dto.NodeObjectType,
        CreateDate = dto.CreateDate,
    };

    private static EfContentTypeDto ToEfContentTypeDto(ContentTypeDto dto) => new()
    {
        NodeId = dto.NodeId,
        Alias = dto.Alias,
        Icon = dto.Icon,
        Thumbnail = dto.Thumbnail,
        Description = dto.Description,
        ListView = dto.ListView,
        IsElement = dto.IsElement,
        AllowedInLibrary = dto.AllowedInLibrary,
        AllowAtRoot = dto.AllowAtRoot,
        Variations = dto.Variations,
    };

    private static EfPropertyTypeGroupDto ToEfPropertyTypeGroupDto(PropertyTypeGroupDto dto) => new()
    {
        UniqueId = dto.UniqueId,
        ContentTypeNodeId = dto.ContentTypeNodeId,
        Type = dto.Type,
        Text = dto.Text,
        Alias = dto.Alias,
        SortOrder = dto.SortOrder,
    };

    private static EfPropertyTypeDto ToEfPropertyTypeDto(PropertyTypeDto dto) => new()
    {
        DataTypeId = dto.DataTypeId,
        ContentTypeId = dto.ContentTypeId,
        PropertyTypeGroupId = dto.PropertyTypeGroupId,
        Alias = dto.Alias,
        Name = dto.Name,
        SortOrder = dto.SortOrder,
        Mandatory = dto.Mandatory,
        MandatoryMessage = dto.MandatoryMessage,
        ValidationRegExp = dto.ValidationRegExp,
        ValidationRegExpMessage = dto.ValidationRegExpMessage,
        Description = dto.Description,
        LabelOnTop = dto.LabelOnTop,
        Variations = dto.Variations,
        UniqueId = dto.UniqueId,
    };
}
