using System.Globalization;
using System.Linq.Expressions;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Persistence;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;

namespace Umbraco.Cms.Core.Services;

public class EntityService : RepositoryService, IEntityService
{
    private readonly IEntityRepository _entityRepository;
    private readonly IIdKeyMap _idKeyMap;
    private readonly Dictionary<string, UmbracoObjectTypes> _objectTypes;
    private IQuery<IUmbracoEntity>? _queryRootEntity;

    public EntityService(
        ICoreScopeProvider provider,
        ILoggerFactory loggerFactory,
        IEventMessagesFactory eventMessagesFactory,
        IIdKeyMap idKeyMap,
        IEntityRepository entityRepository)
        : base(provider, loggerFactory, eventMessagesFactory)
    {
        _idKeyMap = idKeyMap;
        _entityRepository = entityRepository;

        _objectTypes = new Dictionary<string, UmbracoObjectTypes>
        {
            { typeof(IDataType).FullName!, UmbracoObjectTypes.DataType },
            { typeof(IContent).FullName!, UmbracoObjectTypes.Document },
            { typeof(IContentType).FullName!, UmbracoObjectTypes.DocumentType },
            { typeof(IMedia).FullName!, UmbracoObjectTypes.Media },
            { typeof(IMediaType).FullName!, UmbracoObjectTypes.MediaType },
            { typeof(IMember).FullName!, UmbracoObjectTypes.Member },
            { typeof(IMemberType).FullName!, UmbracoObjectTypes.MemberType },
            { typeof(IMemberGroup).FullName!, UmbracoObjectTypes.MemberGroup },
            { typeof(ITemplate).FullName!, UmbracoObjectTypes.Template },
        };
    }

    #region Static Queries

    // lazy-constructed because when the ctor runs, the query factory may not be ready
    private IQuery<IUmbracoEntity> QueryRootEntity => _queryRootEntity ??= Query<IUmbracoEntity>()
                                                          .Where(x => x.ParentId == -1);

    #endregion

    /// <inheritdoc />
    public IEntitySlim? Get(int id)
    {
        using (ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            return _entityRepository.Get(id);
        }
    }

    /// <inheritdoc />
    public IEntitySlim? Get(Guid key)
    {
        using (ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            return _entityRepository.Get(key);
        }
    }

    /// <inheritdoc />
    public virtual IEntitySlim? Get(int id, UmbracoObjectTypes objectType)
    {
        using (ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            return _entityRepository.Get(id, objectType.GetGuid());
        }
    }

    /// <inheritdoc />
    public IEntitySlim? Get(Guid key, UmbracoObjectTypes objectType)
    {
        using (ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            return _entityRepository.Get(key, objectType.GetGuid());
        }
    }

    /// <inheritdoc />
    public virtual IEntitySlim? Get<T>(int id)
        where T : IUmbracoEntity
    {
        using (ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            return _entityRepository.Get(id);
        }
    }

    /// <inheritdoc />
    public virtual IEntitySlim? Get<T>(Guid key)
        where T : IUmbracoEntity
    {
        using (ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            return _entityRepository.Get(key);
        }
    }

    /// <inheritdoc />
    public bool Exists(int id)
    {
        using (ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            return _entityRepository.Exists(id);
        }
    }

    /// <inheritdoc />
    public bool Exists(Guid key)
    {
        using (ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            return _entityRepository.Exists(key);
        }
    }

    public bool Exists(IEnumerable<Guid> keys)
    {
        using (ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            return _entityRepository.Exists(keys);
        }
    }

    /// <inheritdoc />
    public bool Exists(Guid key, UmbracoObjectTypes objectType)
    {
        using (ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            return _entityRepository.Exists(key, objectType.GetGuid());
        }
    }

    /// <inheritdoc />
    public bool Exists(int id, UmbracoObjectTypes objectType)
    {
        using (ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            return _entityRepository.Exists(id, objectType.GetGuid());
        }
    }

    /// <inheritdoc />
    public virtual IEnumerable<IEntitySlim> GetAll<T>()
        where T : IUmbracoEntity
        => GetAll<T>(Array.Empty<int>());

