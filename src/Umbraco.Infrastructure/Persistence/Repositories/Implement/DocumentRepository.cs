using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Persistence.Factories;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

/// <summary>
///     Represents a repository for doing CRUD operations for <see cref="IContent" />.
/// </summary>
internal class DocumentRepository : PublishableContentRepositoryBase<IContent, DocumentRepository, DocumentDto, DocumentVersionDto, DocumentCultureVariationDto>, IDocumentRepository
{
    private readonly AppCaches _appCaches;
    private readonly IContentTypeRepository _contentTypeRepository;
    private readonly ILoggerFactory _loggerFactory;
    private readonly IScopeAccessor _scopeAccessor;
    private readonly IRepositoryCacheVersionService _repositoryCacheVersionService;
    private readonly ICacheSyncService _cacheSyncService;
    private readonly ITemplateRepository _templateRepository;
    private PermissionRepository<IContent>? _permissionRepository;

    public DocumentRepository(
        IScopeAccessor scopeAccessor,
        AppCaches appCaches,
        ILogger<DocumentRepository> logger,
        ILoggerFactory loggerFactory,
        IContentTypeRepository contentTypeRepository,
        ITemplateRepository templateRepository,
        ITagRepository tagRepository,
        ILanguageRepository languageRepository,
        IRelationRepository relationRepository,
        IRelationTypeRepository relationTypeRepository,
        PropertyEditorCollection propertyEditors,
        DataValueReferenceFactoryCollection dataValueReferenceFactories,
        IDataTypeService dataTypeService,
        IJsonSerializer serializer,
        IEventAggregator eventAggregator,
        IRepositoryCacheVersionService repositoryCacheVersionService,
        ICacheSyncService cacheSyncService)
        : base(
            scopeAccessor,
            appCaches,
            logger,
            loggerFactory,
            tagRepository,
            languageRepository,
            relationRepository,
            relationTypeRepository,
            propertyEditors,
            dataValueReferenceFactories,
            dataTypeService,
            serializer,
            eventAggregator,
            repositoryCacheVersionService,
            cacheSyncService)
    {
        _contentTypeRepository =
            contentTypeRepository ?? throw new ArgumentNullException(nameof(contentTypeRepository));
        _templateRepository = templateRepository ?? throw new ArgumentNullException(nameof(templateRepository));
        _repositoryCacheVersionService = repositoryCacheVersionService;
        _cacheSyncService = cacheSyncService;
        _appCaches = appCaches;
        _loggerFactory = loggerFactory;
        _scopeAccessor = scopeAccessor;
    }

    [Obsolete("Please use the constructor with all parameters. Scheduled for removal in Umbraco 18.")]
    public DocumentRepository(
        IScopeAccessor scopeAccessor,
        AppCaches appCaches,
        ILogger<DocumentRepository> logger,
        ILoggerFactory loggerFactory,
        IContentTypeRepository contentTypeRepository,
        ITemplateRepository templateRepository,
        ITagRepository tagRepository,
        ILanguageRepository languageRepository,
        IRelationRepository relationRepository,
        IRelationTypeRepository relationTypeRepository,
        PropertyEditorCollection propertyEditors,
        DataValueReferenceFactoryCollection dataValueReferenceFactories,
        IDataTypeService dataTypeService,
        IJsonSerializer serializer,
        IEventAggregator eventAggregator)
        : this(
            scopeAccessor,
            appCaches,
            logger,
            loggerFactory,
            contentTypeRepository,
            templateRepository,
            tagRepository,
            languageRepository,
            relationRepository,
            relationTypeRepository,
            propertyEditors,
            dataValueReferenceFactories,
            dataTypeService,
            serializer,
            eventAggregator,
            StaticServiceProvider.Instance.GetRequiredService<IRepositoryCacheVersionService>(),
            StaticServiceProvider.Instance.GetRequiredService<ICacheSyncService>())
    {
    }

    protected override DocumentRepository This => this;

    // note: is ok to 'new' the repo here as it's a sub-repo really
    private PermissionRepository<IContent> PermissionRepository => _permissionRepository ??=
                                                                       new PermissionRepository<IContent>(
                                                                           _scopeAccessor,
                                                                           _appCaches,
                                                                           _loggerFactory.CreateLogger<PermissionRepository<IContent>>(),
                                                                           _repositoryCacheVersionService,
                                                                           _cacheSyncService);

