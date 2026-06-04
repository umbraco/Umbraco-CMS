using System.Data;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.Cache;
using Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore;
using Umbraco.Cms.Infrastructure.Persistence.EFCore;
using Umbraco.Cms.Infrastructure.Persistence.EFCore.Scoping;
using Umbraco.Cms.Infrastructure.Persistence.Factories;
using Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement.EFCore;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

/// <summary>
///     Represents a repository for doing CRUD operations for <see cref="IContentType" />.
/// </summary>
/// <remarks>
///     This repository runs entirely on the EF Core <see cref="UmbracoDbContext"/> (async base) while still
///     exposing the synchronous <see cref="IContentTypeRepository"/> contract that the shared content-type
///     service layer calls; the sync members bridge to the async base. Tables not modelled in EF Core
///     (content-instance tables owned by other repositories) are accessed with EF raw SQL on the same context.
/// </remarks>
internal sealed partial class ContentTypeRepository : AsyncEntityRepositoryBase<Guid, IContentType>, IContentTypeRepository
{
    private readonly IShortStringHelper _shortStringHelper;
    private readonly IIdKeyMap _idKeyMap;

    /// <summary>
    /// Initializes a new instance of the <see cref="ContentTypeRepository"/> class.
    /// </summary>
    public ContentTypeRepository(
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
            efCoreScopeAccessor,
            cache,
            logger,
            repositoryCacheVersionService,
            cacheSyncService)
    {
        CommonRepository = commonRepository;
        LanguageRepository = languageRepository;
        _shortStringHelper = shortStringHelper;
        _idKeyMap = idKeyMap;
    }

    private IContentTypeCommonRepository CommonRepository { get; }

    private ILanguageRepository LanguageRepository { get; }

    private static Guid NodeObjectTypeId => Constants.ObjectTypes.DocumentType;

    // ----------------------------------------------------------------------------------------------------
    // Async EF Core base overrides + cache policy. Bodies are synchronous-in-substance (the EF work runs
    // synchronously and is returned via completed tasks), so the sync bridge below never blocks on real I/O.
    // ----------------------------------------------------------------------------------------------------

    /// <inheritdoc />
    protected override IAsyncRepositoryCachePolicy<IContentType, Guid> CreateCachePolicy()
        => new AsyncFullDataSetRepositoryCachePolicy<IContentType, Guid>(
            GlobalIsolatedCache,
            ScopeAccessor,
            RepositoryCacheVersionService,
            CacheSyncService,
            entity => entity.Key,
            /*expires:*/ true);

    /// <inheritdoc />
    protected override Task<IContentType?> PerformGetAsync(Guid key)
        => Task.FromResult(GetAllCached().FirstOrDefault(x => x.Key == key));

    /// <inheritdoc />
    protected override Task<IEnumerable<IContentType>?> PerformGetAllAsync()
        => Task.FromResult(CommonRepository.GetAllTypes()?.OfType<IContentType>());

    /// <inheritdoc />
    protected override Task<IEnumerable<IContentType>?> PerformGetManyAsync(Guid[]? keys)
    {
        IEnumerable<IContentType> all = GetAllCached();
        if (keys is { Length: > 0 })
        {
            var set = keys.ToHashSet();
            all = all.Where(x => set.Contains(x.Key));
        }

        return Task.FromResult<IEnumerable<IContentType>?>(all);
    }

    /// <inheritdoc />
    protected override Task<bool> PerformExistsAsync(Guid key)
        => Task.FromResult(GetAllCached().Any(x => x.Key == key));

