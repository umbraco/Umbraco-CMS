using System.Data;
using System.Globalization;
using Microsoft.Extensions.Logging;
using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Exceptions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Persistence.Factories;
using Umbraco.Cms.Infrastructure.Persistence.Querying;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;
using static Umbraco.Cms.Core.Persistence.SqlExtensionsStatics;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

/// <summary>
///     Represents a repository for doing CRUD operations for <see cref="DataType" />
/// </summary>
internal sealed class DataTypeRepository : EntityRepositoryBase<int, IDataType>, IDataTypeRepository
{
    private readonly ILogger<IDataType> _dataTypeLogger;
    private readonly PropertyEditorCollection _editors;
    private readonly IConfigurationEditorJsonSerializer _serializer;
    private readonly IDataValueEditorFactory _dataValueEditorFactory;
    private readonly DataTypeByGuidReadRepository _dataTypeByGuidReadRepository;

    public DataTypeRepository(
        IScopeAccessor scopeAccessor,
        AppCaches cache,
        PropertyEditorCollection editors,
        ILogger<DataTypeRepository> logger,
        ILoggerFactory loggerFactory,
        IConfigurationEditorJsonSerializer serializer,
        IRepositoryCacheVersionService repositoryCacheVersionService,
        ICacheSyncService cacheSyncService,
        IDataValueEditorFactory dataValueEditorFactory)
        : base(
            scopeAccessor,
            cache,
            logger,
            repositoryCacheVersionService,
            cacheSyncService
            )
    {
        _editors = editors;
        _serializer = serializer;
        _dataValueEditorFactory = dataValueEditorFactory;
        _dataTypeLogger = loggerFactory.CreateLogger<IDataType>();
        _dataTypeByGuidReadRepository = new DataTypeByGuidReadRepository(
            this,
            scopeAccessor,
            cache,
            loggerFactory.CreateLogger<DataTypeByGuidReadRepository>(),
            repositoryCacheVersionService,
            cacheSyncService);
    }

    private Guid NodeObjectTypeId => Constants.ObjectTypes.DataType;

    public IDataType? Get(Guid key) => _dataTypeByGuidReadRepository.Get(key);

    public override void Save(IDataType entity)
    {
        base.Save(entity);

        // Also populate the GUID cache so subsequent lookups by GUID don't hit the database.
        _dataTypeByGuidReadRepository.PopulateCacheByKey(entity);
    }

    public override void Delete(IDataType entity)
    {
        base.Delete(entity);

        // Also clear the GUID cache so subsequent lookups by GUID don't return stale data.
        _dataTypeByGuidReadRepository.ClearCacheByKey(entity.Key);
    }

    public IEnumerable<MoveEventInfo<IDataType>> Move(IDataType toMove, EntityContainer? container)
    {
        var parentId = -1;
        if (container != null)
        {
            // Check on paths
            if (string.Format(",{0},", container.Path)
                    .IndexOf(string.Format(",{0},", toMove.Id), StringComparison.Ordinal) > -1)
            {
                throw new DataOperationException<MoveOperationStatusType>(
                    MoveOperationStatusType.FailedNotAllowedByPath);
            }

            parentId = container.Id;
        }

        // used to track all the moved entities to be given to the event
        // FIXME: Use constructor that takes parent key when this method is refactored
        var moveInfo = new List<MoveEventInfo<IDataType>> { new(toMove, toMove.Path, parentId) };

        var origPath = toMove.Path;

        // do the move to a new parent
        toMove.ParentId = parentId;

        // set the updated path
        toMove.Path = string.Concat(container == null ? parentId.ToInvariantString() : container.Path, ",", toMove.Id);

        // schedule it for updating in the transaction
        Save(toMove);

        // update all descendants from the original path, update in order of level
        IEnumerable<IDataType> descendants =
            Get(Query<IDataType>().Where(type => type.Path.StartsWith(origPath + ",")));

        IDataType lastParent = toMove;
        if (descendants is not null)
        {
            foreach (IDataType descendant in descendants.OrderBy(x => x.Level))
            {
                // FIXME: Use constructor that takes parent key when this method is refactored
                moveInfo.Add(new MoveEventInfo<IDataType>(descendant, descendant.Path, descendant.ParentId));

                descendant.ParentId = lastParent.Id;
                descendant.Path = string.Concat(lastParent.Path, ",", descendant.Id);

                // schedule it for updating in the transaction
                Save(descendant);
            }
        }

        return moveInfo;
    }