    protected override IEnumerable<IContent> MapDtosToContent(
        List<DocumentDto> dtos,
        bool withCache = false,
        string[]? propertyAliases = null,
        bool loadTemplates = true,
        bool loadVariants = true)
    {
        var temps = new List<TempContent<Content>>();
        var contentTypes = new Dictionary<int, IContentType?>();
        var templateIds = new List<int>();

        var content = new Content[dtos.Count];

        for (var i = 0; i < dtos.Count; i++)
        {
            DocumentDto dto = dtos[i];

            if (withCache)
            {
                // if the cache contains the (proper version of the) item, use it
                IContent? cached =
                    IsolatedCache.GetCacheItem<IContent>(RepositoryCacheKeys.GetKey<IContent, int>(dto.NodeId));
                if (cached != null && cached.VersionId == dto.ContentVersionDto.ContentVersionDto.Id)
                {
                    content[i] = (Content)cached;
                    continue;
                }
            }

            // else, need to build it

            // get the content type - the repository is full cache *but* still deep-clones
            // whatever comes out of it, so use our own local index here to avoid this
            var contentTypeId = dto.ContentDto.ContentTypeId;
            if (contentTypes.TryGetValue(contentTypeId, out IContentType? contentType) == false)
            {
                contentTypes[contentTypeId] = contentType = _contentTypeRepository.Get(contentTypeId);
            }

            Content c = content[i] = ContentBaseFactory.BuildEntity(dto, contentType);

            if (loadTemplates)
            {
                // need templates
                var templateId = dto.ContentVersionDto.TemplateId;
                if (templateId.HasValue)
                {
                    templateIds.Add(templateId.Value);
                }

                if (dto.Published)
                {
                    templateId = dto.PublishedVersionDto!.TemplateId;
                    if (templateId.HasValue)
                    {
                        templateIds.Add(templateId.Value);
                    }
                }
            }

            // need temps, for properties, templates and variations
            var versionId = dto.ContentVersionDto.Id;
            var publishedVersionId = dto.Published ? dto.PublishedVersionDto!.Id : 0;
            var temp = new TempContent<Content>(dto.NodeId, versionId, publishedVersionId, contentType, c)
            {
                Template1Id = dto.ContentVersionDto.TemplateId
            };
            if (dto.Published)
            {
                temp.Template2Id = dto.PublishedVersionDto!.TemplateId;
            }

            temps.Add(temp);
        }

        Dictionary<int, ITemplate>? templates = null;
        if (loadTemplates)
        {
            // load all required templates in 1 query, and index
            templates = _templateRepository.GetMany(templateIds.ToArray())?
                .ToDictionary(x => x.Id, x => x);
        }

        // An empty array of propertyAliases indicates that no properties need to be loaded (null = load all properties).
        var loadProperties = propertyAliases is { Length: 0 } is false;

        IDictionary<int, PropertyCollection>? properties = null;
        if (loadProperties)
        {
            // load properties for all documents from database in 1 query - indexed by version id
            // if propertyAliases is provided, only load those specific properties
            properties = GetPropertyCollections(temps, propertyAliases);
        }

        // assign templates and properties
        foreach (TempContent<Content> temp in temps)
        {
            if (loadTemplates)
            {
                // set the template ID if it matches an existing template
                if (temp.Template1Id.HasValue && (templates?.ContainsKey(temp.Template1Id.Value) ?? false))
                {
                    temp.Content!.TemplateId = temp.Template1Id;
                }

                if (temp.Template2Id.HasValue && (templates?.ContainsKey(temp.Template2Id.Value) ?? false))
                {
                    temp.Content!.PublishTemplateId = temp.Template2Id;
                }
            }

            // set properties
            if (loadProperties)
            {
                if (properties?.ContainsKey(temp.VersionId) ?? false)
                {
                    temp.Content!.Properties = properties[temp.VersionId];
                }
                else
                {
                    throw new InvalidOperationException($"No property data found for version: '{temp.VersionId}'.");
                }
            }
            else
            {
                // When loadProperties is false (propertyAliases is empty array), clear the property collection
                temp.Content!.Properties = new PropertyCollection();
            }
        }

        if (loadVariants)
        {
            // set variations, if varying
            temps = temps.Where(x => x.ContentType?.VariesByCulture() ?? false).ToList();
            if (temps.Count > 0)
            {
                // load all variations for all documents from database, in one query
                IDictionary<int, List<ContentVariation>> contentVariations = GetContentVariations(temps);
                IDictionary<int, List<EntityVariation>> documentVariations = GetEntityVariations(temps);
                foreach (TempContent<Content> temp in temps)
                {
                    SetVariations(temp.Content, contentVariations, documentVariations);
                }
            }
        }

        foreach (Content c in content)
        {
            c.ResetDirtyProperties(false); // reset dirty initial properties (U4-1946)
        }

        return content;
    }

