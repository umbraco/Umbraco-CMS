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
using Umbraco.Cms.Core.Notifications;
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
            cacheSyncService,
            contentTypeRepository)
    {
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

    protected override void AddAdditionalTempContentMapping(
        List<DocumentDto> dtos,
        List<TempContent<IContent>> temps,
        bool withCache,
        string[]? propertyAliases,
        bool loadTemplates,
        bool loadVariants)
    {
        if (loadTemplates is false)
        {
            return;
        }

        var templateIds = new List<int>();
        foreach (DocumentDto dto in dtos)
        {
            TempContent<IContent>? temp = temps.FirstOrDefault(t => t.Id == dto.NodeId);
            if (temp is null)
            {
                continue;
            }

            temp.Template1Id = dto.ContentVersionDto.TemplateId;
            if (temp.Template1Id.HasValue)
            {
                templateIds.Add(temp.Template1Id.Value);
            }

            if (dto.Published)
            {
                temp.Template2Id = dto.PublishedVersionDto!.TemplateId;
                if (temp.Template2Id.HasValue)
                {
                    templateIds.Add(temp.Template2Id.Value);
                }
            }
        }

        // load all required templates in 1 query, and index
        var templates = _templateRepository
            .GetMany(templateIds.Distinct().ToArray())?
            .ToDictionary(x => x.Id, x => x);

        foreach (TempContent<IContent> temp in temps)
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
    }

    protected override void AddAdditionalContentMapping(DocumentDto dto, IContent content)
    {
        // get template
        if (dto.ContentVersionDto.TemplateId.HasValue)
        {
            content.TemplateId = dto.ContentVersionDto.TemplateId;
        }
    }

    protected override DocumentDto BuildEntityDto(IContent entity)
        => ContentBaseFactory.BuildDto(entity, NodeObjectTypeId);

    protected override IContent BuildEntity(DocumentDto entityDto, IContentType? contentType)
        => ContentBaseFactory.BuildEntity(entityDto, contentType);

    protected override void OnUowRefreshedEntity(IContent entity)
        => OnUowRefreshedEntity(new ContentRefreshNotification(entity, new EventMessages()));

    #region Repository Base

    protected override Guid NodeObjectTypeId => Constants.ObjectTypes.Document;

    protected override IEnumerable<string> GetEntityDeleteClauses()
    {
        var nodeId = QuoteColumnName("nodeId");
        var uniqueId = QuoteColumnName("uniqueId");
        var umbracoNode = QuoteTableName(NodeDto.TableName);
        return
        [
            $@"DELETE FROM {QuoteTableName(Constants.DatabaseSchema.Tables.RedirectUrl)} WHERE {QuoteColumnName("contentKey")} IN
            (SELECT {uniqueId} FROM {umbracoNode} WHERE id = @id)",
            $@"UPDATE {QuoteTableName(Constants.DatabaseSchema.Tables.UserGroup)}
              SET {QuoteColumnName("startContentId")} = NULL
              WHERE {QuoteColumnName("startContentId")} = @id",
            $"DELETE FROM {QuoteTableName(Constants.DatabaseSchema.Tables.Domain)} WHERE {QuoteColumnName("domainRootStructureID")} = @id",
            $"DELETE FROM {QuoteTableName(Constants.DatabaseSchema.Tables.Document)} WHERE {nodeId} = @id",
            $"DELETE FROM {QuoteTableName(Constants.DatabaseSchema.Tables.DocumentCultureVariation)} WHERE {nodeId} = @id",
            $@"DELETE FROM {QuoteTableName(Constants.DatabaseSchema.Tables.DocumentVersion)} WHERE id IN
              (SELECT id FROM {QuoteTableName(Constants.DatabaseSchema.Tables.ContentVersion)} WHERE {nodeId} = @id)",
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
        ];
    }

    #endregion

    #region Content Repository

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

    protected override string RecycleBinCacheKey => CacheKeys.ContentRecycleBinCacheKey;

    #endregion
}