    /// <inheritdoc />
    public virtual IEnumerable<IEntitySlim> GetAll<T>(params int[] ids)
        where T : IUmbracoEntity
    {
        Type entityType = typeof(T);
        UmbracoObjectTypes objectType = GetObjectType(entityType);
        Guid objectTypeId = objectType.GetGuid();

        using (ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            return _entityRepository.GetAll(objectTypeId, ids);
        }
    }

    /// <inheritdoc />
    public virtual IEnumerable<IEntitySlim> GetAll(UmbracoObjectTypes objectType)
        => GetAll(objectType, Array.Empty<int>());

    /// <inheritdoc />
    public virtual IEnumerable<IEntitySlim> GetAll(UmbracoObjectTypes objectType, params int[] ids)
    {
        Type? entityType = objectType.GetClrType();
        if (entityType == null)
        {
            throw new NotSupportedException($"Type \"{objectType}\" is not supported here.");
        }

        GetObjectType(entityType);

        using (ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            return _entityRepository.GetAll(objectType.GetGuid(), ids);
        }
    }

    /// <inheritdoc />
    public virtual IEnumerable<IEntitySlim> GetAll(Guid objectType)
        => GetAll(objectType, Array.Empty<int>());

    /// <inheritdoc />
    public virtual IEnumerable<IEntitySlim> GetAll(Guid objectType, params int[] ids)
    {
        Type? entityType = ObjectTypes.GetClrType(objectType);
        GetObjectType(entityType);

        using (ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            return _entityRepository.GetAll(objectType, ids);
        }
    }

    /// <inheritdoc />
    public virtual IEnumerable<IEntitySlim> GetAll<T>(params Guid[] keys)
        where T : IUmbracoEntity
    {
        Type entityType = typeof(T);
        UmbracoObjectTypes objectType = GetObjectType(entityType);
        Guid objectTypeId = objectType.GetGuid();

        using (ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            return _entityRepository.GetAll(objectTypeId, keys);
        }
    }

    /// <inheritdoc />
    public IEnumerable<IEntitySlim> GetAll(UmbracoObjectTypes objectType, Guid[] keys)
    {
        Type? entityType = objectType.GetClrType();
        GetObjectType(entityType);

        using (ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            return _entityRepository.GetAll(objectType.GetGuid(), keys);
        }
    }

    /// <inheritdoc />
    public virtual IEnumerable<IEntitySlim> GetAll(Guid objectType, params Guid[] keys)
    {
        Type? entityType = ObjectTypes.GetClrType(objectType);
        GetObjectType(entityType);

        using (ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            return _entityRepository.GetAll(objectType, keys);
        }
    }

    /// <inheritdoc />
    public virtual IEnumerable<IEntitySlim> GetRootEntities(UmbracoObjectTypes objectType)
    {
        using (ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            return _entityRepository.GetByQuery(QueryRootEntity, objectType.GetGuid());
        }
    }

    /// <inheritdoc />
    public virtual IEntitySlim? GetParent(int id)
    {
        using (ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            IEntitySlim? entity = _entityRepository.Get(id);
            if (entity is null || entity.ParentId == -1 || entity.ParentId == -20 || entity.ParentId == -21)
            {
                return null;
            }

            return _entityRepository.Get(entity.ParentId);
        }
    }

    /// <inheritdoc />
    public virtual IEntitySlim? GetParent(int id, UmbracoObjectTypes objectType)
    {
        using (ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            IEntitySlim? entity = _entityRepository.Get(id);
            if (entity is null || entity.ParentId == -1 || entity.ParentId == -20 || entity.ParentId == -21)
            {
                return null;
            }

            return _entityRepository.Get(entity.ParentId, objectType.GetGuid());
        }
    }

    /// <inheritdoc />
    public virtual IEnumerable<IEntitySlim> GetChildren(int parentId)
    {
        using (ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            IQuery<IUmbracoEntity> query = Query<IUmbracoEntity>().Where(x => x.ParentId == parentId);
            return _entityRepository.GetByQuery(query);
        }
    }

    /// <inheritdoc />
    public virtual IEnumerable<IEntitySlim> GetChildren(int parentId, UmbracoObjectTypes objectType)
    {
        using (ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            IQuery<IUmbracoEntity> query = Query<IUmbracoEntity>().Where(x => x.ParentId == parentId);
            return _entityRepository.GetByQuery(query, objectType.GetGuid());
        }
    }

