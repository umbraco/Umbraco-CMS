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

            return _entityRepository.GetPagedResultsByQuery(query, objectType.GetGuid(), pageIndex, pageSize, out totalRecords, filter, ordering);
        }
    }
}