    protected override IContent MapDtoToContent(DocumentDto dto)
    {
        IContentType? contentType = _contentTypeRepository.Get(dto.ContentDto.ContentTypeId);
        Content content = ContentBaseFactory.BuildEntity(dto, contentType);

        try
        {
            content.DisableChangeTracking();

            // get template
            if (dto.ContentVersionDto.TemplateId.HasValue)
            {
                content.TemplateId = dto.ContentVersionDto.TemplateId;
            }

            // get properties - indexed by version id
            var versionId = dto.ContentVersionDto.Id;

            // TODO: shall we get published properties or not?
            //var publishedVersionId = dto.Published ? dto.PublishedVersionDto.Id : 0;
            var publishedVersionId = dto.PublishedVersionDto?.Id ?? 0;

            var temp = new TempContent<Content>(dto.NodeId, versionId, publishedVersionId, contentType);
            var ltemp = new List<TempContent<Content>> { temp };
            IDictionary<int, PropertyCollection> properties = GetPropertyCollections(ltemp);
            content.Properties = properties[dto.ContentVersionDto.Id];

            // set variations, if varying
            if (contentType?.VariesByCulture() ?? false)
            {
                IDictionary<int, List<ContentVariation>> contentVariations = GetContentVariations(ltemp);
                IDictionary<int, List<EntityVariation>> documentVariations = GetEntityVariations(ltemp);
                SetVariations(content, contentVariations, documentVariations);
            }

            // reset dirty initial properties (U4-1946)
            content.ResetDirtyProperties(false);
            return content;
        }
        finally
        {
            content.EnableChangeTracking();
        }
    }

    #region Repository Base

    protected override Guid NodeObjectTypeId => Constants.ObjectTypes.Document;

    // TODO ELEMENTS: if this cannot be removed, make the one in the base protected
    // gets the COALESCE expression for variant/invariant name
    private string VariantNameSqlExpression
        => SqlContext.VisitDto<ContentVersionCultureVariationDto, NodeDto>((ccv, node) => ccv.Name ?? node.Text, "ccv")
            .Sql;