    public IReadOnlyDictionary<Udi, IEnumerable<string>> FindUsages(int id)
    {
        if (id == default)
        {
            return new Dictionary<Udi, IEnumerable<string>>();
        }

        Sql<ISqlContext> sql = Sql()
            .Select<ContentTypeDto>(ct => ct.Select(node => node.NodeDto))
            .AndSelect<PropertyTypeDto>(pt => Alias(pt.Alias, "ptAlias"), pt => Alias(pt.Name, "ptName"))
            .From<PropertyTypeDto>()
            .InnerJoin<ContentTypeDto>().On<ContentTypeDto, PropertyTypeDto>(ct => ct.NodeId, pt => pt.ContentTypeId)
            .InnerJoin<NodeDto>().On<NodeDto, ContentTypeDto>(n => n.NodeId, ct => ct.NodeId)
            .Where<PropertyTypeDto>(pt => pt.DataTypeId == id)
            .OrderBy<NodeDto>(node => node.NodeId)
            .AndBy<PropertyTypeDto>(pt => pt.Alias);

        List<ContentTypeReferenceDto>? dtos =
            Database.FetchOneToMany<ContentTypeReferenceDto>(ct => ct.PropertyTypes, sql);

        return dtos.ToDictionary(
            x => (Udi)new GuidUdi(ObjectTypes.GetUdiType(x.NodeDto.NodeObjectType!.Value), x.NodeDto.UniqueId)
                .EnsureClosed(),
            x => (IEnumerable<string>)x.PropertyTypes.Select(p => p.Alias).ToList());
    }

    public IReadOnlyDictionary<Udi, IEnumerable<string>> FindListViewUsages(int id)
    {
        var usages = new Dictionary<Udi, IEnumerable<string>>();

        if (id == default)
        {
            return usages;
        }

        IDataType? dataType = Get(id);

        if (dataType != null && dataType.EditorAlias.Equals(Constants.PropertyEditors.Aliases.ListView))
        {
            // Get All contentTypes where isContainer (list view enabled) is set to true
            Sql<ISqlContext> sql = Sql()
           .Select<ContentTypeDto>(ct => ct.Select(node => node.NodeDto))
           .From<ContentTypeDto>()
           .InnerJoin<NodeDto>().On<NodeDto, ContentTypeDto>(n => n.NodeId, ct => ct.NodeId)
           .Where<ContentTypeDto>(ct => ct.ListView != null);

            List<ContentTypeDto> ctds = Database.Fetch<ContentTypeDto>(sql);

            // If there are not any ContentTypes with a ListView return.
            if (!ctds.Any())
            {
                return usages;
            }

            // First check if it is a custom list view
            ContentTypeDto? customListView = ctds.Where(x => (Constants.Conventions.DataTypes.ListViewPrefix + x.Alias).Equals(dataType.Name)).FirstOrDefault();

            if (customListView != null)
            {
                // Add usages as customListView
                usages.Add(
                    new GuidUdi(ObjectTypes.GetUdiType(customListView.NodeDto.NodeObjectType!.Value), customListView.NodeDto.UniqueId),
                    new List<string> { dataType.Name! });
            }
            else
            {
                // It is not a custom ListView, so check the default ones.
                foreach (ContentTypeDto contentWithListView in ctds)
                {
                    var customListViewName = Constants.Conventions.DataTypes.ListViewPrefix + contentWithListView.Alias;
                    IDataType? clv = Get(Query<IDataType>().Where(x => x.Name == customListViewName))?.FirstOrDefault();

                    // Check if the content type has a custom listview (extra check to prevent duplicates)
                    if (clv == null)
                    {
                        // ContentType has no custom listview so it uses the default one
                        var udi = new GuidUdi(ObjectTypes.GetUdiType(contentWithListView.NodeDto.NodeObjectType!.Value), contentWithListView.NodeDto.UniqueId);
                        var listViewType = new List<string>();

                        if (dataType.Id.Equals(Constants.DataTypes.DefaultContentListView) && udi.EntityType == ObjectTypes.GetUdiType(UmbracoObjectTypes.DocumentType))
                        {
                            listViewType.Add(Constants.Conventions.DataTypes.ListViewPrefix + "Content");
                        }
                        else if (dataType.Id.Equals(Constants.DataTypes.DefaultMediaListView) && udi.EntityType == ObjectTypes.GetUdiType(UmbracoObjectTypes.MediaType))
                        {
                            listViewType.Add(Constants.Conventions.DataTypes.ListViewPrefix + "Media");
                        }
                        else if (dataType.Id.Equals(Constants.DataTypes.DefaultMembersListView) && udi.EntityType == ObjectTypes.GetUdiType(UmbracoObjectTypes.MemberType))
                        {
                            listViewType.Add(Constants.Conventions.DataTypes.ListViewPrefix + "Members");
                        }

                        if (listViewType.Any())
                        {
                            var added = usages.TryAdd(udi, listViewType);
                            if (!added)
                            {
                                usages[udi] = usages[udi].Append(dataType.Name!);
                            }
                        }
                    }
                }

            }
        }

        return usages;
    }