    public IEnumerable<IEntitySlim> GetChildren(Guid? key, UmbracoObjectTypes objectType)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope();

        if (ResolveKey(key, objectType, out int parentId) is false)
        {
            return Enumerable.Empty<IEntitySlim>();
        }

        IEnumerable<IEntitySlim> children = GetChildren(parentId, objectType);

        return children;
    }

    /// <inheritdoc />
    public IEnumerable<IEntitySlim> GetSiblings(
        Guid key,
        IEnumerable<UmbracoObjectTypes> objectTypes,
        int before,
        int after,
        out long totalBefore,
        out long totalAfter,
        IQuery<IUmbracoEntity>? filter = null,
        Ordering? ordering = null)
    {
        if (before < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(before), "The 'before' parameter must be greater than or equal to 0.");
        }

        if (after < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(after), "The 'after' parameter must be greater than or equal to 0.");
        }

        ordering ??= new Ordering("sortOrder");

        using ICoreScope scope = ScopeProvider.CreateCoreScope();

        var objectTypeGuids = objectTypes.Select(x => x.GetGuid()).ToHashSet();

        IEnumerable<IEntitySlim> siblings = _entityRepository.GetSiblings(
            objectTypeGuids,
            key,
            before,
            after,
            filter,
            ordering,
            out totalBefore,
            out totalAfter);

        scope.Complete();
        return siblings;
    }

    /// <inheritdoc />
    public virtual IEnumerable<IEntitySlim> GetDescendants(int id)
    {
        using (ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            IEntitySlim? entity = _entityRepository.Get(id);
            var pathMatch = entity?.Path + ",";
            IQuery<IUmbracoEntity> query = Query<IUmbracoEntity>()
                .Where(x => x.Path.StartsWith(pathMatch) && x.Id != id);
            return _entityRepository.GetByQuery(query);
        }
    }

    /// <inheritdoc />
    public virtual IEnumerable<IEntitySlim> GetDescendants(int id, UmbracoObjectTypes objectType)
    {
        using (ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            IEntitySlim? entity = _entityRepository.Get(id);
            if (entity is null)
            {
                return Enumerable.Empty<IEntitySlim>();
            }

            IQuery<IUmbracoEntity> query = Query<IUmbracoEntity>()
                .Where(x => x.Path.StartsWith(entity.Path) && x.Id != id);
            return _entityRepository.GetByQuery(query, objectType.GetGuid());
        }
    }

    /// <inheritdoc />
    public IEnumerable<IEntitySlim> GetPagedChildren(
        int id,
        UmbracoObjectTypes objectType,
        long pageIndex,
        int pageSize,
        out long totalRecords,
        IQuery<IUmbracoEntity>? filter = null,
        Ordering? ordering = null)
        => GetPagedChildren(id, objectType, pageIndex, pageSize, false, filter, ordering, out totalRecords);

    public IEnumerable<IEntitySlim> GetPagedChildren(
        Guid? parentKey,
        UmbracoObjectTypes childObjectType,
        int skip,
        int take,
        out long totalRecords,
        IQuery<IUmbracoEntity>? filter = null,
        Ordering? ordering = null)
        => GetPagedChildren(
            parentKey,
            new[] { childObjectType },
            childObjectType,
            skip,
            take,
            out totalRecords,
            filter,
            ordering);

    public IEnumerable<IEntitySlim> GetPagedChildren(
        Guid? parentKey,
        IEnumerable<UmbracoObjectTypes> parentObjectTypes,
        UmbracoObjectTypes childObjectType,
        int skip,
        int take,
        out long totalRecords,
        IQuery<IUmbracoEntity>? filter = null,
        Ordering? ordering = null)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope();

        var parentId = 0;
        var parentIdResolved = parentObjectTypes.Any(parentObjectType => ResolveKey(parentKey, parentObjectType, out parentId));
        if (parentIdResolved is false)
        {
            totalRecords = 0;
            return Enumerable.Empty<IEntitySlim>();
        }

        if (take == 0)
        {
            totalRecords = CountChildren(parentId, childObjectType, filter: filter);
            return Enumerable.Empty<IEntitySlim>();
        }

        PaginationHelper.ConvertSkipTakeToPaging(skip, take, out var pageNumber, out var pageSize);

        IEnumerable<IEntitySlim> children = GetPagedChildren(
            parentId,
            childObjectType,
            pageNumber,
            pageSize,
            out totalRecords,
            filter,
            ordering);

        scope.Complete();
        return children;
    }

    /// <inheritdoc />
    public IEnumerable<IEntitySlim> GetPagedTrashedChildren(
        int id,
        UmbracoObjectTypes objectType,
        long pageIndex,
        int pageSize,
        out long totalRecords,
        IQuery<IUmbracoEntity>? filter = null,
        Ordering? ordering = null)
        => GetPagedChildren(id, objectType, pageIndex, pageSize, true, filter, ordering, out totalRecords);

    public IEnumerable<IEntitySlim> GetPagedTrashedChildren(
        Guid? key,
        UmbracoObjectTypes objectType,
        int skip,
        int take,
        out long totalRecords,
        IQuery<IUmbracoEntity>? filter = null,
        Ordering? ordering = null)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope();

        if (ResolveKey(key, objectType, out int parentId) is false)
        {
            totalRecords = 0;
            return Enumerable.Empty<IEntitySlim>();
        }

        if (take == 0)
        {
            totalRecords = CountChildren(parentId, objectType, true, filter);
            return Enumerable.Empty<IEntitySlim>();
        }

        PaginationHelper.ConvertSkipTakeToPaging(skip, take, out var pageNumber, out var pageSize);

        IEnumerable<IEntitySlim> children = GetPagedChildren(
            parentId,
            objectType,
            pageNumber,
            pageSize,
            true,
            filter,
            ordering,
            out totalRecords);

        scope.Complete();
        return children;
    }

    /// <inheritdoc />
    public IEnumerable<IEntitySlim> GetPagedDescendants(
        int id,
        UmbracoObjectTypes objectType,
        long pageIndex,
        int pageSize,
        out long totalRecords,
        IQuery<IUmbracoEntity>? filter = null,
        Ordering? ordering = null)
    {
        using (ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            Guid objectTypeGuid = objectType.GetGuid();
            IQuery<IUmbracoEntity> query = Query<IUmbracoEntity>();

            if (id != Constants.System.Root)
            {
                // lookup the path so we can use it in the prefix query below
                TreeEntityPath[] paths = _entityRepository.GetAllPaths(objectTypeGuid, id).ToArray();
                if (paths.Length == 0)
                {
                    totalRecords = 0;
                    return Enumerable.Empty<IEntitySlim>();
                }

                var path = paths[0].Path;
                query.Where(x => x.Path.SqlStartsWith(path + ",", TextColumnType.NVarchar));
            }

            return _entityRepository.GetPagedResultsByQuery(query, objectTypeGuid, pageIndex, pageSize, out totalRecords, filter, ordering);
        }
    }

    /// <inheritdoc />
    public IEnumerable<IEntitySlim> GetPagedDescendants(
        IEnumerable<int> ids,
        UmbracoObjectTypes objectType,
        long pageIndex,
        int pageSize,
        out long totalRecords,
        IQuery<IUmbracoEntity>? filter = null,
        Ordering? ordering = null)
    {
        totalRecords = 0;

        var idsA = ids.ToArray();
        if (idsA.Length == 0)
        {
            return Enumerable.Empty<IEntitySlim>();
        }

        using (ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            Guid objectTypeGuid = objectType.GetGuid();
            IQuery<IUmbracoEntity> query = Query<IUmbracoEntity>();

            if (idsA.All(x => x != Constants.System.Root))
            {
                TreeEntityPath[] paths = _entityRepository.GetAllPaths(objectTypeGuid, idsA).ToArray();
                if (paths.Length == 0)
                {
                    totalRecords = 0;
                    return Enumerable.Empty<IEntitySlim>();
                }

                var clauses = new List<Expression<Func<IUmbracoEntity, bool>>>();
                foreach (var id in idsA)
                {
                    // if the id is root then don't add any clauses
                    if (id == Constants.System.Root)
                    {
                        continue;
                    }

                    TreeEntityPath? entityPath = paths.FirstOrDefault(x => x.Id == id);
                    if (entityPath == null)
                    {
                        continue;
                    }

                    var path = entityPath.Path;
                    var qid = id;
                    clauses.Add(x =>
                        x.Path.SqlStartsWith(path + ",", TextColumnType.NVarchar) ||
                        x.Path.SqlEndsWith("," + qid, TextColumnType.NVarchar));
                }

                query.WhereAny(clauses);
            }

            return _entityRepository.GetPagedResultsByQuery(query, objectTypeGuid, pageIndex, pageSize, out totalRecords, filter, ordering);
        }
    }

    /// <inheritdoc />
    public IEnumerable<IEntitySlim> GetPagedDescendants(
        UmbracoObjectTypes objectType,
        long pageIndex,
        int pageSize,
        out long totalRecords,
        IQuery<IUmbracoEntity>? filter = null,
        Ordering? ordering = null,
        bool includeTrashed = true)
    {
        using (ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            IQuery<IUmbracoEntity> query = Query<IUmbracoEntity>();
            if (includeTrashed == false)
            {
                query.Where(x => x.Trashed == false);
            }

            return _entityRepository.GetPagedResultsByQuery(query, objectType.GetGuid(), pageIndex, pageSize, out totalRecords, filter, ordering);
        }
    }

    /// <inheritdoc />
    public virtual UmbracoObjectTypes GetObjectType(int id)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            return _entityRepository.GetObjectType(id);
        }
    }

    /// <inheritdoc />
    public virtual UmbracoObjectTypes GetObjectType(Guid key)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            return _entityRepository.GetObjectType(key);
        }
    }

    /// <inheritdoc />
    public virtual UmbracoObjectTypes GetObjectType(IUmbracoEntity entity) =>
        entity is IEntitySlim light
            ? ObjectTypes.GetUmbracoObjectType(light.NodeObjectType)
            : GetObjectType(entity.Id);

    /// <inheritdoc />
    public virtual Type? GetEntityType(int id)
    {
        UmbracoObjectTypes objectType = GetObjectType(id);
        return objectType.GetClrType();
    }

    /// <inheritdoc />
    public Attempt<int> GetId(Guid key, UmbracoObjectTypes objectType) => _idKeyMap.GetIdForKey(key, objectType);

    /// <inheritdoc />
    public Attempt<int> GetId(Udi udi) => _idKeyMap.GetIdForUdi(udi);

    /// <inheritdoc />
    public Attempt<Guid> GetKey(int id, UmbracoObjectTypes umbracoObjectType) =>
        _idKeyMap.GetKeyForId(id, umbracoObjectType);

    /// <inheritdoc />
    public virtual IEnumerable<TreeEntityPath> GetAllPaths(UmbracoObjectTypes objectType, params int[]? ids)
    {
        Type? entityType = objectType.GetClrType();
        GetObjectType(entityType);

        using (ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            return _entityRepository.GetAllPaths(objectType.GetGuid(), ids);
        }
    }

    /// <inheritdoc />
    public virtual IEnumerable<TreeEntityPath> GetAllPaths(UmbracoObjectTypes objectType, params Guid[] keys)
    {
        Type? entityType = objectType.GetClrType();
        GetObjectType(entityType);

        using (ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            return _entityRepository.GetAllPaths(objectType.GetGuid(), keys);
        }
    }

    /// <inheritdoc />
    public int ReserveId(Guid key)
    {
        using (ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            return _entityRepository.ReserveId(key);
        }
    }

    private int CountChildren(int id, UmbracoObjectTypes objectType, bool trashed = false, IQuery<IUmbracoEntity>? filter = null) =>
        CountChildren(id, new HashSet<UmbracoObjectTypes>() { objectType }, trashed, filter);

    private int CountChildren(
        int id,
        IEnumerable<UmbracoObjectTypes> objectTypes,
        bool trashed = false,
        IQuery<IUmbracoEntity>? filter = null)
    {
        using (ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            IQuery<IUmbracoEntity> query = Query<IUmbracoEntity>().Where(x => x.ParentId == id && x.Trashed == trashed);

            var objectTypeGuids = objectTypes.Select(x => x.GetGuid()).ToHashSet();
            return _entityRepository.CountByQuery(query, objectTypeGuids, filter);
        }
    }

    private bool ResolveKey(Guid? key, UmbracoObjectTypes objectType, out int id)
    {
        // We have to explicitly check for "root key" since this value is null, and GetId does not accept null.
        if (key == Constants.System.RootKey)
        {
            id = Constants.System.Root;
            return true;
        }

        Attempt<int> parentIdAttempt = GetId(key!.Value, objectType);
        id = parentIdAttempt.Result;
        return parentIdAttempt.Success;
    }

    // gets the object type, throws if not supported
    private UmbracoObjectTypes GetObjectType(Type? type)
    {
        if (type?.FullName == null || !_objectTypes.TryGetValue(type.FullName, out UmbracoObjectTypes objType))
        {
            throw new NotSupportedException($"Type \"{type?.FullName ?? "<null>"}\" is not supported here.");
        }

        return objType;
    }

    private IEnumerable<IEntitySlim> GetPagedChildren(
        int id,
        UmbracoObjectTypes objectType,
        long pageIndex,
        int pageSize,
        bool trashed,
        IQuery<IUmbracoEntity>? filter,
        Ordering? ordering,
        out long totalRecords)
    {
        using (ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            IQuery<IUmbracoEntity> query = Query<IUmbracoEntity>().Where(x => x.ParentId == id && x.Trashed == trashed);

            if (pageSize == 0)
            {
                totalRecords = _entityRepository.CountByQuery(query, objectType.GetGuid(), filter);
                return Enumerable.Empty<IEntitySlim>();
            }

            return _entityRepository.GetPagedResultsByQuery(query, objectType.GetGuid(), pageIndex, pageSize, out totalRecords, filter, ordering);
        }
    }

    public IEnumerable<IEntitySlim> GetPagedChildren(
        Guid? parentKey,
        IEnumerable<UmbracoObjectTypes> parentObjectTypes,
        IEnumerable<UmbracoObjectTypes> childObjectTypes,
        int skip,
        int take,
        bool trashed,
        out long totalRecords,
        IQuery<IUmbracoEntity>? filter = null,
        Ordering? ordering = null)
    {
        using (ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            var parentId = 0;
            var parentIdResolved = parentObjectTypes.Any(parentObjectType => ResolveKey(parentKey, parentObjectType, out parentId));
            if (parentIdResolved is false)
            {
                totalRecords = 0;
                return Enumerable.Empty<IEntitySlim>();
            }

            if (take == 0)
            {
                totalRecords = CountChildren(parentId, childObjectTypes, filter: filter);
                return Array.Empty<IEntitySlim>();
            }

            IQuery<IUmbracoEntity> query = Query<IUmbracoEntity>().Where(x => x.ParentId == parentId && x.Trashed == trashed);

            PaginationHelper.ConvertSkipTakeToPaging(skip, take, out var pageNumber, out var pageSize);

            var objectTypeGuids = childObjectTypes.Select(x => x.GetGuid()).ToHashSet();
            if (pageSize == 0)
            {
                totalRecords = _entityRepository.CountByQuery(query, objectTypeGuids, filter);
                return Enumerable.Empty<IEntitySlim>();
            }

            return _entityRepository.GetPagedResultsByQuery(query, objectTypeGuids, pageNumber, pageSize, out totalRecords, filter, ordering);
        }
    }

    /// <inheritdoc/>>
    public Guid[] GetPathKeys(ITreeEntity entity, bool omitSelf = false)
    {
        IEnumerable<int> ids = entity.Path.Split(Constants.CharArrays.Comma, StringSplitOptions.RemoveEmptyEntries)
            .Select(x => int.TryParse(x, NumberStyles.Integer, CultureInfo.InvariantCulture, out var val) ? val : -1)
            .Where(x => x != -1);

        Guid[] keys = ids
            .Select(x => _idKeyMap.GetKeyForId(x, UmbracoObjectTypes.Document))
            .Where(x => x.Success)
            .Select(x => x.Result)
            .ToArray();

        if (omitSelf)
        {
            // Omit the last path key as that will be for the item itself.
            return keys.Take(keys.Length - 1).ToArray();
        }

        return keys;
    }
}