    protected override IEnumerable<string> GetDeleteClauses()
    {
        var nodeId = QuoteColumnName("nodeId");
        var uniqueId = QuoteColumnName("uniqueId");
        var umbracoNode = QuoteTableName(NodeDto.TableName);
        var list = new List<string>
    {
      $"DELETE FROM {QuoteTableName(Constants.DatabaseSchema.Tables.ContentSchedule)} WHERE {nodeId} = @id",
      $@"DELETE FROM {QuoteTableName(Constants.DatabaseSchema.Tables.RedirectUrl)} WHERE {QuoteColumnName("contentKey")} IN
        (SELECT {uniqueId} FROM {umbracoNode} WHERE id = @id)",
      $"DELETE FROM {QuoteTableName(Constants.DatabaseSchema.Tables.User2NodeNotify )} WHERE {nodeId} = @id",
      $@"DELETE FROM {QuoteTableName(Constants.DatabaseSchema.Tables.UserGroup2GranularPermission)} WHERE {uniqueId} IN
        (SELECT {uniqueId} FROM {umbracoNode} WHERE id = @id)",
      $"DELETE FROM {QuoteTableName(Constants.DatabaseSchema.Tables.UserStartNode)} WHERE {QuoteColumnName("startNode")} = @id",
      $@"UPDATE {QuoteTableName(Constants.DatabaseSchema.Tables.UserGroup)}
        SET {QuoteColumnName("startContentId")} = NULL
        WHERE {QuoteColumnName("startContentId")} = @id",
      $"DELETE FROM {QuoteTableName(Constants.DatabaseSchema.Tables.Relation)} WHERE {QuoteColumnName("parentId")} = @id",
      $"DELETE FROM {QuoteTableName(Constants.DatabaseSchema.Tables.Relation)} WHERE {QuoteColumnName("childId")} = @id",
      $"DELETE FROM {QuoteTableName(Constants.DatabaseSchema.Tables.TagRelationship)} WHERE {nodeId} = @id",
      $"DELETE FROM {QuoteTableName(Constants.DatabaseSchema.Tables.Domain)} WHERE {QuoteColumnName("domainRootStructureID")} = @id",
      $"DELETE FROM {QuoteTableName(Constants.DatabaseSchema.Tables.Document)} WHERE {nodeId} = @id",
      $"DELETE FROM {QuoteTableName(Constants.DatabaseSchema.Tables.DocumentCultureVariation)} WHERE {nodeId} = @id",
      $@"DELETE FROM {QuoteTableName(Constants.DatabaseSchema.Tables.DocumentVersion)} WHERE id IN
        (SELECT id FROM {QuoteTableName(Constants.DatabaseSchema.Tables.ContentVersion )} WHERE {nodeId} = @id)",
      $@"DELETE FROM {QuoteTableName(Constants.DatabaseSchema.Tables.PropertyData)} WHERE {QuoteColumnName("versionId")} IN
        (SELECT id FROM {QuoteTableName(Constants.DatabaseSchema.Tables.ContentVersion)} WHERE {nodeId} = @id)",
      $@"DELETE FROM {QuoteTableName(Constants.DatabaseSchema.Tables.ContentVersionCultureVariation)} WHERE {QuoteColumnName("versionId")} IN
        (SELECT id FROM {QuoteTableName(Constants.DatabaseSchema.Tables.ContentVersion)} WHERE {nodeId} = @id)",
      $"DELETE FROM {QuoteTableName(Constants.DatabaseSchema.Tables.ContentVersion)} WHERE {nodeId} = @id",
      $"DELETE FROM {QuoteTableName(Constants.DatabaseSchema.Tables.Content)} WHERE {nodeId} = @id",
      $@"DELETE FROM {QuoteTableName(Constants.DatabaseSchema.Tables.AccessRule)} WHERE {QuoteColumnName("accessId")} IN
        (SELECT id FROM {QuoteTableName(Constants.DatabaseSchema.Tables.Access)}
          WHERE {nodeId} = @id OR {QuoteColumnName("loginNodeId")} = @id OR {QuoteColumnName("noAccessNodeId")} = @id)",
      $"DELETE FROM {QuoteTableName(Constants.DatabaseSchema.Tables.Access)} WHERE {nodeId} = @id",
      $"DELETE FROM {QuoteTableName(Constants.DatabaseSchema.Tables.Access)} WHERE {QuoteColumnName("loginNodeId")} = @id",
      $"DELETE FROM {QuoteTableName(Constants.DatabaseSchema.Tables.Access)} WHERE {QuoteColumnName("noAccessNodeId")} = @id",
      $@"DELETE FROM {QuoteTableName(Constants.DatabaseSchema.Tables.DocumentUrl)} WHERE {uniqueId} IN
        (SELECT {uniqueId} FROM {umbracoNode} WHERE id = @id)",
      $@"DELETE FROM {QuoteTableName(Constants.DatabaseSchema.Tables.DocumentUrlAlias)} WHERE {uniqueId} IN
        (SELECT {uniqueId} FROM {umbracoNode} WHERE id = @id)",
      $"DELETE FROM {umbracoNode} WHERE id = @id",
        };
        return list;
    }

    #endregion

    #region Content Repository

    public int CountPublished(string? contentTypeAlias = null)
    {
        Sql<ISqlContext> sql = SqlContext.Sql();
        if (contentTypeAlias.IsNullOrWhiteSpace())
        {
            sql.SelectCount()
                .From<NodeDto>()
                .InnerJoin<DocumentDto>()
                .On<NodeDto, DocumentDto>(left => left.NodeId, right => right.NodeId)
                .Where<NodeDto>(x => x.NodeObjectType == NodeObjectTypeId && x.Trashed == false)
                .Where<DocumentDto>(x => x.Published);
        }
        else
        {
            sql.SelectCount()
                .From<NodeDto>()
                .InnerJoin<ContentDto>()
                .On<NodeDto, ContentDto>(left => left.NodeId, right => right.NodeId)
                .InnerJoin<DocumentDto>()
                .On<NodeDto, DocumentDto>(left => left.NodeId, right => right.NodeId)
                .InnerJoin<ContentTypeDto>()
                .On<ContentTypeDto, ContentDto>(left => left.NodeId, right => right.ContentTypeId)
                .Where<NodeDto>(x => x.NodeObjectType == NodeObjectTypeId && x.Trashed == false)
                .Where<ContentTypeDto>(x => x.Alias == contentTypeAlias)
                .Where<DocumentDto>(x => x.Published);
        }

        return Database.ExecuteScalar<int>(sql);
    }