    #region Overrides of RepositoryBase<int,DataTypeDefinition>

    protected override IDataType? PerformGet(int id)
    {
        IDataType? dataType = GetMany(id).FirstOrDefault();

        if (dataType != null)
        {
            // Also populate the GUID cache so subsequent lookups by GUID don't hit the database.
            _dataTypeByGuidReadRepository.PopulateCacheByKey(dataType);
        }

        return dataType;
    }

    private string? EnsureUniqueNodeName(string? nodeName, int id = 0)
    {
        SqlTemplate template = SqlContext.Templates.Get(
            Constants.SqlTemplates.DataTypeRepository.EnsureUniqueNodeName,
            tsql => tsql
                .Select<NodeDto>(x => Alias(x.NodeId, "id"), x => Alias(x.Text, "name"))
                .From<NodeDto>()
                .Where<NodeDto>(x => x.NodeObjectType == SqlTemplate.Arg<Guid>("nodeObjectType")));

        Sql<ISqlContext> sql = template.Sql(NodeObjectTypeId);
        List<SimilarNodeName>? names = Database.Fetch<SimilarNodeName>(sql);

        return SimilarNodeName.GetUniqueName(names, id, nodeName);
    }

    [TableName(Constants.DatabaseSchema.Tables.ContentType)]
    private sealed class ContentTypeReferenceDto : ContentTypeDto
    {
        [ResultColumn]
        [Reference(ReferenceType.Many)]
        public List<PropertyTypeReferenceDto> PropertyTypes { get; } = null!;
    }

    [TableName(Constants.DatabaseSchema.Tables.PropertyType)]
    private sealed class PropertyTypeReferenceDto
    {
        [Column("ptAlias")]
        public string? Alias { get; set; }

        [Column("ptName")]
        public string? Name { get; set; }
    }

    protected override IEnumerable<IDataType> PerformGetAll(params int[]? ids)
    {
        Sql<ISqlContext> dataTypeSql = GetBaseQuery(false);

        if (ids?.Any() ?? false)
        {
            dataTypeSql.WhereIn<NodeDto>(w => w.NodeId, ids);
        }
        else
        {
            dataTypeSql.Where<NodeDto>(x => x.NodeObjectType == NodeObjectTypeId);
        }

        List<DataTypeDto>? dtos = Database.Fetch<DataTypeDto>(dataTypeSql);
        IDataType[] dataTypes = dtos.Select(x => DataTypeFactory.BuildEntity(
            x,
            _editors,
            _dataTypeLogger,
            _serializer,
            _dataValueEditorFactory)).ToArray();

        // Also populate the GUID cache so subsequent lookups by GUID don't hit the database.
        _dataTypeByGuidReadRepository.PopulateCacheByKey(dataTypes);

        return dataTypes;
    }

    protected override IEnumerable<IDataType> PerformGetByQuery(IQuery<IDataType> query)
    {
        Sql<ISqlContext> sqlClause = GetBaseQuery(false);
        var translator = new SqlTranslator<IDataType>(sqlClause, query);
        Sql<ISqlContext> sql = translator.Translate();

        List<DataTypeDto>? dtos = Database.Fetch<DataTypeDto>(sql);

        return dtos.Select(x => DataTypeFactory.BuildEntity(
            x,
            _editors,
            _dataTypeLogger,
            _serializer,
            _dataValueEditorFactory)).ToArray();
    }

