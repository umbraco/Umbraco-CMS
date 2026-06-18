using System.Data;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Exceptions;
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
using Umbraco.Extensions;

// EF1002: the raw SQL below interpolates only compile-time identifier constants (table/column names) and
// server-generated integer ids — never user-controlled strings. All caller-supplied values (Guids, object-type
// ids, IQuery clause arguments) are bound through the positional parameter array. Identifiers cannot be passed as
// parameters, so ExecuteSqlInterpolated is not an option; the queries are not vulnerable to SQL injection.
#pragma warning disable EF1002 // Risk of vulnerability to SQL injection.

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement.EFCore;

/// <summary>
///     Provides the shared functionality for the content-type composition repositories (document, media and member
///     types), running entirely on the EF Core <see cref="UmbracoDbContext"/> (async base) while still exposing the
///     synchronous <see cref="IContentTypeRepositoryBase{TEntity}"/> contract that the content-type service layer
///     calls; the sync members bridge to the async base.
/// </summary>
/// <remarks>
///     Derived repositories supply the entity-specific <see cref="NodeObjectTypeId"/> and <see cref="SupportsPublishing"/>,
///     implement the <see cref="PersistNewItem"/> / <see cref="PersistUpdatedItem"/> orchestration (adding any entity
///     specific tables such as templates), and may delete entity-specific definition tables via
///     <see cref="DeleteContentTypeSpecificDefinitionTables"/>. Tables not modelled in EF Core (content-instance tables
///     owned by other repositories) are accessed with EF raw SQL on the same context.
/// </remarks>
/// <typeparam name="TEntity">The type of content-type composition managed by the repository.</typeparam>
internal abstract class AsyncContentTypeRepositoryBase<TEntity> : AsyncEntityRepositoryBase<Guid, TEntity>,
    IContentTypeRepositoryBase<TEntity>
    where TEntity : class, IContentTypeComposition
{
    private readonly IShortStringHelper _shortStringHelper;
    private readonly IIdKeyMap _idKeyMap;

    /// <summary>
    /// Initializes a new instance of the <see cref="AsyncContentTypeRepositoryBase{TEntity}"/> class.
    /// </summary>
    protected AsyncContentTypeRepositoryBase(
        AppCaches cache,
        ILogger<AsyncContentTypeRepositoryBase<TEntity>> logger,
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

    /// <summary>
    /// Gets the common repository that holds the cached, hydrated set of all content-type compositions.
    /// </summary>
    protected IContentTypeCommonRepository CommonRepository { get; }

    /// <summary>
    /// Gets the language repository.
    /// </summary>
    protected ILanguageRepository LanguageRepository { get; }

    /// <summary>
    /// Gets the node object type for the repository's entity.
    /// </summary>
    protected abstract Guid NodeObjectTypeId { get; }

    /// <summary>
    /// Gets a value indicating whether the repository's entity supports publishing.
    /// </summary>
    protected abstract bool SupportsPublishing { get; }

    // ----------------------------------------------------------------------------------------------------
    // Async EF Core base overrides + cache policy. Bodies are synchronous-in-substance (the EF work runs
    // synchronously and is returned via completed tasks), so the sync bridge below never blocks on real I/O.
    // ----------------------------------------------------------------------------------------------------

    /// <inheritdoc />
    protected override IAsyncRepositoryCachePolicy<TEntity, Guid> CreateCachePolicy()
        => new AsyncFullDataSetRepositoryCachePolicy<TEntity, Guid>(
            GlobalIsolatedCache,
            ScopeAccessor,
            RepositoryCacheVersionService,
            CacheSyncService,
            entity => entity.Key,
            /*expires:*/ true);

    /// <inheritdoc />
    protected override Task<TEntity?> PerformGetAsync(Guid key)
        => Task.FromResult(GetAllCached().FirstOrDefault(x => x.Key == key));

    /// <inheritdoc />
    protected override Task<IEnumerable<TEntity>?> PerformGetAllAsync()
        => Task.FromResult(CommonRepository.GetAllTypes()?.OfType<TEntity>());

    /// <inheritdoc />
    protected override Task<IEnumerable<TEntity>?> PerformGetManyAsync(Guid[]? keys)
    {
        IEnumerable<TEntity> all = GetAllCached();
        if (keys is { Length: > 0 })
        {
            var set = keys.ToHashSet();
            all = all.Where(x => set.Contains(x.Key));
        }

        return Task.FromResult<IEnumerable<TEntity>?>(all);
    }

    /// <inheritdoc />
    protected override Task<bool> PerformExistsAsync(Guid key)
        => Task.FromResult(GetAllCached().Any(x => x.Key == key));

    /// <inheritdoc />
    protected override Task PersistNewItemAsync(TEntity item)
    {
        PersistNewItem(item);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    protected override Task PersistUpdatedItemAsync(TEntity item)
    {
        PersistUpdatedItem(item);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    protected override Task PersistDeletedItemAsync(TEntity entity)
    {
        PersistDeletedItem(entity);
        return Task.CompletedTask;
    }

    // ----------------------------------------------------------------------------------------------------
    // Entity-specific persistence hooks implemented by derived repositories.
    // ----------------------------------------------------------------------------------------------------

    /// <summary>
    /// Persists a new content type, including any entity-specific tables. Derived repositories typically call
    /// <see cref="PersistNewBaseContentTypeEFCore"/> for the shared definition tables.
    /// </summary>
    protected abstract void PersistNewItem(TEntity entity);

    /// <summary>
    /// Persists an updated content type, including any entity-specific tables. Derived repositories typically call
    /// <see cref="PersistUpdatedBaseContentTypeEFCore"/> for the shared definition tables.
    /// </summary>
    protected abstract void PersistUpdatedItem(TEntity entity);

    /// <summary>
    /// Deletes entity-specific definition tables (e.g. templates, version cleanup policies) for the given content
    /// type id, before the shared <c>cmsContentType</c> + <c>umbracoNode</c> rows are removed. The default is a no-op.
    /// </summary>
    /// <param name="db">The ambient EF Core context.</param>
    /// <param name="id">The content type node id.</param>
    protected virtual void DeleteContentTypeSpecificDefinitionTables(UmbracoDbContext db, int id)
    {
    }

    // ----------------------------------------------------------------------------------------------------
    // Synchronous IContentTypeRepositoryBase contract (consumed by the shared sync content-type service base).
    // Reads are served from the FullDataSet cache; writes/deletes bridge to the async base.
    // ----------------------------------------------------------------------------------------------------

    private IEnumerable<TEntity> GetAllCached()
    {
        EnsureAmbientScopeOnCallerContext();
        return GetAllAsync(CancellationToken.None).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Touches the ambient EF Core scope on the synchronous caller's execution context before entering an
    /// async member. AsyncLocal changes made inside an async method are invisible to its synchronous caller,
    /// so if the NPoco bridge scope were first created inside the async flow, the ambient scope stack
    /// bookkeeping would diverge from the bridge's scope-context cleanup, leaving a stale (disposed) scope
    /// ambient for subsequent operations.
    /// </summary>
    private void EnsureAmbientScopeOnCallerContext() => _ = AmbientScope;

    /// <inheritdoc />
    public TEntity? Get(int id)
        => GetAllCached().FirstOrDefault(x => x.Id == id);

    /// <inheritdoc />
    public IEnumerable<TEntity> GetMany(params int[]? ids)
    {
        IEnumerable<TEntity> all = GetAllCached();
        return ids is { Length: > 0 } ? all.Where(x => ids.Contains(x.Id)) : all;
    }

    /// <inheritdoc />
    public bool Exists(int id) => GetAllCached().Any(x => x.Id == id);

    /// <inheritdoc />
    public TEntity? Get(Guid id)
        => GetAllCached().FirstOrDefault(x => x.Key == id);

    /// <remarks>
    ///     Explicit implementation to disambiguate from the <see cref="int"/>-keyed <see cref="GetMany(int[])"/>.
    /// </remarks>
    IEnumerable<TEntity> IReadRepository<Guid, TEntity>.GetMany(params Guid[]? ids)
    {
        IEnumerable<TEntity> all = GetAllCached();
        return ids is { Length: > 0 } ? all.Where(x => ids.Contains(x.Key)) : all;
    }

    /// <inheritdoc />
    public bool Exists(Guid id) => GetAllCached().Any(x => x.Key == id);

    /// <inheritdoc />
    public TEntity? Get(string alias)
        => GetAllCached().FirstOrDefault(x => x.Alias.InvariantEquals(alias));

    /// <inheritdoc />
    public void Save(TEntity entity)
    {
        EnsureAmbientScopeOnCallerContext();
        SaveAsync(entity, CancellationToken.None).GetAwaiter().GetResult();
    }

    /// <inheritdoc />
    public void Delete(TEntity entity)
    {
        EnsureAmbientScopeOnCallerContext();
        DeleteAsync(entity, CancellationToken.None).GetAwaiter().GetResult();
    }

    /// <inheritdoc />
    public IEnumerable<TEntity> Get(IQuery<TEntity> query)
        => PerformGetByQuery(query).WhereNotNull();

    /// <inheritdoc />
    public int Count(IQuery<TEntity>? query) => PerformCount(query);

    // ----------------------------------------------------------------------------------------------------
    // IQuery read path — the where clauses captured by the IQuery (SQL text + args) are appended to a
    // hand-written id-select and executed by EF Core as raw SQL; matching entities are then hydrated from
    // the cached full set.
    // ----------------------------------------------------------------------------------------------------
    private IEnumerable<TEntity> PerformGetByQuery(IQuery<TEntity> query)
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
            : Enumerable.Empty<TEntity>();
    }

    /// <summary>
    /// Translates the SQL where clauses captured by an <see cref="IQuery{T}"/> into a fragment that EF Core
    /// can execute as raw SQL: each clause's local <c>@N</c> parameter placeholders are renumbered into EF's
    /// positional <c>{N}</c> form, offset by the parameters already present in the surrounding statement and
    /// by the parameters of the preceding clauses.
    /// </summary>
    protected static (string Sql, object[] Args) TranslateWhereClauses(
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

    private int PerformCount(IQuery<TEntity>? query)
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

    // ----------------------------------------------------------------------------------------------------
    // Delete (sync) — invoked by the async PersistDeletedItemAsync wrapper above.
    // ----------------------------------------------------------------------------------------------------

    /// <summary>
    ///     Deletes a content type. First checks for children and removes those first.
    /// </summary>
    protected virtual void PersistDeletedItem(TEntity entity)
    {
        TEntity[] children = GetAllCached().Where(x => x.ParentId == entity.Id).ToArray();
        foreach (TEntity child in children)
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
    /// cmsPropertyTypeGroup + cmsContentTypeAllowedContentType + cmsContentType2ContentType rows, then any
    /// entity-specific definition tables (via <see cref="DeleteContentTypeSpecificDefinitionTables"/>), then the
    /// cmsContentType + umbracoNode rows in FK-safe order. <c>ExecuteDelete</c> is set-based and bypasses the
    /// change tracker.
    /// </summary>
    private void PersistDeletedBaseContentTypeEFCore(TEntity entity)
    {
        var id = entity.Id;
        ExecuteEfScope(db =>
        {
            db.PropertyTypes.Where(x => x.ContentTypeId == id).ExecuteDelete();
            db.PropertyTypeGroups.Where(x => x.ContentTypeNodeId == id).ExecuteDelete();
            db.ContentTypeAllowedContentTypes.Where(x => x.Id == id || x.AllowedId == id).ExecuteDelete();
            db.ContentTypeComposition.Where(x => x.ParentId == id || x.ChildId == id).ExecuteDelete();
            DeleteContentTypeSpecificDefinitionTables(db, id);
            db.ContentTypes.Where(x => x.NodeId == id).ExecuteDelete();
            db.Nodes.Where(x => x.NodeId == id).ExecuteDelete();
        });
    }

    // ----------------------------------------------------------------------------------------------------
    // Shared persistence of the content-type definition tables (umbracoNode + cmsContentType + composition +
    // allowed types + property groups/types), called by the derived PersistNewItem / PersistUpdatedItem.
    // ----------------------------------------------------------------------------------------------------

    /// <summary>
    /// EF Core implementation of the content-type-definition insert path: writes the umbracoNode +
    /// cmsContentType + composition + allowed-type + property group/type rows through the EF Core
    /// <see cref="UmbracoDbContext"/>.
    /// </summary>
    protected void PersistNewBaseContentTypeEFCore(IContentTypeComposition entity)
    {
        ValidateVariations(entity);

        ContentTypeDto contentTypeDto = ContentTypeFactory.BuildEFCoreContentTypeDto(entity);

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

            // Insert collection of allowed content types. Resolved against the database AFTER the content
            // type row insert, so a type allowing itself (created with a self-reference, before it has an
            // identity) resolves within the same transaction.
            foreach ((var allowedId, var allowedSortOrder) in GetAllowedContentTypeIds(db, entity))
            {
                db.ContentTypeAllowedContentTypes.Add(new ContentTypeAllowedContentTypeDto
                {
                    Id = entity.Id,
                    AllowedId = allowedId,
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
    protected void PersistUpdatedBaseContentTypeEFCore(IContentTypeComposition entity)
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
        // (resolved against the database, so types created earlier in the same scope resolve too)
        ExecuteEfScope(db =>
        {
            db.ContentTypeAllowedContentTypes.Where(x => x.Id == entity.Id).ExecuteDelete();

            foreach ((var allowedId, var sortOrder) in GetAllowedContentTypeIds(db, entity))
            {
                db.ContentTypeAllowedContentTypes.Add(new ContentTypeAllowedContentTypeDto
                {
                    Id = entity.Id,
                    AllowedId = allowedId,
                    SortOrder = sortOrder,
                });
            }

            db.SaveChanges();
        });

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
        IEnumerable<TEntity> all = GetAllCached();
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
    protected void ExecuteEfScope(Action<UmbracoDbContext> action)
        => ExecuteEfScope(db =>
        {
            action(db);
            return true;
        });

    protected T ExecuteEfScope<T>(Func<UmbracoDbContext, T> func)
        => AmbientScope.ExecuteWithContextAsync(db => Task.FromResult(func(db))).GetAwaiter().GetResult();

    // ----------------------------------------------------------------------------------------------------
    // Shared content-type operations (move, variation cascade, variant-data moves and helpers).
    // Raw SQL conventions: content-instance tables owned by other repositories use Constants.DatabaseSchema
    // table names with inlined column names; server-generated integer ids are inlined (avoids the SQL Server
    // 2100-parameter limit); reserved identifiers ("group", "current", "text") are double-quoted, which both
    // SQL Server (QUOTED_IDENTIFIER ON) and SQLite accept; Guid values are bound as parameters, never string
    // literals (provider-specific storage formats).
    // ----------------------------------------------------------------------------------------------------

    /// <summary>
    /// Moves the specified content type entity to a new container or to the root if the container is <c>null</c>.
    /// Updates the entity's parent, path, and level, and also updates all descendant entities accordingly.
    /// </summary>
    public IEnumerable<MoveEventInfo<TEntity>> Move(TEntity moving, EntityContainer? container)
    {
        var parentId = Constants.System.Root;
        Guid? parentKey = Constants.System.RootKey;
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
            parentKey = container.Key;
        }

        // track moved entities
        var moveInfo = new List<MoveEventInfo<TEntity>> { new(moving, moving.Path, parentKey) };

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

        // update all descendants (from the cached full set), update in order of level
        TEntity[] descendants = GetAllCached()
            .Where(type => type.Path.StartsWith(movingPath, StringComparison.Ordinal))
            .ToArray();
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
    /// When content types are removed from a composition (U4-1690), clears the orphaned property data
    /// on content of <paramref name="entity"/> for property types that belonged to the removed types.
    /// </summary>
    private void ClearPropertyDataForRemovedContentTypes(IContentTypeComposition entity)
    {
        if (!entity.RemovedContentTypes.Any())
        {
            return;
        }

        ExecuteEfScope(db =>
        {
            // note: Guid values must be bound as parameters (not string literals) — providers store/bind them
            // in provider-specific formats (e.g. uppercase TEXT on SQLite, uniqueidentifier on SQL Server).
            var p0 = "{0}";
            foreach (var key in entity.RemovedContentTypes)
            {
                // delete property data on content of this content type, for property types belonging to
                // the removed composition type
                db.Database.ExecuteSqlRaw(
                    $"""
                     DELETE FROM {Constants.DatabaseSchema.Tables.PropertyData} WHERE id IN (
                     SELECT pd.id
                     FROM {Constants.DatabaseSchema.Tables.PropertyData} pd
                     INNER JOIN {Constants.DatabaseSchema.Tables.ContentVersion} cv ON pd.versionId = cv.id
                     INNER JOIN {Constants.DatabaseSchema.Tables.Content} c ON cv.nodeId = c.nodeId
                     INNER JOIN {NodeDto.TableName} n ON c.nodeId = n.{NodeDto.PrimaryKeyColumnName}
                     INNER JOIN {PropertyTypeDto.TableName} pt ON pd.propertyTypeId = pt.{PropertyTypeDto.PrimaryKeyColumnName}
                     WHERE n.{NodeDto.NodeObjectTypeColumnName} = {p0} AND c.contentTypeId = {entity.Id} AND pt.{PropertyTypeDto.ContentTypeIdColumnName} = {key})
                     """,
                    Constants.ObjectTypes.Document);
            }
        });
    }

    private void ValidateAlias(IPropertyType pt)
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

    /// <summary>
    /// Validates that the entity's alias is not empty.
    /// </summary>
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

    private static bool IsPropertyValueChanged(PropertyValueVersionDto pubRow, PropertyValueVersionDto row) =>
        (!pubRow.TextValue.IsNullOrWhiteSpace() && pubRow.TextValue != row.TextValue)
        || (!pubRow.VarcharValue.IsNullOrWhiteSpace() && pubRow.VarcharValue != row.VarcharValue)
        || (pubRow.DateValue.HasValue && pubRow.DateValue != row.DateValue)
        || (pubRow.DecimalValue.HasValue && pubRow.DecimalValue != row.DecimalValue)
        || (pubRow.IntValue.HasValue && pubRow.IntValue != row.IntValue);

    /// <summary>
    ///     Corrects the property type variations for the given entity to make sure the property type variation is
    ///     compatible with the variation set on the entity itself.
    /// </summary>
    private void CorrectPropertyTypeVariations(IContentTypeComposition entity)
    {
        foreach (IPropertyType propertyType in entity.PropertyTypes)
        {
            propertyType.Variations = entity.Variations & propertyType.Variations;
        }
    }

    /// <summary>
    ///     Ensures that no property types are flagged for a variance that is not supported by the content type itself.
    /// </summary>
    private void ValidateVariations(IContentTypeComposition entity)
    {
        foreach (IPropertyType prop in entity.PropertyTypes)
        {
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
        var propertyTypeIds = propertyTypesL.Select(x => x.Id).ToList();

        // select the current variations (before the change) from database
        Dictionary<int, byte> oldVariations = ExecuteEfScope(db => db.PropertyTypes
            .Where(x => propertyTypeIds.Contains(x.Id))
            .Select(x => new { x.Id, x.Variations })
            .ToDictionary(x => x.Id, x => x.Variations));

        Dictionary<int, (ContentVariation, ContentVariation)>? changes = null;

        foreach (IPropertyType propertyType in propertyTypesL)
        {
            if (!oldVariations.TryGetValue(propertyType.Id, out var oldVariationB))
            {
                continue;
            }

            var oldVariation = (ContentVariation)oldVariationB;

            ContentVariation newVariation = propertyType.Variations;
            if (oldVariation == newVariation)
            {
                continue;
            }

            changes ??= new Dictionary<int, (ContentVariation, ContentVariation)>();
            changes[propertyType.Id] = (oldVariation, newVariation);
        }

        return changes;
    }

    /// <summary>
    ///     Clear any redirects associated with content for a content type.
    /// </summary>
    private void Clear301Redirects(IContentTypeComposition contentType)
        => ExecuteEfScope(db => db.Database.ExecuteSqlRaw(
            $"""
             DELETE FROM {Constants.DatabaseSchema.Tables.RedirectUrl} WHERE contentKey IN (
             SELECT n.{NodeDto.KeyColumnName}
             FROM {NodeDto.TableName} n
             INNER JOIN {Constants.DatabaseSchema.Tables.Content} c ON c.nodeId = n.{NodeDto.PrimaryKeyColumnName}
             WHERE c.contentTypeId = {contentType.Id})
             """));

    /// <summary>
    ///     Clear any scheduled publishing associated with content for a content type.
    /// </summary>
    private void ClearScheduledPublishing(IContentTypeComposition contentType)
    {
        // TODO: Fill this in when scheduled publishing is enabled for variants
    }

    private int GetDefaultLanguageId()
        => ExecuteEfScope(db => db.Language.Where(x => x.IsDefault).Select(x => x.Id).First());

    /// <summary>
    ///     Moves variant data for property type variation changes.
    /// </summary>
    private void MovePropertyTypeVariantData(
        IDictionary<int, (ContentVariation FromVariation, ContentVariation ToVariation)> propertyTypeChanges,
        IEnumerable<IContentTypeComposition> impacted)
    {
        var defaultLanguageId = GetDefaultLanguageId();
        var impactedL = impacted.Select(x => x.Id).ToList();

        foreach (IGrouping<(ContentVariation FromVariation, ContentVariation ToVariation),
                     KeyValuePair<int, (ContentVariation FromVariation, ContentVariation ToVariation)>> grouping in
                 propertyTypeChanges.GroupBy(x => x.Value))
        {
            var propertyTypeIds = grouping.Select(x => x.Key).ToList();
            (ContentVariation fromVariation, ContentVariation toVariation) = grouping.Key;

            var fromCultureEnabled = fromVariation.HasFlag(ContentVariation.Culture);
            var toCultureEnabled = toVariation.HasFlag(ContentVariation.Culture);

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
    private void MoveContentTypeVariantData(
        IContentTypeComposition contentType,
        ContentVariation fromVariation,
        ContentVariation toVariation)
    {
        var defaultLanguageId = GetDefaultLanguageId();

        var cultureIsNotEnabled = !fromVariation.HasFlag(ContentVariation.Culture);
        var cultureWillBeEnabled = toVariation.HasFlag(ContentVariation.Culture);

        if (!cultureIsNotEnabled || !cultureWillBeEnabled)
        {
            return;
        }

        // move the names: first clear out any existing names that might already exist under the default lang,
        // then insert names into the two culture-variation tables based on the invariant data
        ExecuteEfScope(db =>
        {
            // clear out the versionCultureVariation table
            db.Database.ExecuteSqlRaw(
                $"""
                 DELETE FROM {Constants.DatabaseSchema.Tables.ContentVersionCultureVariation} WHERE id IN (
                 SELECT ccv.id
                 FROM {Constants.DatabaseSchema.Tables.ContentVersionCultureVariation} ccv
                 INNER JOIN {Constants.DatabaseSchema.Tables.ContentVersion} cv ON cv.id = ccv.versionId
                 INNER JOIN {Constants.DatabaseSchema.Tables.Content} c ON c.nodeId = cv.nodeId
                 WHERE c.contentTypeId = {contentType.Id} AND ccv.languageId = {defaultLanguageId})
                 """);

            // clear out the documentCultureVariation table
            db.Database.ExecuteSqlRaw(
                $"""
                 DELETE FROM {Constants.DatabaseSchema.Tables.DocumentCultureVariation} WHERE id IN (
                 SELECT dcv.id
                 FROM {Constants.DatabaseSchema.Tables.DocumentCultureVariation} dcv
                 INNER JOIN {Constants.DatabaseSchema.Tables.Content} c ON c.nodeId = dcv.nodeId
                 WHERE c.contentTypeId = {contentType.Id} AND dcv.languageId = {defaultLanguageId})
                 """);

            // insert rows into the versionCultureVariation table based on contentVersion data for the default lang
            db.Database.ExecuteSqlRaw(
                $"""
                 INSERT INTO {Constants.DatabaseSchema.Tables.ContentVersionCultureVariation} (versionId, "name", availableUserId, "date", languageId)
                 SELECT cv.id, cv."text", cv.userId, cv.versionDate, {defaultLanguageId}
                 FROM {Constants.DatabaseSchema.Tables.ContentVersion} cv
                 INNER JOIN {Constants.DatabaseSchema.Tables.Content} c ON c.nodeId = cv.nodeId
                 WHERE c.contentTypeId = {contentType.Id}
                 """);

            // insert rows into the documentCultureVariation table (make Available + default language ID)
            db.Database.ExecuteSqlRaw(
                $"""
                 INSERT INTO {Constants.DatabaseSchema.Tables.DocumentCultureVariation} (nodeId, edited, published, "name", available, languageId)
                 SELECT d.nodeId, d.edited, d.published, n."text", 1, {defaultLanguageId}
                 FROM {Constants.DatabaseSchema.Tables.Document} d
                 INNER JOIN {NodeDto.TableName} n ON n.{NodeDto.PrimaryKeyColumnName} = d.nodeId
                 INNER JOIN {Constants.DatabaseSchema.Tables.Content} c ON c.nodeId = n.{NodeDto.PrimaryKeyColumnName}
                 WHERE c.contentTypeId = {contentType.Id}
                 """);
        });
    }

    private void CopyTagData(
        int? sourceLanguageId,
        int? targetLanguageId,
        IReadOnlyCollection<int> propertyTypeIds,
        IReadOnlyCollection<int>? contentTypeIds = null)
    {
        if (propertyTypeIds.Count == 0)
        {
            return;
        }

        var pts = string.Join(",", propertyTypeIds);
        var cts = contentTypeIds is { Count: > 0 } ? string.Join(",", contentTypeIds) : null;
        var contentJoin = cts is null ? string.Empty : $"INNER JOIN {Constants.DatabaseSchema.Tables.Content} c ON r.nodeId = c.nodeId";
        var contentWhere = cts is null ? string.Empty : $"AND c.contentTypeId IN ({cts})";

        // note: nullable language ids are compared via COALESCE(x, -1), mirroring NPoco's SqlNullableEquals
        var srcCmp = sourceLanguageId?.ToString(CultureInfo.InvariantCulture) ?? "-1";
        var targetCmp = targetLanguageId?.ToString(CultureInfo.InvariantCulture) ?? "-1";
        var targetLiteral = targetLanguageId?.ToString(CultureInfo.InvariantCulture) ?? "NULL";

        ExecuteEfScope(db =>
        {
            // delete existing relations (for target language); do *not* delete existing tags
            db.Database.ExecuteSqlRaw(
                $"""
                 DELETE FROM {Constants.DatabaseSchema.Tables.TagRelationship} WHERE tagId IN (
                 SELECT t.id
                 FROM {Constants.DatabaseSchema.Tables.Tag} t
                 INNER JOIN {Constants.DatabaseSchema.Tables.TagRelationship} r ON t.id = r.tagId
                 {contentJoin}
                 WHERE r.propertyTypeId IN ({pts})
                 {contentWhere}
                 AND COALESCE(t.languageId, -1) = {targetCmp})
                 """);

            // copy tags from source language to target language; target tags may exist already, so check
            // for existence via the "xtags" left join
            db.Database.ExecuteSqlRaw(
                $"""
                 INSERT INTO {Constants.DatabaseSchema.Tables.Tag} (tag, "group", languageId)
                 SELECT DISTINCT t.tag, t."group", {targetLiteral}
                 FROM {Constants.DatabaseSchema.Tables.Tag} t
                 INNER JOIN {Constants.DatabaseSchema.Tables.TagRelationship} r ON t.id = r.tagId
                 LEFT JOIN {Constants.DatabaseSchema.Tables.Tag} xtags ON t.tag = xtags.tag AND t."group" = xtags."group" AND COALESCE(xtags.languageId, -1) = {targetCmp}
                 {contentJoin}
                 WHERE r.propertyTypeId IN ({pts})
                 {contentWhere}
                 AND xtags.id IS NULL
                 AND COALESCE(t.languageId, -1) = {srcCmp}
                 """);

            // create relations to the new tags (existing target relations were deleted above)
            db.Database.ExecuteSqlRaw(
                $"""
                 INSERT INTO {Constants.DatabaseSchema.Tables.TagRelationship} (nodeId, propertyTypeId, tagId)
                 SELECT DISTINCT r.nodeId, r.propertyTypeId, otag.id
                 FROM {Constants.DatabaseSchema.Tables.TagRelationship} r
                 INNER JOIN {Constants.DatabaseSchema.Tables.Tag} t ON r.tagId = t.id
                 INNER JOIN {Constants.DatabaseSchema.Tables.Tag} otag ON t.tag = otag.tag AND t."group" = otag."group" AND COALESCE(otag.languageId, -1) = {targetCmp}
                 {contentJoin}
                 WHERE COALESCE(t.languageId, -1) = {srcCmp}
                 AND r.propertyTypeId IN ({pts})
                 {contentWhere}
                 """);

            // delete original relations - *not* the tags - all of them
            db.Database.ExecuteSqlRaw(
                $"""
                 DELETE FROM {Constants.DatabaseSchema.Tables.TagRelationship} WHERE tagId IN (
                 SELECT t.id
                 FROM {Constants.DatabaseSchema.Tables.Tag} t
                 INNER JOIN {Constants.DatabaseSchema.Tables.TagRelationship} r ON t.id = r.tagId
                 {contentJoin}
                 WHERE r.propertyTypeId IN ({pts})
                 {contentWhere}
                 AND COALESCE(t.languageId, -1) <> {targetCmp})
                 """);
        });
    }

    /// <summary>
    ///     Copies property data from one language to another.
    /// </summary>
    /// <param name="sourceLanguageId">The source language (can be null ie invariant).</param>
    /// <param name="targetLanguageId">The target language (can be null ie invariant)</param>
    /// <param name="propertyTypeIds">The property type identifiers.</param>
    /// <param name="contentTypeIds">The content type identifiers.</param>
    private void CopyPropertyData(
        int? sourceLanguageId,
        int? targetLanguageId,
        IReadOnlyCollection<int> propertyTypeIds,
        IReadOnlyCollection<int>? contentTypeIds = null)
    {
        if (propertyTypeIds.Count == 0)
        {
            return;
        }

        var pts = string.Join(",", propertyTypeIds);
        var cts = contentTypeIds is { Count: > 0 } ? string.Join(",", contentTypeIds) : null;
        var versionScope = cts is null
            ? string.Empty
            : $"""
               versionId IN (
               SELECT cv.id
               FROM {Constants.DatabaseSchema.Tables.ContentVersion} cv
               INNER JOIN {Constants.DatabaseSchema.Tables.Content} c ON cv.nodeId = c.nodeId
               WHERE c.contentTypeId IN ({cts})) AND
               """;

        var srcCmp = sourceLanguageId?.ToString(CultureInfo.InvariantCulture) ?? "-1";
        var targetCmp = targetLanguageId?.ToString(CultureInfo.InvariantCulture) ?? "-1";
        var targetLiteral = targetLanguageId?.ToString(CultureInfo.InvariantCulture) ?? "NULL";

        ExecuteEfScope(db =>
        {
            // first clear out any existing property data that might already exist under the target language
            db.Database.ExecuteSqlRaw(
                $"""
                 DELETE FROM {Constants.DatabaseSchema.Tables.PropertyData} WHERE
                 {versionScope}
                 COALESCE(languageId, -1) = {targetCmp}
                 AND propertyTypeId IN ({pts})
                 """);

            // now insert all property data into the target language that exists under the source language
            var sourceJoin = cts is null
                ? string.Empty
                : $"""
                   INNER JOIN {Constants.DatabaseSchema.Tables.ContentVersion} cv ON pd.versionId = cv.id
                   INNER JOIN {Constants.DatabaseSchema.Tables.Content} c ON cv.nodeId = c.nodeId
                   """;
            var sourceWhere = cts is null ? string.Empty : $"AND c.contentTypeId IN ({cts})";
            db.Database.ExecuteSqlRaw(
                $"""
                 INSERT INTO {Constants.DatabaseSchema.Tables.PropertyData} (versionId, propertyTypeId, segment, intValue, decimalValue, dateValue, varcharValue, textValue, languageId)
                 SELECT pd.versionId, pd.propertyTypeId, pd.segment, pd.intValue, pd.decimalValue, pd.dateValue, pd.varcharValue, pd.textValue, {targetLiteral}
                 FROM {Constants.DatabaseSchema.Tables.PropertyData} pd
                 {sourceJoin}
                 WHERE COALESCE(pd.languageId, -1) = {srcCmp}
                 AND pd.propertyTypeId IN ({pts})
                 {sourceWhere}
                 """);

            // when copying from Culture, keep the original values around in case we want to go back
            // when copying from Nothing, kill the original values, we don't want them around
            if (sourceLanguageId == null)
            {
                db.Database.ExecuteSqlRaw(
                    $"""
                     DELETE FROM {Constants.DatabaseSchema.Tables.PropertyData} WHERE
                     {versionScope}
                     languageId IS NULL
                     AND propertyTypeId IN ({pts})
                     """);
            }
        });
    }

    /// <summary>
    ///     Re-normalizes the edited value in the umbracoDocumentCultureVariation and umbracoDocument table when
    ///     variations are changed.
    /// </summary>
    private void RenormalizeDocumentEditedFlags(
        IReadOnlyCollection<int> propertyTypeIds,
        IReadOnlyCollection<int>? contentTypeIds = null)
    {
        if (propertyTypeIds.Count == 0)
        {
            return;
        }

        // TODO: Await this properly when the repository goes fully async.
        var defaultLang = LanguageRepository.GetDefaultIdAsync().GetAwaiter().GetResult();

        var pts = string.Join(",", propertyTypeIds);
        var cts = contentTypeIds is { Count: > 0 } ? string.Join(",", contentTypeIds) : null;
        var contentJoin = cts is null ? string.Empty : $"INNER JOIN {Constants.DatabaseSchema.Tables.Content} c ON c.nodeId = cv.nodeId";
        var contentWhere = cts is null ? string.Empty : $"AND c.contentTypeId IN ({cts})";

        // Build a query for the property values of both the current and the published version so we can check,
        // based on the current variance of each item, whether its 'edited' value should be true/false.
        // Note: no ORDER BY in the raw SQL (EF composes raw queries into subqueries); rows are sorted in memory.
        var propertySql =
            $"""
             SELECT pd.versionId AS VersionId, pd.propertyTypeId AS PropertyTypeId,
             pd.languageId AS LanguageId, pd.segment AS Segment, pd.intValue AS IntValue, pd.decimalValue AS DecimalValue,
             pd.dateValue AS DateValue, pd.varcharValue AS VarcharValue, pd.textValue AS TextValue,
             cv.nodeId AS NodeId, cv."current" AS "Current", CAST(COALESCE(dv.published, 0) AS BIT) AS Published, pt.variations AS Variations
             FROM {Constants.DatabaseSchema.Tables.PropertyData} pd
             INNER JOIN {Constants.DatabaseSchema.Tables.ContentVersion} cv ON cv.id = pd.versionId
             INNER JOIN {PropertyTypeDto.TableName} pt ON pt.{PropertyTypeDto.PrimaryKeyColumnName} = pd.propertyTypeId
             {contentJoin}
             LEFT JOIN {Constants.DatabaseSchema.Tables.DocumentVersion} dv ON cv.id = dv.id
             WHERE (cv."current" = 1 OR dv.published = 1)
             AND pd.propertyTypeId IN ({pts})
             {contentWhere}
             """;

        List<PropertyValueVersionDto> rows = ExecuteEfScope(db =>
            db.Database.SqlQueryRaw<PropertyValueVersionDto>(propertySql).ToList());

        // Published data must come before Current data, per (nodeId, propertyTypeId, languageId, versionId).
        rows = rows
            .OrderBy(x => x.NodeId)
            .ThenBy(x => x.PropertyTypeId)
            .ThenBy(x => x.LanguageId)
            .ThenBy(x => x.VersionId)
            .ToList();

        // keep track of this node/lang to mark or unmark a culture as edited
        var editedLanguageVersions = new Dictionary<(int nodeId, int? langId), bool>();

        // keep track of which node to mark or unmark as edited
        var editedDocument = new Dictionary<int, bool>();
        var nodeId = -1;
        var propertyTypeId = -1;

        PropertyValueVersionDto? pubRow = null;

        foreach (PropertyValueVersionDto row in rows)
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
                    if (!editedLanguageVersions.TryGetValue((row.NodeId, row.LanguageId), out _))
                    {
                        editedLanguageVersions.Add((row.NodeId, row.LanguageId), false);
                    }

                    editedDocument[row.NodeId] = editedDocument.TryGetValue(row.NodeId, out var edited)
                        ? edited |= false
                        : false;
                }
                else if (pubRow == null)
                {
                    // this property is 'edited' since there is no published version
                    editedLanguageVersions[(row.NodeId, row.LanguageId)] = true;
                    editedDocument[row.NodeId] = true;
                }
                else if (IsPropertyValueChanged(pubRow, row))
                {
                    // an invariant property's edited language is indicated by the default lang
                    editedLanguageVersions[
                        (row.NodeId, !propVariations.VariesByCulture() ? defaultLang : row.LanguageId)] = true;
                    editedDocument[row.NodeId] = true;
                }

                pubRow = null;
            }
        }

        // lookup all matching rows in umbracoDocumentCultureVariation
        // fetch in batches to keep statement size bounded
        var languageIds = editedLanguageVersions.Keys
            .Select(x => x.langId)
            .Where(x => x.HasValue)
            .Select(x => x!.Value)
            .Distinct()
            .ToArray();
        IEnumerable<int> nodeIds = editedLanguageVersions.Keys.Select(x => x.nodeId).Distinct();
        Dictionary<(int NodeId, int? LanguageId), DocumentCultureVariationRow> docCultureVariationsToUpdate =
            languageIds.Length == 0
                ? new Dictionary<(int, int?), DocumentCultureVariationRow>()
                : nodeIds.InGroupsOf(Constants.Sql.MaxParameterCount - languageIds.Length)
                    .SelectMany(group =>
                    {
                        var sql =
                            $"""
                             SELECT id AS Id, nodeId AS NodeId, languageId AS LanguageId, edited AS Edited
                             FROM {Constants.DatabaseSchema.Tables.DocumentCultureVariation}
                             WHERE languageId IN ({string.Join(",", languageIds)}) AND nodeId IN ({string.Join(",", group)})
                             """;
                        return ExecuteEfScope(db => db.Database.SqlQueryRaw<DocumentCultureVariationRow>(sql).ToList());
                    })
                    .ToDictionary(
                        x => (x.NodeId, (int?)x.LanguageId),
                        x => x);

        var toUpdate = new List<DocumentCultureVariationRow>();
        foreach (KeyValuePair<(int nodeId, int? langId), bool> ev in editedLanguageVersions)
        {
            if (docCultureVariationsToUpdate.TryGetValue(ev.Key, out DocumentCultureVariationRow? docVariations))
            {
                if (docVariations.Edited != ev.Value)
                {
                    docVariations.Edited = ev.Value;
                    toUpdate.Add(docVariations);
                }
            }
            else if (ev.Key.langId.HasValue)
            {
                // This can happen when a property changes from invariant to variant and the content was only
                // created in non-default languages: there is no DocumentCultureVariation row for the default
                // language, so there is no edited flag to update.
                continue;
            }
        }

        ExecuteEfScope(db =>
        {
            // bulk update umbracoDocumentCultureVariation, once for edited = true, another for edited = false
            foreach (IGrouping<bool, DocumentCultureVariationRow> editValue in toUpdate.GroupBy(x => x.Edited))
            {
                foreach (IEnumerable<DocumentCultureVariationRow> batch in editValue.InGroupsOf(Constants.Sql.MaxParameterCount))
                {
                    db.Database.ExecuteSqlRaw(
                        $"UPDATE {Constants.DatabaseSchema.Tables.DocumentCultureVariation} SET edited = {(editValue.Key ? 1 : 0)} WHERE id IN ({string.Join(",", batch.Select(x => x.Id))})");
                }
            }

            // bulk update the umbracoDocument table
            foreach (IGrouping<bool, KeyValuePair<int, bool>> groupByValue in editedDocument.GroupBy(x => x.Value))
            {
                foreach (IEnumerable<KeyValuePair<int, bool>> batch in groupByValue.InGroupsOf(Constants.Sql.MaxParameterCount))
                {
                    db.Database.ExecuteSqlRaw(
                        $"UPDATE {Constants.DatabaseSchema.Tables.Document} SET edited = {(groupByValue.Key ? 1 : 0)} WHERE nodeId IN ({string.Join(",", batch.Select(x => x.Key))})");
                }
            }
        });
    }

    private void DeletePropertyType(IContentTypeComposition contentType, int propertyTypeId)
        => ExecuteEfScope(db =>
        {
            // first clear dependencies
            db.Database.ExecuteSqlRaw(
                $"DELETE FROM {Constants.DatabaseSchema.Tables.TagRelationship} WHERE propertyTypeId = {propertyTypeId}");
            db.Database.ExecuteSqlRaw(
                $"DELETE FROM {Constants.DatabaseSchema.Tables.PropertyData} WHERE propertyTypeId = {propertyTypeId}");

            // delete granular permissions scoped to this property type (permission format: "<propertyTypeKey>|...")
            Guid? propertyTypeKey = db.PropertyTypes
                .Where(x => x.Id == propertyTypeId)
                .Select(x => (Guid?)x.UniqueId)
                .FirstOrDefault();
            if (propertyTypeKey.HasValue)
            {
                db.Database.ExecuteSqlRaw(
                    $"DELETE FROM {Constants.DatabaseSchema.Tables.UserGroup2GranularPermission} WHERE uniqueId = {{0}} AND permission LIKE {{1}}",
                    contentType.Key,
                    $"{propertyTypeKey.Value}|%");
            }

            // Finally delete the property type.
            db.PropertyTypes.Where(x => x.ContentTypeId == contentType.Id && x.Id == propertyTypeId).ExecuteDelete();
        });

    /// <inheritdoc />
    public string GetUniqueAlias(string alias)
    {
        // alias is unique across ALL content types!
        List<string> aliases = ExecuteEfScope(db => db.ContentTypes
            .Where(x => x.Alias != null && x.Alias.StartsWith(alias))
            .Select(x => x.Alias!)
            .ToList());

        var i = 1;
        string test;
        while (aliases.Contains(test = alias + i))
        {
            i++;
        }

        return test;
    }

    /// <inheritdoc />
    public bool HasContainerInPath(string contentPath)
    {
        var ids = contentPath.Split(Constants.CharArrays.Comma)
            .Select(s => int.Parse(s, CultureInfo.InvariantCulture)).ToArray();
        return HasContainerInPath(ids);
    }

    /// <inheritdoc />
    public bool HasContainerInPath(params int[] ids)
    {
        if (ids.Length == 0)
        {
            return false;
        }

        var sql =
            $"""
             SELECT COUNT(*) AS Value
             FROM {ContentTypeDto.TableName} ct
             INNER JOIN {Constants.DatabaseSchema.Tables.Content} c ON ct.{ContentTypeDto.NodeIdColumnName} = c.contentTypeId
             WHERE c.nodeId IN ({string.Join(",", ids)}) AND ct.listView IS NULL
             """;
        return ExecuteEfScope(db => db.Database.SqlQueryRaw<int>(sql).Single()) > 0;
    }

    /// <inheritdoc />
    public bool HasContentNodes(int id)
    {
        var sql =
            $"SELECT CASE WHEN EXISTS (SELECT * FROM {Constants.DatabaseSchema.Tables.Content} WHERE contentTypeId = {{0}}) THEN 1 ELSE 0 END AS Value";
        return ExecuteEfScope(db => db.Database.SqlQueryRaw<int>(sql, id).Single()) == 1;
    }

    /// <inheritdoc />
    public IEnumerable<Guid> GetAllowedParentKeys(Guid key)
        => ExecuteEfScope(db =>
        {
            IQueryable<int> childNodeIds = db.Nodes
                .Where(x => x.UniqueId == key)
                .Select(x => x.NodeId);

            return (from allowed in db.ContentTypeAllowedContentTypes
                    join node in db.Nodes on allowed.Id equals node.NodeId
                    where childNodeIds.Contains(allowed.AllowedId)
                    select node.UniqueId).ToList();
        });

    /// <summary>
    /// Resolves the entity's allowed content types to their node ids directly against the database (not the
    /// cached full set), so that a content type referencing itself — or another type created earlier in the
    /// same scope — resolves correctly even though it is not yet present in the cache.
    /// </summary>
    private (int Id, int SortOrder)[] GetAllowedContentTypeIds(UmbracoDbContext db, IContentTypeBase contentTypeBase)
    {
        if (contentTypeBase.AllowedContentTypes?.Any() is not true)
        {
            return Array.Empty<(int, int)>();
        }

        Guid[] allowedContentTypeKeys = contentTypeBase
            .AllowedContentTypes
            .OrderBy(c => c.SortOrder)
            .Select(c => c.Key)
            .ToArray();

        Dictionary<Guid, int> idsByKey = db.Nodes
            .Where(n => allowedContentTypeKeys.Contains(n.UniqueId) && n.NodeObjectType == NodeObjectTypeId)
            .Select(n => new { n.UniqueId, n.NodeId })
            .ToDictionary(n => n.UniqueId, n => n.NodeId);

        // NOTE: we're efficiently discarding the input sort order here in favor of a "0 to n" sorting.
        return allowedContentTypeKeys
            .Select((key, index) => (Key: key, Index: index))
            .Where(x => idsByKey.ContainsKey(x.Key))
            .Select(x => (idsByKey[x.Key], x.Index))
            .ToArray();
    }

    private sealed class PropertyValueVersionDto
    {
        private decimal? _decimalValue;

        public int VersionId { get; set; }

        public int PropertyTypeId { get; set; }

        public int? LanguageId { get; set; }

        public string? Segment { get; set; }

        public int? IntValue { get; set; }

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

        // The cmsPropertyType.variations column is an int on SQL Server (Umbraco maps byte/short to int columns);
        // read it as int and cast to ContentVariation at the use site.
        public int Variations { get; set; }
    }

    private sealed class DocumentCultureVariationRow
    {
        public int Id { get; set; }

        public int NodeId { get; set; }

        public int LanguageId { get; set; }

        public bool Edited { get; set; }
    }
}