    public void ReplaceContentPermissions(EntityPermissionSet permissionSet) =>
        PermissionRepository.ReplaceEntityPermissions(permissionSet);

    /// <summary>
    ///     Assigns a single permission to the current content item for the specified group ids
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="permission"></param>
    /// <param name="groupIds"></param>
    public void AssignEntityPermission(IContent entity, string permission, IEnumerable<int> groupIds) =>
        PermissionRepository.AssignEntityPermission(entity, permission, groupIds);

    public EntityPermissionCollection GetPermissionsForEntity(int entityId) =>
        PermissionRepository.GetPermissionsForEntity(entityId);

    /// <summary>
    ///     Used to add/update a permission for a content item
    /// </summary>
    /// <param name="permission"></param>
    public void AddOrUpdatePermissions(ContentPermissionSet permission) => PermissionRepository.Save(permission);

    /// <inheritdoc />
    [Obsolete("Please use the method overload with all parameters. Scheduled for removal in Umbraco 19.")]
    public override IEnumerable<IContent> GetPage(
        IQuery<IContent>? query,
        long pageIndex,
        int pageSize,
        out long totalRecords,
        IQuery<IContent>? filter,
        Ordering? ordering)
        => GetPage(query, pageIndex, pageSize, out totalRecords, propertyAliases: null, filter: filter, ordering: ordering, loadTemplates: true);

    /// <inheritdoc />
    public override IEnumerable<IContent> GetPage(
        IQuery<IContent>? query,
        long pageIndex,
        int pageSize,
        out long totalRecords,
        string[]? propertyAliases,
        IQuery<IContent>? filter,
        Ordering? ordering)
        => GetPage(query, pageIndex, pageSize, out totalRecords, propertyAliases, filter, ordering, loadTemplates: true);

    /// <inheritdoc />
    public IEnumerable<IContent> GetPage(
        IQuery<IContent>? query,
        long pageIndex,
        int pageSize,
        out long totalRecords,
        string[]? propertyAliases,
        IQuery<IContent>? filter,
        Ordering? ordering,
        bool loadTemplates)
    {
        Sql<ISqlContext>? filterSql = null;

        // if we have a filter, map its clauses to an Sql statement
        if (filter != null)
        {
            // if the clause works on "name", we need to swap the field and use the variantName instead,
            // so that querying also works on variant content (for instance when searching a listview).

            // figure out how the "name" field is going to look like - so we can look for it
            var nameField = SqlContext.VisitModelField<IContent>(x => x.Name);

            filterSql = Sql();
            foreach (Tuple<string, object[]> filterClause in filter.GetWhereClauses())
            {
                var clauseSql = filterClause.Item1;
                var clauseArgs = filterClause.Item2;

                // replace the name field
                // we cannot reference an aliased field in a WHERE clause, so have to repeat the expression here
                clauseSql = clauseSql.Replace(nameField, VariantNameSqlExpression);

                // append the clause
                filterSql.Append($"AND ({clauseSql})", clauseArgs);
            }
        }

        return GetPage<DocumentDto>(
            query,
            pageIndex,
            pageSize,
            out totalRecords,
            x => MapDtosToContent(x, propertyAliases: propertyAliases, loadTemplates: loadTemplates),
            filterSql,
            ordering);
    }

    public bool IsPathPublished(IContent? content)
    {
        // fail fast
        if (content?.Path.StartsWith("-1,-20,") ?? false)
        {
            return false;
        }

        // succeed fast
        if (content?.ParentId == -1)
        {
            return content.Published;
        }

        IEnumerable<int>? ids = content?.Path.Split(Constants.CharArrays.Comma).Skip(1)
            .Select(s => int.Parse(s, CultureInfo.InvariantCulture));

        Sql<ISqlContext> sql = SqlContext.Sql()
            .SelectCount<NodeDto>(x => x.NodeId)
            .From<NodeDto>()
            .InnerJoin<DocumentDto>().On<NodeDto, DocumentDto>((n, d) => n.NodeId == d.NodeId && d.Published)
            .WhereIn<NodeDto>(x => x.NodeId, ids);

        var count = Database.ExecuteScalar<int>(sql);
        return count == content?.Level;
    }

    #endregion

    #region Recycle Bin

    public override int RecycleBinId => Constants.System.RecycleBinContent;

    public bool RecycleBinSmells()
    {
        IAppPolicyCache cache = _appCaches.RuntimeCache;
        var cacheKey = CacheKeys.ContentRecycleBinCacheKey;

        // always cache either true or false
        return cache.GetCacheItem(cacheKey, () => CountChildren(RecycleBinId) > 0);
    }