    #endregion

    #region Overrides of EntityRepositoryBase<int,DataTypeDefinition>

    protected override Sql<ISqlContext> GetBaseQuery(bool isCount)
    {
        Sql<ISqlContext> sql = Sql();

        sql = isCount
            ? sql.SelectCount()
            : sql.Select<DataTypeDto>(r => r.Select(x => x.NodeDto));

        sql
            .From<DataTypeDto>()
            .InnerJoin<NodeDto>()
            .On<DataTypeDto, NodeDto>(left => left.NodeId, right => right.NodeId)
            .Where<NodeDto>(x => x.NodeObjectType == NodeObjectTypeId);
        return sql;
    }

    protected override string GetBaseWhereClause() => $"{QuoteTableName("umbracoNode")}.id = @id";

    protected override IEnumerable<string> GetDeleteClauses() => Array.Empty<string>();

    #endregion

    #region Unit of Work Implementation

    protected override void PersistNewItem(IDataType entity)
    {
        entity.AddingEntity();

        // ensure a datatype has a unique name before creating it
        entity.Name = EnsureUniqueNodeName(entity.Name)!;

        // TODO: should the below be removed?
        // Cannot add a duplicate data type
        Sql<ISqlContext> existsSql = Sql()
            .SelectCount()
            .From<DataTypeDto>()
            .InnerJoin<NodeDto>().On<DataTypeDto, NodeDto>((left, right) => left.NodeId == right.NodeId)
            .Where<NodeDto>(x => x.Text == entity.Name);
        var exists = Database.ExecuteScalar<int>(existsSql) > 0;
        if (exists)
        {
            throw new DuplicateNameException("A data type with the name " + entity.Name + " already exists");
        }

        DataTypeDto dto = DataTypeFactory.BuildDto(entity, _serializer);

        // Logic for setting Path, Level and SortOrder
        Sql<ISqlContext> sql = Sql()
            .SelectAll()
            .From<NodeDto>()
            .Where<NodeDto>(x => x.NodeId == entity.ParentId);
        NodeDto parent = Database.First<NodeDto>(sql);
        var level = parent.Level + 1;
        sql = Sql()
            .SelectCount()
            .From<NodeDto>()
            .Where<NodeDto>(x => x.ParentId == entity.ParentId && x.NodeObjectType == NodeObjectTypeId);
        var sortOrder = Database.ExecuteScalar<int>(sql);

        // Create the (base) node data - umbracoNode
        NodeDto nodeDto = dto.NodeDto;
        nodeDto.Path = parent.Path;
        nodeDto.Level = short.Parse(level.ToString(CultureInfo.InvariantCulture));
        nodeDto.SortOrder = sortOrder;
        var o = Database.IsNew(nodeDto) ? Convert.ToInt32(Database.Insert(nodeDto)) : Database.Update(nodeDto);

        // Update with new correct path
        nodeDto.Path = string.Concat(nodeDto.Path, ",", nodeDto.NodeId);
        Database.Update(nodeDto);

        // Update entity with correct values
        entity.Id = nodeDto.NodeId; // Set Id on entity to ensure an Id is set
        entity.Path = nodeDto.Path;
        entity.SortOrder = sortOrder;
        entity.Level = level;

        dto.NodeId = nodeDto.NodeId;
        Database.Insert(dto);

        entity.ResetDirtyProperties();
    }