    /// <inheritdoc />
    protected override Task PersistNewItemAsync(IContentType item)
    {
        PersistNewItem(item);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    protected override Task PersistUpdatedItemAsync(IContentType item)
    {
        PersistUpdatedItem(item);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    protected override Task PersistDeletedItemAsync(IContentType entity)
    {
        PersistDeletedItem(entity);
        return Task.CompletedTask;
    }

    // ----------------------------------------------------------------------------------------------------
    // Synchronous IContentTypeRepository contract (consumed by the shared sync content-type service base).
    // Reads are served from the FullDataSet cache; writes/deletes bridge to the async base.
    // ----------------------------------------------------------------------------------------------------

    private IEnumerable<IContentType> GetAllCached()
        => GetAllAsync(CancellationToken.None).GetAwaiter().GetResult();

    /// <inheritdoc />
    public IContentType? Get(int id)
        => GetAllCached().FirstOrDefault(x => x.Id == id);

    /// <inheritdoc />
    public IEnumerable<IContentType> GetMany(params int[]? ids)
    {
        IEnumerable<IContentType> all = GetAllCached();
        return ids is { Length: > 0 } ? all.Where(x => ids.Contains(x.Id)) : all;
    }

    /// <inheritdoc />
    public bool Exists(int id) => GetAllCached().Any(x => x.Id == id);

    /// <inheritdoc />
    public IContentType? Get(Guid id)
        => GetAllCached().FirstOrDefault(x => x.Key == id);

    /// <remarks>
    ///     Explicit implementation to disambiguate from the <see cref="int"/>-keyed <see cref="GetMany(int[])"/>.
    /// </remarks>
    IEnumerable<IContentType> IReadRepository<Guid, IContentType>.GetMany(params Guid[]? ids)
    {
        IEnumerable<IContentType> all = GetAllCached();
        return ids is { Length: > 0 } ? all.Where(x => ids.Contains(x.Key)) : all;
    }

    /// <inheritdoc />
    public bool Exists(Guid id) => GetAllCached().Any(x => x.Key == id);

    /// <inheritdoc />
    public IContentType? Get(string alias)
        => GetAllCached().FirstOrDefault(x => x.Alias.InvariantEquals(alias));

    /// <inheritdoc />
    public void Save(IContentType entity)
        => SaveAsync(entity, CancellationToken.None).GetAwaiter().GetResult();

    /// <inheritdoc />
    public void Delete(IContentType entity)
        => DeleteAsync(entity, CancellationToken.None).GetAwaiter().GetResult();

    /// <inheritdoc />
    public IEnumerable<IContentType> Get(IQuery<IContentType> query)
        => PerformGetByQuery(query).WhereNotNull();

    /// <inheritdoc />
    public int Count(IQuery<IContentType>? query) => PerformCount(query);

    /// <inheritdoc />
    public IEnumerable<IContentType> GetByQuery(IQuery<PropertyType> query)
    {
        var ints = PerformGetByQuery(query).ToArray();
        return ints.Length > 0 ? GetMany(ints) : Enumerable.Empty<IContentType>();
    }

    /// <summary>
    ///     Gets all property type aliases.
    /// </summary>
    public IEnumerable<string> GetAllPropertyTypeAliases()
        => ExecuteEfScope(db => db.PropertyTypes
            .Select(x => x.Alias!)
            .Distinct()
            .OrderBy(x => x)
            .ToList());

    /// <summary>
    ///     Gets all content type aliases.
    /// </summary>
    public IEnumerable<string> GetAllContentTypeAliases(params Guid[] objectTypes)
        => ExecuteEfScope(db =>
        {
            IQueryable<ContentTypeDto> query = db.ContentTypes;

            if (objectTypes.Any())
            {
                query = query.Where(x =>
                    x.NodeDto.NodeObjectType.HasValue && objectTypes.Contains(x.NodeDto.NodeObjectType.Value));
            }

            return query.Select(x => x.Alias!).ToList();
        });

    /// <summary>
    ///     Retrieves the IDs of all content types that match the specified aliases.
    /// </summary>
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

    // ----------------------------------------------------------------------------------------------------
    // IQuery read path — the where clauses captured by the IQuery (SQL text + args) are appended to a
    // hand-written id-select and executed by EF Core as raw SQL; matching entities are then hydrated from
    // the cached full set.
    // ----------------------------------------------------------------------------------------------------
    private IEnumerable<IContentType> PerformGetByQuery(IQuery<IContentType> query)
    {
        // note: Guid values must be bound as parameters (not string literals) — providers store/bind them
        // in provider-specific formats (e.g. uppercase TEXT on SQLite, uniqueidentifier on SQL Server).
        (var whereSql, var clauseArgs) = TranslateWhereClauses(query.GetWhereClauses(), parameterOffset: 1);
        var p0 = "{0}";
        var sql =
            $"""
             SELECT DISTINCT {ContentTypeDto.TableName}.{ContentTypeDto.NodeIdColumnName} AS Value
             FROM {ContentTypeDto.TableName}
             INNER JOIN {NodeDto.TableName} ON {ContentTypeDto.TableName}.{ContentTypeDto.NodeIdColumnName} = {NodeDto.TableName}.{NodeDto.PrimaryKeyColumnName}
             LEFT JOIN {ContentTypeTemplateDto.TableName} ON {ContentTypeTemplateDto.TableName}.{ContentTypeTemplateDto.ContentTypeNodeIdColumnName} = {ContentTypeDto.TableName}.{ContentTypeDto.NodeIdColumnName}
             WHERE {NodeDto.TableName}.{NodeDto.NodeObjectTypeColumnName} = {p0}{whereSql}
             """;
        var args = new object[clauseArgs.Length + 1];
        args[0] = NodeObjectTypeId;
        clauseArgs.CopyTo(args, 1);

        List<int> ids = ExecuteEfScope(db => db.Database.SqlQueryRaw<int>(sql, args).ToList());

        return ids.Count > 0
            ? GetMany(ids.ToArray()).OrderBy(x => x.Name)
            : Enumerable.Empty<IContentType>();
    }

    private IEnumerable<int> PerformGetByQuery(IQuery<PropertyType> query)
    {
        // used by DataTypeService to remove properties from content types if they have a deleted data type.
        // Matches the legacy behaviour of resolving the content type through the property GROUP — ungrouped
        // property types resolve to 0 and are filtered out.
        (var whereSql, var args) = TranslateWhereClauses(query.GetWhereClauses());
        var sql =
            $"""
             SELECT DISTINCT COALESCE({PropertyTypeGroupDto.TableName}.{PropertyTypeGroupDto.ContentTypeNodeIdColumnName}, 0) AS Value
             FROM {PropertyTypeDto.TableName}
             LEFT JOIN {PropertyTypeGroupDto.TableName} ON {PropertyTypeGroupDto.TableName}.{PropertyTypeGroupDto.PrimaryKeyColumnName} = {PropertyTypeDto.TableName}.{PropertyTypeDto.PropertyTypeGroupIdColumnName}
             INNER JOIN {DataTypeDto.TableName} ON {PropertyTypeDto.TableName}.{PropertyTypeDto.DataTypeIdColumnName} = {DataTypeDto.TableName}.nodeId
             WHERE 1 = 1{whereSql}
             """;

        return ExecuteEfScope(db => db.Database.SqlQueryRaw<int>(sql, args).ToList())
            .Where(id => id > 0)
            .Distinct();
    }

    private int PerformCount(IQuery<IContentType>? query)
    {
        (var whereSql, var clauseArgs) = query is null
            ? (string.Empty, Array.Empty<object>())
            : TranslateWhereClauses(query.GetWhereClauses(), parameterOffset: 1);
        var p0 = "{0}";
        var sql =
            $"""
             SELECT COUNT(*) AS Value
             FROM {ContentTypeDto.TableName}
             INNER JOIN {NodeDto.TableName} ON {ContentTypeDto.TableName}.{ContentTypeDto.NodeIdColumnName} = {NodeDto.TableName}.{NodeDto.PrimaryKeyColumnName}
             LEFT JOIN {ContentTypeTemplateDto.TableName} ON {ContentTypeTemplateDto.TableName}.{ContentTypeTemplateDto.ContentTypeNodeIdColumnName} = {ContentTypeDto.TableName}.{ContentTypeDto.NodeIdColumnName}
             WHERE {NodeDto.TableName}.{NodeDto.NodeObjectTypeColumnName} = {p0}{whereSql}
             """;
        var args = new object[clauseArgs.Length + 1];
        args[0] = NodeObjectTypeId;
        clauseArgs.CopyTo(args, 1);

        return ExecuteEfScope(db => db.Database.SqlQueryRaw<int>(sql, args).Single());
    }

    /// <summary>
    /// Translates the SQL where clauses captured by an <see cref="IQuery{T}"/> into a fragment that EF Core
    /// can execute as raw SQL: each clause's local <c>@N</c> parameter placeholders are renumbered into EF's
    /// positional <c>{N}</c> form, offset by the parameters already present in the surrounding statement and
    /// by the parameters of the preceding clauses.
    /// </summary>
    private static (string Sql, object[] Args) TranslateWhereClauses(
        IEnumerable<Tuple<string, object[]>> clauses,
        int parameterOffset = 0)
    {
        var sb = new StringBuilder();
        var args = new List<object>();
        foreach (Tuple<string, object[]> clause in clauses)
        {
            var offset = parameterOffset + args.Count;
            var clauseSql = Regex.Replace(
                clause.Item1,
                @"@(\d+)",
                m => "{" + (int.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture) + offset) + "}");
            sb.Append(" AND (").Append(clauseSql).Append(')');
            args.AddRange(clause.Item2);
        }

        return (sb.ToString(), args.ToArray());
    }

    // ----------------------------------------------------------------------------------------------------
    // Persist (sync) — invoked by the async Persist*ItemAsync wrappers above.
    // ----------------------------------------------------------------------------------------------------

    /// <summary>
    ///     Deletes a content type. First checks for children and removes those first.
    /// </summary>
    private void PersistDeletedItem(IContentType entity)
    {
        IContentType[] children = GetAllCached().Where(x => x.ParentId == entity.Id).ToArray();
        foreach (IContentType child in children)
        {
            PersistDeletedItem(child);
        }

        // The cascade tables below (property data, notifications, tags, granular permissions) belong to other
        // repositories and are not modelled in EF Core — raw SQL on the shared context.
        ExecuteEfScope(db =>
        {
            // Before deleting the definition tables, clear any leftover property data linked to this content
            // type. A document-type switch can leave property data pointing at the previous type (FK on cmsPropertyType).
            db.Database.ExecuteSqlRaw(
                $"""
                 DELETE FROM {Constants.DatabaseSchema.Tables.PropertyData} WHERE propertyTypeId IN (
                 SELECT {PropertyTypeDto.PrimaryKeyColumnName} FROM {PropertyTypeDto.TableName} WHERE {PropertyTypeDto.ContentTypeIdColumnName} = {entity.Id})
                 """);

            db.Database.ExecuteSqlRaw(
                $"DELETE FROM {Constants.DatabaseSchema.Tables.User2NodeNotify} WHERE nodeId = {entity.Id}");
            db.Database.ExecuteSqlRaw(
                $"DELETE FROM {Constants.DatabaseSchema.Tables.TagRelationship} WHERE nodeId = {entity.Id}");

            // delete all granular permissions for this content type
            db.Database.ExecuteSqlRaw(
                $"DELETE FROM {Constants.DatabaseSchema.Tables.UserGroup2GranularPermission} WHERE uniqueId = {{0}}",
                entity.Key);
        });

        // Definition tables modelled in EF Core.
        PersistDeletedBaseContentTypeEFCore(entity);

        entity.DeleteDate = DateTime.UtcNow;
        CommonRepository.ClearCache(); // always
    }

    /// <summary>
    /// EF Core implementation of the content-type-definition delete path. Removes the cmsPropertyType +
    /// cmsPropertyTypeGroup + cmsContentTypeAllowedContentType + cmsContentType2ContentType + cmsDocumentType +
    /// umbracoContentVersionCleanupPolicy + cmsContentType + umbracoNode rows in FK-safe order. <c>ExecuteDelete</c>
    /// is set-based and bypasses the change tracker.
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

    private void PersistNewItem(IContentType entity)
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

            if (defaultTemplateId > 0)
            {
                db.ContentTypeTemplates.Add(new ContentTypeTemplateDto
                {
                    ContentTypeNodeId = entity.Id,
                    TemplateNodeId = defaultTemplateId,
                    IsDefault = true,
                });
            }

            foreach (ITemplate template in allowedTemplates)
            {
                db.ContentTypeTemplates.Add(new ContentTypeTemplateDto
                {
                    ContentTypeNodeId = entity.Id,
                    TemplateNodeId = template.Id,
                    IsDefault = false,
                });
            }

            db.SaveChanges();
        });
    }

    private void PersistUpdatedItem(IContentType entity)
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
                    db.ContentVersionCleanupPolicies.Add(new ContentVersionCleanupPolicyDto
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
    /// EF Core implementation of the content-type-definition insert path: writes the umbracoNode +
    /// cmsContentType + composition + allowed-type + property group/type rows through the EF Core
    /// <see cref="UmbracoDbContext"/>.
    /// </summary>
    private void PersistNewBaseContentTypeEFCore(IContentTypeComposition entity)
    {
        ValidateVariations(entity);

        ContentTypeDto contentTypeDto = ContentTypeFactory.BuildEFCoreContentTypeDto(entity);
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
            NodeDto? parent = db.Nodes.FirstOrDefault(x => x.NodeId == entity.ParentId);
            var level = (parent?.Level ?? 0) + 1;
            var sortOrder = db.Nodes.Count(x => x.ParentId == entity.ParentId && x.NodeObjectType == NodeObjectTypeId);

            // Create the (base) node data - umbracoNode
            NodeDto nodeDto = contentTypeDto.NodeDto;
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
                    db.ContentTypeComposition.Add(new ContentType2ContentTypeDto { ParentId = composition.Id, ChildId = entity.Id });
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
                        db.ContentTypeComposition.Add(new ContentType2ContentTypeDto { ParentId = parentId.Value, ChildId = entity.Id });
                    }
                }
            }

            db.SaveChanges();

            // Insert collection of allowed content types
            foreach ((IContentType allowedEntity, int allowedSortOrder) in allowedContentTypes)
            {
                db.ContentTypeAllowedContentTypes.Add(new ContentTypeAllowedContentTypeDto
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
                PropertyTypeGroupDto tabDto = PropertyGroupFactory.BuildEFCoreGroupDto(propertyGroup, nodeDto.NodeId);
                db.PropertyTypeGroups.Add(tabDto);
                db.SaveChanges();
                propertyGroup.Id = tabDto.Id; // Set Id on PropertyGroup

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

                if (propertyType.DataTypeId == 0 || propertyType.DataTypeId == default)
                {
                    AssignDataTypeIdFromProvidedKeyOrPropertyEditor(db, propertyType);
                }

                PropertyTypeDto propertyTypeDto = PropertyGroupFactory.BuildEFCorePropertyTypeDto(tabId, propertyType, nodeDto.NodeId);
                db.PropertyTypes.Add(propertyTypeDto);
                db.SaveChanges();
                propertyType.Id = propertyTypeDto.Id; // Set Id on new PropertyType

                DataTypeDto? dataTypeDto = db.DataTypes.FirstOrDefault(x => x.NodeId == propertyType.DataTypeId);
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
    /// EF Core implementation of the content-type-definition update path. The variation cascade and
    /// content-instance cleanup run as EF raw SQL (variant engine), sharing the same bridged scope.
    /// </summary>
    private void PersistUpdatedBaseContentTypeEFCore(IContentTypeComposition entity)
    {
        CorrectPropertyTypeVariations(entity);
        ValidateVariations(entity);

        ContentTypeDto dto = ContentTypeFactory.BuildEFCoreContentTypeDto(entity);

        ContentVariation oldContentTypeVariation = default;

        ExecuteEfScope(db =>
        {
            // The bridged context is shared across repository operations within the ambient scope and accumulates
            // tracked entities from prior inserts/saves. Clear it so the junction-table re-inserts below don't
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

            // capture the current (old) variation
            oldContentTypeVariation = (ContentVariation)db.ContentTypes
                .Where(x => x.NodeId == dto.NodeId)
                .Select(x => x.Variations)
                .First();

            // handle (update) the node (ExecuteUpdate is set-based and tracking-free)
            NodeDto nodeDto = dto.NodeDto;
            db.Nodes.Where(x => x.NodeId == nodeDto.NodeId).ExecuteUpdate(s => s
                .SetProperty(x => x.UniqueId, nodeDto.UniqueId)
                .SetProperty(x => x.ParentId, nodeDto.ParentId)
                .SetProperty(x => x.Level, nodeDto.Level)
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
                db.ContentTypeComposition.Add(new ContentType2ContentTypeDto { ParentId = composition.Id, ChildId = entity.Id });
            }

            db.SaveChanges();
        });

        // removing a ContentType from a composition (U4-1690): content-instance cleanup (EF raw SQL)
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
                    db.ContentTypeAllowedContentTypes.Add(new ContentTypeAllowedContentTypeDto
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
                DeletePropertyType(entity, propertyTypeId); // also clears tag/property-data/permission dependencies
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
                PropertyTypeGroupDto groupDto = PropertyGroupFactory.BuildEFCoreGroupDto(propertyGroup, entity.Id);
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

                if (propertyGroup.PropertyTypes is not null)
                {
                    foreach (IPropertyType propertyType in propertyGroup.PropertyTypes)
                    {
                        propertyType.PropertyGroupId = new Lazy<int>(() => groupId);
                    }
                }
            }
        });

        // check if the content type variation has been changed (content-instance cascade runs as EF raw SQL)
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

        Dictionary<int, (ContentVariation FromVariation, ContentVariation ToVariation)>? propertyTypeVariationChanges =
            propertyTypeVariationDirty != null
                ? GetPropertyVariationChanges(propertyTypeVariationDirty)
                : null;

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
                if (propertyType.DataTypeId == 0 || propertyType.DataTypeId == default)
                {
                    AssignDataTypeIdFromProvidedKeyOrPropertyEditor(db, propertyType);
                }

                ValidateAlias(propertyType);

                var groupId = propertyType.PropertyGroupId?.Value ?? default;
                PropertyTypeDto propertyTypeDto = PropertyGroupFactory.BuildEFCorePropertyTypeDto(groupId, propertyType, entity.Id);
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

        // restrict property data changes to impacted content types (content-instance cascade runs as EF raw SQL)
        IEnumerable<IContentType> all = GetAllCached();
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
    /// EF Core variant of the data-type resolution: tries the key via <see cref="IIdKeyMap"/>, then falls back to
    /// looking up a data type by its property editor alias.
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

        if (propertyType.PropertyEditorAlias.IsNullOrWhiteSpace())
        {
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
    /// synchronous repository contract onto the async-only scope API. The action runs synchronously so there is
    /// no real await and no deadlock risk.
    /// </summary>
    private void ExecuteEfScope(Action<UmbracoDbContext> action)
        => ExecuteEfScope(db =>
        {
            action(db);
            return true;
        });

    private T ExecuteEfScope<T>(Func<UmbracoDbContext, T> func)
        => AmbientScope.ExecuteWithContextAsync(db => Task.FromResult(func(db))).GetAwaiter().GetResult();

}