    #endregion

    #region Schedule

    /// <inheritdoc />
    public void ClearSchedule(DateTime date)
    {
        Sql<ISqlContext> sql = Sql().Delete<ContentScheduleDto>().Where<ContentScheduleDto>(x => x.Date <= date);
        Database.Execute(sql);
    }

    /// <inheritdoc />
    public void ClearSchedule(DateTime date, ContentScheduleAction action)
    {
        var a = action.ToString();
        Sql<ISqlContext> sql = Sql().Delete<ContentScheduleDto>()
            .Where<ContentScheduleDto>(x => x.Date <= date && x.Action == a);
        Database.Execute(sql);
    }

    private Sql GetSqlForHasScheduling(ContentScheduleAction action, DateTime date)
    {
        SqlTemplate template = SqlContext.Templates.Get(
            "Umbraco.Core.DocumentRepository.GetSqlForHasScheduling",
            tsql => tsql
                .SelectCount()
                .From<ContentScheduleDto>()
                .Where<ContentScheduleDto>(x =>
                    x.Action == SqlTemplate.Arg<string>("action") && x.Date <= SqlTemplate.Arg<DateTime>("date")));

        Sql<ISqlContext> sql = template.Sql(action.ToString(), date);
        return sql;
    }

    public bool HasContentForExpiration(DateTime date)
    {
        Sql sql = GetSqlForHasScheduling(ContentScheduleAction.Expire, date);
        return Database.ExecuteScalar<int>(sql) > 0;
    }

    public bool HasContentForRelease(DateTime date)
    {
        Sql sql = GetSqlForHasScheduling(ContentScheduleAction.Release, date);
        return Database.ExecuteScalar<int>(sql) > 0;
    }

    /// <inheritdoc />
    public IEnumerable<IContent> GetContentForRelease(DateTime date)
    {
        var action = ContentScheduleAction.Release.ToString();

        Sql<ISqlContext> sql = GetBaseQuery(QueryType.Many)
            .WhereIn<NodeDto>(x => x.NodeId, Sql()
                .Select<ContentScheduleDto>(x => x.NodeId)
                .From<ContentScheduleDto>()
                .Where<ContentScheduleDto>(x => x.Action == action && x.Date <= date));

        AddGetByQueryOrderBy(sql);

        return MapDtosToContent(Database.Fetch<DocumentDto>(sql));
    }

    /// <inheritdoc />
    public IDictionary<int, IEnumerable<ContentSchedule>> GetContentSchedulesByIds(int[] contentIds)
    {
        Sql<ISqlContext> sql = Sql()
            .Select<ContentScheduleDto>()
            .From<ContentScheduleDto>()
            .WhereIn<ContentScheduleDto>(contentScheduleDto => contentScheduleDto.NodeId, contentIds);

        List<ContentScheduleDto>? contentScheduleDtos = Database.Fetch<ContentScheduleDto>(sql);

        IDictionary<int, IEnumerable<ContentSchedule>> dictionary = contentScheduleDtos
            .GroupBy(contentSchedule => contentSchedule.NodeId)
            .ToDictionary(
                group => group.Key,
                group => group.Select(scheduleDto => new ContentSchedule(
                    scheduleDto.Id,
                    LanguageRepository.GetIsoCodeById(scheduleDto.LanguageId) ?? Constants.System.InvariantCulture,
                    scheduleDto.Date,
                    scheduleDto.Action == ContentScheduleAction.Release.ToString()
                        ? ContentScheduleAction.Release
                        : ContentScheduleAction.Expire))
                    .ToList().AsEnumerable()); // We have to materialize it here,
                                               // to avoid this being used after the scope is disposed.

        return dictionary;
    }

    /// <inheritdoc />
    public IEnumerable<IContent> GetContentForExpiration(DateTime date)
    {
        var action = ContentScheduleAction.Expire.ToString();

        Sql<ISqlContext> sql = GetBaseQuery(QueryType.Many)
            .WhereIn<NodeDto>(x => x.NodeId, Sql()
                .Select<ContentScheduleDto>(x => x.NodeId)
                .From<ContentScheduleDto>()
                .Where<ContentScheduleDto>(x => x.Action == action && x.Date <= date));

        AddGetByQueryOrderBy(sql);

        return MapDtosToContent(Database.Fetch<DocumentDto>(sql));
    }

    #endregion
}