    protected override void PersistUpdatedItem(IDataType entity)
    {
        entity.Name = EnsureUniqueNodeName(entity.Name, entity.Id)!;

        // Cannot change to a duplicate alias
        Sql<ISqlContext> existsSql = Sql()
            .SelectCount()
            .From<DataTypeDto>()
            .InnerJoin<NodeDto>().On<DataTypeDto, NodeDto>((left, right) => left.NodeId == right.NodeId)
            .Where<NodeDto>(x => x.Text == entity.Name && x.NodeId != entity.Id);
        var exists = Database.ExecuteScalar<int>(existsSql) > 0;
        if (exists)
        {
            throw new DuplicateNameException("A data type with the name " + entity.Name + " already exists");
        }

        // Updates Modified date
        entity.UpdatingEntity();

        // Look up parent to get and set the correct Path if ParentId has changed
        if (entity.IsPropertyDirty("ParentId"))
        {
            Sql<ISqlContext> sql = Sql()
                .SelectAll()
                .From<NodeDto>()
                .Where<NodeDto>(x => x.NodeId == entity.ParentId);
            NodeDto? parent = Database.FirstOrDefault<NodeDto>(sql);
            entity.Path = string.Concat(parent?.Path ?? "-1", ",", entity.Id);
            entity.Level = (parent?.Level ?? 0) + 1;

            sql = Sql()
                .SelectMax<NodeDto>(c => c.SortOrder, 0)
                .From<NodeDto>()
                .Where<NodeDto>(x => x.ParentId == entity.ParentId && x.NodeObjectType == NodeObjectTypeId);
            var maxSortOrder = Database.ExecuteScalar<int>(sql);
            entity.SortOrder = maxSortOrder + 1;
        }

        DataTypeDto dto = DataTypeFactory.BuildDto(entity, _serializer);

        // Updates the (base) node data - umbracoNode
        NodeDto nodeDto = dto.NodeDto;
        Database.Update(nodeDto);
        Database.Update(dto);

        entity.ResetDirtyProperties();
    }

    protected override void PersistDeletedItem(IDataType entity)
    {
        Sql<ISqlContext> sql;

        // Remove Notifications
        sql = Sql().Delete<User2NodeNotifyDto>(x => x.NodeId == entity.Id);
        Database.Execute(sql);

        // Remove Permissions
        sql = Sql().Delete<UserGroup2GranularPermissionDto>(x => x.UniqueId == entity.Key);
        Database.Execute(sql);

        // Remove associated tags
        sql = Sql().Delete<TagRelationshipDto>(x => x.NodeId == entity.Id);
        Database.Execute(sql);

        // PropertyTypes containing the DataType being deleted
        sql = Sql()
            .SelectAll()
            .From<PropertyTypeDto>()
            .Where<PropertyTypeDto>(x => x.DataTypeId == entity.Id);
        List<PropertyTypeDto>? propertyTypeDtos = Database.Fetch<PropertyTypeDto>(sql);

        // Go through the PropertyTypes and delete referenced PropertyData before deleting the PropertyType
        foreach (PropertyTypeDto? dto in propertyTypeDtos)
        {
            sql = Sql().Delete<PropertyDataDto>(x => x.PropertyTypeId == dto.Id);
            Database.Execute(sql);

            sql = Sql().Delete<PropertyTypeDto>(x => x.Id == dto.Id);
            Database.Execute(sql);
        }

        // Delete Content specific data
        sql = Sql().Delete<DataTypeDto>(x => x.NodeId == entity.Id);
        Database.Execute(sql);

        // Delete (base) node data
        sql = Sql().Delete<NodeDto>(x => x.UniqueId == entity.Key);
        Database.Execute(sql);

        entity.DeleteDate = DateTime.UtcNow;
    }

    #endregion

    #region Read Repository implementation for Guid keys

    /// <summary>
    /// Populates the int-keyed cache with the given entity.
    /// This allows entities retrieved by GUID to also be cached for int ID lookups.
    /// </summary>
    private void PopulateCacheById(IDataType entity)
    {
        if (entity.HasIdentity)
        {
            var cacheKey = GetCacheKey(entity.Id);
            IsolatedCache.Insert(cacheKey, () => entity, TimeSpan.FromMinutes(5), true);
        }
    }

    /// <summary>
    /// Populates the int-keyed cache with the given entities.
    /// This allows entities retrieved by GUID to also be cached for int ID lookups.
    /// </summary>
    private void PopulateCacheById(IEnumerable<IDataType> entities)
    {
        foreach (IDataType entity in entities)
        {
            PopulateCacheById(entity);
        }
    }

    private static string GetCacheKey(int id) => RepositoryCacheKeys.GetKey<IDataType>() + id;

    // reading repository purely for looking up by GUID
    private sealed class DataTypeByGuidReadRepository : EntityRepositoryBase<Guid, IDataType>
    {
        private readonly DataTypeRepository _outerRepo;

