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