        public DataTypeByGuidReadRepository(
            DataTypeRepository outerRepo,
            IScopeAccessor scopeAccessor,
            AppCaches cache,
            ILogger<DataTypeByGuidReadRepository> logger,
            IRepositoryCacheVersionService repositoryCacheVersionService,
            ICacheSyncService cacheSyncService)
            : base(
                scopeAccessor,
                cache,
                logger,
                repositoryCacheVersionService,
                cacheSyncService) =>
            _outerRepo = outerRepo;

        protected override IDataType? PerformGet(Guid id)
        {
            Sql<ISqlContext> sql = _outerRepo.GetBaseQuery(false)
                .Where<NodeDto>(x => x.UniqueId == id);

            DataTypeDto? dto = Database.FirstOrDefault<DataTypeDto>(sql);

            if (dto == null)
            {
                return null;
            }

            IDataType dataType = DataTypeFactory.BuildEntity(
                dto,
                _outerRepo._editors,
                _outerRepo._dataTypeLogger,
                _outerRepo._serializer,
                _outerRepo._dataValueEditorFactory);

            // Also populate the int-keyed cache so subsequent lookups by int ID don't hit the database
            _outerRepo.PopulateCacheById(dataType);

            return dataType;
        }

        protected override IEnumerable<IDataType> PerformGetAll(params Guid[]? ids)
        {
            Sql<ISqlContext> sql = _outerRepo.GetBaseQuery(false);
            if (ids?.Length > 0)
            {
                sql.WhereIn<NodeDto>(x => x.UniqueId, ids);
            }
            else
            {
                sql.Where<NodeDto>(x => x.NodeObjectType == _outerRepo.NodeObjectTypeId);
            }

            List<DataTypeDto>? dtos = Database.Fetch<DataTypeDto>(sql);
            IDataType[] dataTypes = dtos.Select(x => DataTypeFactory.BuildEntity(
                x,
                _outerRepo._editors,
                _outerRepo._dataTypeLogger,
                _outerRepo._serializer,
                _outerRepo._dataValueEditorFactory)).ToArray();

            // Also populate the int-keyed cache so subsequent lookups by int ID don't hit the database
            _outerRepo.PopulateCacheById(dataTypes);

            return dataTypes;
        }

        protected override IEnumerable<IDataType> PerformGetByQuery(IQuery<IDataType> query) =>
            throw new InvalidOperationException("This method won't be implemented.");

        protected override IEnumerable<string> GetDeleteClauses() =>
            throw new InvalidOperationException("This method won't be implemented.");

        protected override void PersistNewItem(IDataType entity) =>
            throw new InvalidOperationException("This method won't be implemented.");

        protected override void PersistUpdatedItem(IDataType entity) =>
            throw new InvalidOperationException("This method won't be implemented.");

        protected override Sql<ISqlContext> GetBaseQuery(bool isCount) =>
            throw new InvalidOperationException("This method won't be implemented.");

        protected override string GetBaseWhereClause() =>
            throw new InvalidOperationException("This method won't be implemented.");

        /// <summary>
        /// Populates the GUID-keyed cache with the given entity.
        /// This allows entities retrieved by int ID to also be cached for GUID lookups.
        /// </summary>
        public void PopulateCacheByKey(IDataType entity)
        {
            if (entity.HasIdentity)
            {
                var cacheKey = GetCacheKey(entity.Key);
                IsolatedCache.Insert(cacheKey, () => entity, TimeSpan.FromMinutes(5), true);
            }
        }

        /// <summary>
        /// Populates the GUID-keyed cache with the given entities.
        /// This allows entities retrieved by int ID to also be cached for GUID lookups.
        /// </summary>
        public void PopulateCacheByKey(IEnumerable<IDataType> entities)
        {
            foreach (IDataType entity in entities)
            {
                PopulateCacheByKey(entity);
            }
        }

        /// <summary>
        /// Clears the GUID-keyed cache entry for the given key.
        /// This ensures deleted entities are not returned from the cache.
        /// </summary>
        public void ClearCacheByKey(Guid key)
        {
            var cacheKey = GetCacheKey(key);
            IsolatedCache.Clear(cacheKey);
        }

        private static string GetCacheKey(Guid key) => RepositoryCacheKeys.GetKey<IDataType>() + key;
    }

    #endregion
}
