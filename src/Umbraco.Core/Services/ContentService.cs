using System.Collections.Immutable;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Exceptions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Persistence;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services.Changes;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Implements the content service.
/// </summary>
public class ContentService : RepositoryService, IContentService
{
    private readonly IAuditService _auditService;
    private readonly IContentTypeRepository _contentTypeRepository;
    private readonly IDocumentBlueprintRepository _documentBlueprintRepository;
    private readonly IDocumentRepository _documentRepository;
    private readonly IEntityRepository _entityRepository;
    private readonly ILanguageRepository _languageRepository;
    private readonly ILogger<ContentService> _logger;
    private readonly Lazy<IPropertyValidationService> _propertyValidationService;
    private readonly IShortStringHelper _shortStringHelper;
    private readonly ICultureImpactFactory _cultureImpactFactory;
    private readonly IUserIdKeyResolver _userIdKeyResolver;
    private readonly PropertyEditorCollection _propertyEditorCollection;
    private readonly IIdKeyMap _idKeyMap;
    private ContentSettings _contentSettings;
    private readonly IRelationService _relationService;
    private IQuery<IContent>? _queryNotTrashed;
    private readonly Lazy<IContentCrudService> _crudServiceLazy;

    // Property for convenient access (deferred resolution for both paths)
    private IContentCrudService CrudService => _crudServiceLazy.Value;

    // Query operation service fields (for Phase 2 extracted query operations)
    private readonly IContentQueryOperationService? _queryOperationService;
    private readonly Lazy<IContentQueryOperationService>? _queryOperationServiceLazy;

    // Version operation service fields (for Phase 3 extracted version operations)
    private readonly IContentVersionOperationService? _versionOperationService;
    private readonly Lazy<IContentVersionOperationService>? _versionOperationServiceLazy;

    // Move operation service fields (for Phase 4 extracted move operations)
    private readonly IContentMoveOperationService? _moveOperationService;
    private readonly Lazy<IContentMoveOperationService>? _moveOperationServiceLazy;

    // Publish operation service fields (for Phase 5 extracted publish operations)
    private readonly IContentPublishOperationService? _publishOperationService;
    private readonly Lazy<IContentPublishOperationService>? _publishOperationServiceLazy;

    // Permission manager field (for Phase 6 extracted permission operations)
    private readonly ContentPermissionManager? _permissionManager;
    private readonly Lazy<ContentPermissionManager>? _permissionManagerLazy;

    // Blueprint manager field (for Phase 7 extracted blueprint operations)
    private readonly ContentBlueprintManager? _blueprintManager;
    private readonly Lazy<ContentBlueprintManager>? _blueprintManagerLazy;

    /// <summary>
    /// Gets the query operation service.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the service was not properly initialized.</exception>
    private IContentQueryOperationService QueryOperationService =>
        _queryOperationService ?? _queryOperationServiceLazy?.Value
        ?? throw new InvalidOperationException("QueryOperationService not initialized. Ensure the service is properly injected via constructor.");

    /// <summary>
    /// Gets the version operation service.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the service was not properly initialized.</exception>
    private IContentVersionOperationService VersionOperationService =>
        _versionOperationService ?? _versionOperationServiceLazy?.Value
        ?? throw new InvalidOperationException("VersionOperationService not initialized. Ensure the service is properly injected via constructor.");

    /// <summary>
    /// Gets the move operation service.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the service was not properly initialized.</exception>
    private IContentMoveOperationService MoveOperationService =>
        _moveOperationService ?? _moveOperationServiceLazy?.Value
        ?? throw new InvalidOperationException("MoveOperationService not initialized. Ensure the service is properly injected via constructor.");

    /// <summary>
    /// Gets the publish operation service.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the service was not properly initialized.</exception>
    private IContentPublishOperationService PublishOperationService =>
        _publishOperationService ?? _publishOperationServiceLazy?.Value
        ?? throw new InvalidOperationException("PublishOperationService not initialized. Ensure the service is properly injected via constructor.");

    /// <summary>
    /// Gets the permission manager.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the manager was not properly initialized.</exception>
    private ContentPermissionManager PermissionManager =>
        _permissionManager ?? _permissionManagerLazy?.Value
        ?? throw new InvalidOperationException("PermissionManager not initialized. Ensure the manager is properly injected via constructor.");

    /// <summary>
    /// Gets the blueprint manager.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the manager was not properly initialized.</exception>
    private ContentBlueprintManager BlueprintManager =>
        _blueprintManager ?? _blueprintManagerLazy?.Value
        ?? throw new InvalidOperationException("BlueprintManager not initialized. Ensure the manager is properly injected via constructor.");

    #region Constructors

    [Microsoft.Extensions.DependencyInjection.ActivatorUtilitiesConstructor]
    public ContentService(
        ICoreScopeProvider provider,
        ILoggerFactory loggerFactory,
        IEventMessagesFactory eventMessagesFactory,
        IDocumentRepository documentRepository,
        IEntityRepository entityRepository,
        IAuditService auditService,
        IContentTypeRepository contentTypeRepository,
        IDocumentBlueprintRepository documentBlueprintRepository,
        ILanguageRepository languageRepository,
        Lazy<IPropertyValidationService> propertyValidationService,
        IShortStringHelper shortStringHelper,
        ICultureImpactFactory cultureImpactFactory,
        IUserIdKeyResolver userIdKeyResolver,
        PropertyEditorCollection propertyEditorCollection,
        IIdKeyMap idKeyMap,
        IOptionsMonitor<ContentSettings> optionsMonitor,
        IRelationService relationService,
        IContentCrudService crudService,
        IContentQueryOperationService queryOperationService,  // NEW PARAMETER - Phase 2 query operations
        IContentVersionOperationService versionOperationService,  // NEW PARAMETER - Phase 3 version operations
        IContentMoveOperationService moveOperationService,  // NEW PARAMETER - Phase 4 move operations
        IContentPublishOperationService publishOperationService,  // NEW PARAMETER - Phase 5 publish operations
        ContentPermissionManager permissionManager,  // NEW PARAMETER - Phase 6 permission operations
        ContentBlueprintManager blueprintManager)  // NEW PARAMETER - Phase 7 blueprint operations
        : base(provider, loggerFactory, eventMessagesFactory)
    {
        _documentRepository = documentRepository;
        _entityRepository = entityRepository;
        _auditService = auditService;
        _contentTypeRepository = contentTypeRepository;
        _documentBlueprintRepository = documentBlueprintRepository;
        _languageRepository = languageRepository;
        _propertyValidationService = propertyValidationService;
        _shortStringHelper = shortStringHelper;
        _cultureImpactFactory = cultureImpactFactory;
        _userIdKeyResolver = userIdKeyResolver;
        _propertyEditorCollection = propertyEditorCollection;
        _idKeyMap = idKeyMap;
        _contentSettings = optionsMonitor.CurrentValue;
        optionsMonitor.OnChange((contentSettings) =>
        {
            _contentSettings = contentSettings;
        });
        _relationService = relationService;
        _logger = loggerFactory.CreateLogger<ContentService>();
        ArgumentNullException.ThrowIfNull(crudService);
        // Wrap in Lazy for consistent access pattern (already resolved, so returns immediately)
        _crudServiceLazy = new Lazy<IContentCrudService>(() => crudService);

        // Phase 2: Query operation service (direct injection)
        ArgumentNullException.ThrowIfNull(queryOperationService);
        _queryOperationService = queryOperationService;
        _queryOperationServiceLazy = null;  // Not needed when directly injected

        // Phase 3: Version operation service (direct injection)
        ArgumentNullException.ThrowIfNull(versionOperationService);
        _versionOperationService = versionOperationService;
        _versionOperationServiceLazy = null;  // Not needed when directly injected

        // Phase 4: Move operation service (direct injection)
        ArgumentNullException.ThrowIfNull(moveOperationService);
        _moveOperationService = moveOperationService;
        _moveOperationServiceLazy = null;  // Not needed when directly injected

        // Phase 5: Publish operation service (direct injection)
        ArgumentNullException.ThrowIfNull(publishOperationService);
        _publishOperationService = publishOperationService;
        _publishOperationServiceLazy = null;  // Not needed when directly injected

        // Phase 6: Permission manager (direct injection)
        ArgumentNullException.ThrowIfNull(permissionManager);
        _permissionManager = permissionManager;
        _permissionManagerLazy = null;  // Not needed when directly injected

        // Phase 7: Blueprint manager (direct injection)
        ArgumentNullException.ThrowIfNull(blueprintManager);
        _blueprintManager = blueprintManager;
        _blueprintManagerLazy = null;  // Not needed when directly injected
    }

    [Obsolete("Use the non-obsolete constructor instead. Scheduled removal in v19.")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public ContentService(
        ICoreScopeProvider provider,
        ILoggerFactory loggerFactory,
        IEventMessagesFactory eventMessagesFactory,
        IDocumentRepository documentRepository,
        IEntityRepository entityRepository,
        IAuditRepository auditRepository,  // Old parameter (kept for signature compatibility)
        IContentTypeRepository contentTypeRepository,
        IDocumentBlueprintRepository documentBlueprintRepository,
        ILanguageRepository languageRepository,
        Lazy<IPropertyValidationService> propertyValidationService,
        IShortStringHelper shortStringHelper,
        ICultureImpactFactory cultureImpactFactory,
        IUserIdKeyResolver userIdKeyResolver,
        PropertyEditorCollection propertyEditorCollection,
        IIdKeyMap idKeyMap,
        IOptionsMonitor<ContentSettings> optionsMonitor,
        IRelationService relationService)
        : base(provider, loggerFactory, eventMessagesFactory)
    {
        // All existing field assignments...
        _documentRepository = documentRepository ?? throw new ArgumentNullException(nameof(documentRepository));
        _entityRepository = entityRepository ?? throw new ArgumentNullException(nameof(entityRepository));
        _contentTypeRepository = contentTypeRepository ?? throw new ArgumentNullException(nameof(contentTypeRepository));
        _documentBlueprintRepository = documentBlueprintRepository ?? throw new ArgumentNullException(nameof(documentBlueprintRepository));
        _languageRepository = languageRepository ?? throw new ArgumentNullException(nameof(languageRepository));
        _propertyValidationService = propertyValidationService ?? throw new ArgumentNullException(nameof(propertyValidationService));
        _shortStringHelper = shortStringHelper ?? throw new ArgumentNullException(nameof(shortStringHelper));
        _cultureImpactFactory = cultureImpactFactory ?? throw new ArgumentNullException(nameof(cultureImpactFactory));
        _userIdKeyResolver = userIdKeyResolver ?? throw new ArgumentNullException(nameof(userIdKeyResolver));
        _propertyEditorCollection = propertyEditorCollection ?? throw new ArgumentNullException(nameof(propertyEditorCollection));
        _idKeyMap = idKeyMap ?? throw new ArgumentNullException(nameof(idKeyMap));
        _contentSettings = optionsMonitor?.CurrentValue ?? throw new ArgumentNullException(nameof(optionsMonitor));
        optionsMonitor.OnChange((contentSettings) =>
        {
            _contentSettings = contentSettings;
        });
        _relationService = relationService ?? throw new ArgumentNullException(nameof(relationService));
        _logger = loggerFactory.CreateLogger<ContentService>();

        // Lazy resolution of IAuditService (from StaticServiceProvider)
        _auditService = StaticServiceProvider.Instance.GetRequiredService<IAuditService>();

        // NEW: Lazy resolution of IContentCrudService
        _crudServiceLazy = new Lazy<IContentCrudService>(() =>
            StaticServiceProvider.Instance.GetRequiredService<IContentCrudService>(),
            LazyThreadSafetyMode.ExecutionAndPublication);

        // Phase 2: Lazy resolution of IContentQueryOperationService
        _queryOperationServiceLazy = new Lazy<IContentQueryOperationService>(() =>
            StaticServiceProvider.Instance.GetRequiredService<IContentQueryOperationService>(),
            LazyThreadSafetyMode.ExecutionAndPublication);

        // Phase 3: Lazy resolution of IContentVersionOperationService
        _versionOperationServiceLazy = new Lazy<IContentVersionOperationService>(() =>
            StaticServiceProvider.Instance.GetRequiredService<IContentVersionOperationService>(),
            LazyThreadSafetyMode.ExecutionAndPublication);

        // Phase 4: Lazy resolution of IContentMoveOperationService
        _moveOperationServiceLazy = new Lazy<IContentMoveOperationService>(() =>
            StaticServiceProvider.Instance.GetRequiredService<IContentMoveOperationService>(),
            LazyThreadSafetyMode.ExecutionAndPublication);

        // Phase 5: Lazy resolution of IContentPublishOperationService
        _publishOperationServiceLazy = new Lazy<IContentPublishOperationService>(() =>
            StaticServiceProvider.Instance.GetRequiredService<IContentPublishOperationService>(),
            LazyThreadSafetyMode.ExecutionAndPublication);

        // Phase 6: Lazy resolution of ContentPermissionManager
        _permissionManagerLazy = new Lazy<ContentPermissionManager>(() =>
            StaticServiceProvider.Instance.GetRequiredService<ContentPermissionManager>(),
            LazyThreadSafetyMode.ExecutionAndPublication);

        // Phase 7: Lazy resolution of ContentBlueprintManager
        _blueprintManagerLazy = new Lazy<ContentBlueprintManager>(() =>
            StaticServiceProvider.Instance.GetRequiredService<ContentBlueprintManager>(),
            LazyThreadSafetyMode.ExecutionAndPublication);
    }

    [Obsolete("Use the non-obsolete constructor instead. Scheduled removal in v19.")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public ContentService(
        ICoreScopeProvider provider,
        ILoggerFactory loggerFactory,
        IEventMessagesFactory eventMessagesFactory,
        IDocumentRepository documentRepository,
        IEntityRepository entityRepository,
        IAuditRepository auditRepository,  // Old parameter (kept for signature compatibility)
        IAuditService auditService,
        IContentTypeRepository contentTypeRepository,
        IDocumentBlueprintRepository documentBlueprintRepository,
        ILanguageRepository languageRepository,
        Lazy<IPropertyValidationService> propertyValidationService,
        IShortStringHelper shortStringHelper,
        ICultureImpactFactory cultureImpactFactory,
        IUserIdKeyResolver userIdKeyResolver,
        PropertyEditorCollection propertyEditorCollection,
        IIdKeyMap idKeyMap,
        IOptionsMonitor<ContentSettings> optionsMonitor,
        IRelationService relationService)
        : base(provider, loggerFactory, eventMessagesFactory)
    {
        // All existing field assignments...
        _documentRepository = documentRepository ?? throw new ArgumentNullException(nameof(documentRepository));
        _entityRepository = entityRepository ?? throw new ArgumentNullException(nameof(entityRepository));
        _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        _contentTypeRepository = contentTypeRepository ?? throw new ArgumentNullException(nameof(contentTypeRepository));
        _documentBlueprintRepository = documentBlueprintRepository ?? throw new ArgumentNullException(nameof(documentBlueprintRepository));
        _languageRepository = languageRepository ?? throw new ArgumentNullException(nameof(languageRepository));
        _propertyValidationService = propertyValidationService ?? throw new ArgumentNullException(nameof(propertyValidationService));
        _shortStringHelper = shortStringHelper ?? throw new ArgumentNullException(nameof(shortStringHelper));
        _cultureImpactFactory = cultureImpactFactory ?? throw new ArgumentNullException(nameof(cultureImpactFactory));
        _userIdKeyResolver = userIdKeyResolver ?? throw new ArgumentNullException(nameof(userIdKeyResolver));
        _propertyEditorCollection = propertyEditorCollection ?? throw new ArgumentNullException(nameof(propertyEditorCollection));
        _idKeyMap = idKeyMap ?? throw new ArgumentNullException(nameof(idKeyMap));
        _contentSettings = optionsMonitor?.CurrentValue ?? throw new ArgumentNullException(nameof(optionsMonitor));
        optionsMonitor.OnChange((contentSettings) =>
        {
            _contentSettings = contentSettings;
        });
        _relationService = relationService ?? throw new ArgumentNullException(nameof(relationService));
        _logger = loggerFactory.CreateLogger<ContentService>();

        // NEW: Lazy resolution of IContentCrudService
        _crudServiceLazy = new Lazy<IContentCrudService>(() =>
            StaticServiceProvider.Instance.GetRequiredService<IContentCrudService>(),
            LazyThreadSafetyMode.ExecutionAndPublication);

        // Phase 2: Lazy resolution of IContentQueryOperationService
        _queryOperationServiceLazy = new Lazy<IContentQueryOperationService>(() =>
            StaticServiceProvider.Instance.GetRequiredService<IContentQueryOperationService>(),
            LazyThreadSafetyMode.ExecutionAndPublication);

        // Phase 3: Lazy resolution of IContentVersionOperationService
        _versionOperationServiceLazy = new Lazy<IContentVersionOperationService>(() =>
            StaticServiceProvider.Instance.GetRequiredService<IContentVersionOperationService>(),
            LazyThreadSafetyMode.ExecutionAndPublication);

        // Phase 4: Lazy resolution of IContentMoveOperationService
        _moveOperationServiceLazy = new Lazy<IContentMoveOperationService>(() =>
            StaticServiceProvider.Instance.GetRequiredService<IContentMoveOperationService>(),
            LazyThreadSafetyMode.ExecutionAndPublication);

        // Phase 5: Lazy resolution of IContentPublishOperationService
        _publishOperationServiceLazy = new Lazy<IContentPublishOperationService>(() =>
            StaticServiceProvider.Instance.GetRequiredService<IContentPublishOperationService>(),
            LazyThreadSafetyMode.ExecutionAndPublication);

        // Phase 6: Lazy resolution of ContentPermissionManager
        _permissionManagerLazy = new Lazy<ContentPermissionManager>(() =>
            StaticServiceProvider.Instance.GetRequiredService<ContentPermissionManager>(),
            LazyThreadSafetyMode.ExecutionAndPublication);

        // Phase 7: Lazy resolution of ContentBlueprintManager
        _blueprintManagerLazy = new Lazy<ContentBlueprintManager>(() =>
            StaticServiceProvider.Instance.GetRequiredService<ContentBlueprintManager>(),
            LazyThreadSafetyMode.ExecutionAndPublication);
    }

    #endregion

    #region Static queries

    // lazy-constructed because when the ctor runs, the query factory may not be ready
    private IQuery<IContent> QueryNotTrashed =>
        _queryNotTrashed ??= Query<IContent>().Where(x => x.Trashed == false);

    #endregion

    #region Rollback

    public OperationResult Rollback(int id, int versionId, string culture = "*", int userId = Constants.Security.SuperUserId)
        => VersionOperationService.Rollback(id, versionId, culture, userId);

    #endregion

    #region Count

    public int CountPublished(string? contentTypeAlias = null)
        => QueryOperationService.CountPublished(contentTypeAlias);

    public int Count(string? contentTypeAlias = null)
        => QueryOperationService.Count(contentTypeAlias);

    public int CountChildren(int parentId, string? contentTypeAlias = null)
        => QueryOperationService.CountChildren(parentId, contentTypeAlias);

    public int CountDescendants(int parentId, string? contentTypeAlias = null)
        => QueryOperationService.CountDescendants(parentId, contentTypeAlias);

    #endregion

    #region Permissions

    /// <summary>
    ///     Used to bulk update the permissions set for a content item. This will replace all permissions
    ///     assigned to an entity with a list of user id &amp; permission pairs.
    /// </summary>
    /// <param name="permissionSet"></param>
    public void SetPermissions(EntityPermissionSet permissionSet)
        => PermissionManager.SetPermissions(permissionSet);

    /// <summary>
    ///     Assigns a single permission to the current content item for the specified group ids
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="permission"></param>
    /// <param name="groupIds"></param>
    public void SetPermission(IContent entity, string permission, IEnumerable<int> groupIds)
        => PermissionManager.SetPermission(entity, permission, groupIds);

    /// <summary>
    ///     Returns implicit/inherited permissions assigned to the content item for all user groups
    /// </summary>
    /// <param name="content"></param>
    /// <returns></returns>
    public EntityPermissionCollection GetPermissions(IContent content)
        => PermissionManager.GetPermissions(content);

    #endregion

    #region Create

    /// <summary>
    ///     Creates an <see cref="IContent" /> object using the alias of the <see cref="IContentType" />
    ///     that this Content should based on.
    /// </summary>
    /// <remarks>
    ///     Note that using this method will simply return a new IContent without any identity
    ///     as it has not yet been persisted. It is intended as a shortcut to creating new content objects
    ///     that does not invoke a save operation against the database.
    /// </remarks>
    /// <param name="name">Name of the Content object</param>
    /// <param name="parentId">Id of Parent for the new Content</param>
    /// <param name="contentTypeAlias">Alias of the <see cref="IContentType" /></param>
    /// <param name="userId">Optional id of the user creating the content</param>
    /// <returns>
    ///     <see cref="IContent" />
    /// </returns>
    public IContent Create(string name, Guid parentId, string contentTypeAlias, int userId = Constants.Security.SuperUserId)
        => CrudService.Create(name, parentId, contentTypeAlias, userId);

    /// <summary>
    ///     Creates an <see cref="IContent" /> object of a specified content type.
    /// </summary>
    /// <remarks>
    ///     This method simply returns a new, non-persisted, IContent without any identity. It
    ///     is intended as a shortcut to creating new content objects that does not invoke a save
    ///     operation against the database.
    /// </remarks>
    /// <param name="name">The name of the content object.</param>
    /// <param name="parentId">The identifier of the parent, or -1.</param>
    /// <param name="contentTypeAlias">The alias of the content type.</param>
    /// <param name="userId">The optional id of the user creating the content.</param>
    /// <returns>The content object.</returns>
    public IContent Create(string name, int parentId, string contentTypeAlias, int userId = Constants.Security.SuperUserId)
        => CrudService.Create(name, parentId, contentTypeAlias, userId);

    /// <summary>
    ///     Creates an <see cref="IContent" /> object of a specified content type.
    /// </summary>
    /// <remarks>
    ///     This method simply returns a new, non-persisted, IContent without any identity. It
    ///     is intended as a shortcut to creating new content objects that does not invoke a save
    ///     operation against the database.
    /// </remarks>
    /// <param name="name">The name of the content object.</param>
    /// <param name="parentId">The identifier of the parent, or -1.</param>
    /// <param name="contentType">The content type of the content</param>
    /// <param name="userId">The optional id of the user creating the content.</param>
    /// <returns>The content object.</returns>
    public IContent Create(string name, int parentId, IContentType contentType, int userId = Constants.Security.SuperUserId)
        => CrudService.Create(name, parentId, contentType, userId);

    /// <summary>
    ///     Creates an <see cref="IContent" /> object of a specified content type, under a parent.
    /// </summary>
    /// <remarks>
    ///     This method simply returns a new, non-persisted, IContent without any identity. It
    ///     is intended as a shortcut to creating new content objects that does not invoke a save
    ///     operation against the database.
    /// </remarks>
    /// <param name="name">The name of the content object.</param>
    /// <param name="parent">The parent content object.</param>
    /// <param name="contentTypeAlias">The alias of the content type.</param>
    /// <param name="userId">The optional id of the user creating the content.</param>
    /// <returns>The content object.</returns>
    public IContent Create(string name, IContent? parent, string contentTypeAlias, int userId = Constants.Security.SuperUserId)
        => CrudService.Create(name, parent, contentTypeAlias, userId);

    /// <summary>
    ///     Creates an <see cref="IContent" /> object of a specified content type.
    /// </summary>
    /// <remarks>This method returns a new, persisted, IContent with an identity.</remarks>
    /// <param name="name">The name of the content object.</param>
    /// <param name="parentId">The identifier of the parent, or -1.</param>
    /// <param name="contentTypeAlias">The alias of the content type.</param>
    /// <param name="userId">The optional id of the user creating the content.</param>
    /// <returns>The content object.</returns>
    public IContent CreateAndSave(string name, int parentId, string contentTypeAlias, int userId = Constants.Security.SuperUserId)
        => CrudService.CreateAndSave(name, parentId, contentTypeAlias, userId);

    /// <summary>
    ///     Creates an <see cref="IContent" /> object of a specified content type, under a parent.
    /// </summary>
    /// <remarks>This method returns a new, persisted, IContent with an identity.</remarks>
    /// <param name="name">The name of the content object.</param>
    /// <param name="parent">The parent content object.</param>
    /// <param name="contentTypeAlias">The alias of the content type.</param>
    /// <param name="userId">The optional id of the user creating the content.</param>
    /// <returns>The content object.</returns>
    public IContent CreateAndSave(string name, IContent parent, string contentTypeAlias, int userId = Constants.Security.SuperUserId)
        => CrudService.CreateAndSave(name, parent, contentTypeAlias, userId);

    #endregion

    #region Get, Has, Is

    /// <summary>
    ///     Gets an <see cref="IContent" /> object by Id
    /// </summary>
    /// <param name="id">Id of the Content to retrieve</param>
    /// <returns>
    ///     <see cref="IContent" />
    /// </returns>
    public IContent? GetById(int id)
        => CrudService.GetById(id);

    /// <summary>
    ///     Gets an <see cref="IContent" /> object by Id
    /// </summary>
    /// <param name="ids">Ids of the Content to retrieve</param>
    /// <returns>
    ///     <see cref="IContent" />
    /// </returns>
    public IEnumerable<IContent> GetByIds(IEnumerable<int> ids)
        => CrudService.GetByIds(ids);

    /// <summary>
    ///     Gets an <see cref="IContent" /> object by its 'UniqueId'
    /// </summary>
    /// <param name="key">Guid key of the Content to retrieve</param>
    /// <returns>
    ///     <see cref="IContent" />
    /// </returns>
    public IContent? GetById(Guid key)
        => CrudService.GetById(key);

    /// <inheritdoc />
    public ContentScheduleCollection GetContentScheduleByContentId(int contentId)
        => PublishOperationService.GetContentScheduleByContentId(contentId);

    public ContentScheduleCollection GetContentScheduleByContentId(Guid contentId)
        => PublishOperationService.GetContentScheduleByContentId(contentId);

    /// <inheritdoc />
    public void PersistContentSchedule(IContent content, ContentScheduleCollection contentSchedule)
        => PublishOperationService.PersistContentSchedule(content, contentSchedule);

    /// <summary>
    /// </summary>
    /// <param name="contents"></param>
    /// <param name="userId"></param>
    /// <returns></returns>
    Attempt<OperationResult?> IContentServiceBase<IContent>.Save(IEnumerable<IContent> contents, int userId) =>
        Attempt.Succeed(Save(contents, userId));

    /// <summary>
    ///     Gets <see cref="IContent" /> objects by Ids
    /// </summary>
    /// <param name="ids">Ids of the Content to retrieve</param>
    /// <returns>
    ///     <see cref="IContent" />
    /// </returns>
    public IEnumerable<IContent> GetByIds(IEnumerable<Guid> ids)
        => CrudService.GetByIds(ids);

    /// <inheritdoc />
    public IEnumerable<IContent> GetPagedOfType(
        int contentTypeId,
        long pageIndex,
        int pageSize,
        out long totalRecords,
        IQuery<IContent>? filter = null,
        Ordering? ordering = null)
        => QueryOperationService.GetPagedOfType(contentTypeId, pageIndex, pageSize, out totalRecords, filter, ordering);

    /// <inheritdoc />
    public IEnumerable<IContent> GetPagedOfTypes(int[] contentTypeIds, long pageIndex, int pageSize, out long totalRecords, IQuery<IContent>? filter, Ordering? ordering = null)
        => QueryOperationService.GetPagedOfTypes(contentTypeIds, pageIndex, pageSize, out totalRecords, filter, ordering);

    /// <summary>
    ///     Gets a collection of <see cref="IContent" /> objects by Level
    /// </summary>
    /// <param name="level">The level to retrieve Content from</param>
    /// <returns>An Enumerable list of <see cref="IContent" /> objects</returns>
    /// <remarks>Contrary to most methods, this method filters out trashed content items.</remarks>
    public IEnumerable<IContent> GetByLevel(int level)
        => QueryOperationService.GetByLevel(level);

    /// <summary>
    ///     Gets a specific version of an <see cref="IContent" /> item.
    /// </summary>
    /// <param name="versionId">Id of the version to retrieve</param>
    /// <returns>An <see cref="IContent" /> item</returns>
    public IContent? GetVersion(int versionId)
        => VersionOperationService.GetVersion(versionId);

    /// <summary>
    ///     Gets a collection of an <see cref="IContent" /> objects versions by Id
    /// </summary>
    /// <param name="id"></param>
    /// <returns>An Enumerable list of <see cref="IContent" /> objects</returns>
    public IEnumerable<IContent> GetVersions(int id)
        => VersionOperationService.GetVersions(id);

    /// <summary>
    ///     Gets a collection of an <see cref="IContent" /> objects versions by Id
    /// </summary>
    /// <returns>An Enumerable list of <see cref="IContent" /> objects</returns>
    public IEnumerable<IContent> GetVersionsSlim(int id, int skip, int take)
        => VersionOperationService.GetVersionsSlim(id, skip, take);

    /// <summary>
    ///     Gets a list of all version Ids for the given content item ordered so latest is first
    /// </summary>
    /// <param name="id"></param>
    /// <param name="maxRows">The maximum number of rows to return</param>
    /// <returns></returns>
    public IEnumerable<int> GetVersionIds(int id, int maxRows)
        => VersionOperationService.GetVersionIds(id, maxRows);

    /// <summary>
    ///     Gets a collection of <see cref="IContent" /> objects, which are ancestors of the current content.
    /// </summary>
    /// <param name="id">Id of the <see cref="IContent" /> to retrieve ancestors for</param>
    /// <returns>An Enumerable list of <see cref="IContent" /> objects</returns>
    public IEnumerable<IContent> GetAncestors(int id)
        => CrudService.GetAncestors(id);

    /// <summary>
    ///     Gets a collection of <see cref="IContent" /> objects, which are ancestors of the current content.
    /// </summary>
    /// <param name="content"><see cref="IContent" /> to retrieve ancestors for</param>
    /// <returns>An Enumerable list of <see cref="IContent" /> objects</returns>
    public IEnumerable<IContent> GetAncestors(IContent content)
        => CrudService.GetAncestors(content);

    public IEnumerable<IContent> GetPublishedChildren(int id)
        => PublishOperationService.GetPublishedChildren(id);

    /// <inheritdoc />
    public IEnumerable<IContent> GetPagedChildren(int id, long pageIndex, int pageSize, out long totalChildren, IQuery<IContent>? filter = null, Ordering? ordering = null)
        => CrudService.GetPagedChildren(id, pageIndex, pageSize, out totalChildren, filter, ordering);

    /// <inheritdoc />
    public IEnumerable<IContent> GetPagedDescendants(int id, long pageIndex, int pageSize, out long totalChildren, IQuery<IContent>? filter = null, Ordering? ordering = null)
        => CrudService.GetPagedDescendants(id, pageIndex, pageSize, out totalChildren, filter, ordering);

    private IQuery<IContent>? GetPagedDescendantQuery(string contentPath)
    {
        IQuery<IContent>? query = Query<IContent>();
        if (!contentPath.IsNullOrWhiteSpace())
        {
            query?.Where(x => x.Path.SqlStartsWith($"{contentPath},", TextColumnType.NVarchar));
        }

        return query;
    }

    private IEnumerable<IContent> GetPagedLocked(IQuery<IContent>? query, long pageIndex, int pageSize, out long totalChildren, IQuery<IContent>? filter, Ordering? ordering)
    {
        if (pageIndex < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(pageIndex));
        }

        if (pageSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(pageSize));
        }

        if (ordering == null)
        {
            throw new ArgumentNullException(nameof(ordering));
        }

        return _documentRepository.GetPage(query, pageIndex, pageSize, out totalChildren, filter, ordering);
    }

    /// <summary>
    ///     Gets the parent of the current content as an <see cref="IContent" /> item.
    /// </summary>
    /// <param name="id">Id of the <see cref="IContent" /> to retrieve the parent from</param>
    /// <returns>Parent <see cref="IContent" /> object</returns>
    public IContent? GetParent(int id)
        => CrudService.GetParent(id);

    /// <summary>
    ///     Gets the parent of the current content as an <see cref="IContent" /> item.
    /// </summary>
    /// <param name="content"><see cref="IContent" /> to retrieve the parent from</param>
    /// <returns>Parent <see cref="IContent" /> object</returns>
    public IContent? GetParent(IContent? content)
        => CrudService.GetParent(content);

    /// <summary>
    ///     Gets a collection of <see cref="IContent" /> objects, which reside at the first level / root
    /// </summary>
    /// <returns>An Enumerable list of <see cref="IContent" /> objects</returns>
    public IEnumerable<IContent> GetRootContent()
        => CrudService.GetRootContent();

    /// <summary>
    ///     Gets all published content items
    /// </summary>
    /// <returns></returns>
    internal IEnumerable<IContent> GetAllPublished()
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            scope.ReadLock(Constants.Locks.ContentTree);
            return _documentRepository.Get(QueryNotTrashed);
        }
    }

    /// <inheritdoc />
    public IEnumerable<IContent> GetContentForExpiration(DateTime date)
        => PublishOperationService.GetContentForExpiration(date);

    /// <inheritdoc />
    public IEnumerable<IContent> GetContentForRelease(DateTime date)
        => PublishOperationService.GetContentForRelease(date);

    /// <summary>
    ///     Gets a collection of an <see cref="IContent" /> objects, which resides in the Recycle Bin
    /// </summary>
    /// <returns>An Enumerable list of <see cref="IContent" /> objects</returns>
    public IEnumerable<IContent> GetPagedContentInRecycleBin(long pageIndex, int pageSize, out long totalRecords, IQuery<IContent>? filter = null, Ordering? ordering = null)
        => MoveOperationService.GetPagedContentInRecycleBin(pageIndex, pageSize, out totalRecords, filter, ordering);

    /// <summary>
    ///     Checks whether an <see cref="IContent" /> item has any children
    /// </summary>
    /// <param name="id">Id of the <see cref="IContent" /></param>
    /// <returns>True if the content has any children otherwise False</returns>
    public bool HasChildren(int id)
        => CrudService.HasChildren(id);

    /// <summary>
    ///     Checks whether a document with the specified id exists.
    /// </summary>
    /// <param name="id">The document id.</param>
    /// <returns>True if the document exists; otherwise false.</returns>
    public bool Exists(int id)
        => CrudService.Exists(id);

    /// <summary>
    ///     Checks whether a document with the specified key exists.
    /// </summary>
    /// <param name="key">The document key.</param>
    /// <returns>True if the document exists; otherwise false.</returns>
    public bool Exists(Guid key)
        => CrudService.Exists(key);

    /// <inheritdoc/>
    public IDictionary<int, IEnumerable<ContentSchedule>> GetContentSchedulesByIds(Guid[] keys)
        => PublishOperationService.GetContentSchedulesByIds(keys);

    /// <inheritdoc />
    public bool IsPathPublishable(IContent content)
        => PublishOperationService.IsPathPublishable(content);

    public bool IsPathPublished(IContent? content)
        => PublishOperationService.IsPathPublished(content);

    #endregion

    #region Save, Publish, Unpublish

    /// <inheritdoc />
    public OperationResult Save(IContent content, int? userId = null, ContentScheduleCollection? contentSchedule = null)
        => CrudService.Save(content, userId, contentSchedule);

    /// <inheritdoc />
    public OperationResult Save(IEnumerable<IContent> contents, int userId = Constants.Security.SuperUserId)
        => CrudService.Save(contents, userId);

    /// <inheritdoc/>
    public PublishResult Publish(IContent content, string[] cultures, int userId = Constants.Security.SuperUserId)
        => PublishOperationService.Publish(content, cultures, userId);

    /// <inheritdoc />
    public PublishResult Unpublish(IContent content, string? culture = "*", int userId = Constants.Security.SuperUserId)
        => PublishOperationService.Unpublish(content, culture, userId);


    /// <inheritdoc />
    public IEnumerable<PublishResult> PerformScheduledPublish(DateTime date)
        => PublishOperationService.PerformScheduledPublish(date);

    /// <inheritdoc />
    public IEnumerable<PublishResult> PublishBranch(IContent content, PublishBranchFilter publishBranchFilter, string[] cultures, int userId = Constants.Security.SuperUserId)
        => PublishOperationService.PublishBranch(content, publishBranchFilter, cultures, userId);

    #endregion

    #region Delete

    /// <inheritdoc />
    public OperationResult Delete(IContent content, int userId = Constants.Security.SuperUserId)
        => CrudService.Delete(content, userId);

    private void DeleteLocked(ICoreScope scope, IContent content, EventMessages evtMsgs)
    {
        void DoDelete(IContent c)
        {
            _documentRepository.Delete(c);
            scope.Notifications.Publish(new ContentDeletedNotification(c, evtMsgs));

            // media files deleted by QueuingEventDispatcher
        }

        const int pageSize = 500;
        var total = long.MaxValue;
        while (total > 0)
        {
            // get descendants - ordered from deepest to shallowest
            IEnumerable<IContent> descendants = GetPagedDescendants(content.Id, 0, pageSize, out total, ordering: Ordering.By("Path", Direction.Descending));
            foreach (IContent c in descendants)
            {
                DoDelete(c);
            }
        }

        DoDelete(content);
    }

    // TODO: both DeleteVersions methods below have an issue. Sort of. They do NOT take care of files the way
    // Delete does - for a good reason: the file may be referenced by other, non-deleted, versions. BUT,
    // if that's not the case, then the file will never be deleted, because when we delete the content,
    // the version referencing the file will not be there anymore. SO, we can leak files.

    /// <summary>
    ///     Permanently deletes versions from an <see cref="IContent" /> object prior to a specific date.
    ///     This method will never delete the latest version of a content item.
    /// </summary>
    /// <param name="id">Id of the <see cref="IContent" /> object to delete versions from</param>
    /// <param name="versionDate">Latest version date</param>
    /// <param name="userId">Optional Id of the User deleting versions of a Content object</param>
    public void DeleteVersions(int id, DateTime versionDate, int userId = Constants.Security.SuperUserId)
        => VersionOperationService.DeleteVersions(id, versionDate, userId);

    /// <summary>
    ///     Permanently deletes specific version(s) from an <see cref="IContent" /> object.
    ///     This method will never delete the latest version of a content item.
    /// </summary>
    /// <param name="id">Id of the <see cref="IContent" /> object to delete a version from</param>
    /// <param name="versionId">Id of the version to delete</param>
    /// <param name="deletePriorVersions">Boolean indicating whether to delete versions prior to the versionId</param>
    /// <param name="userId">Optional Id of the User deleting versions of a Content object</param>
    public void DeleteVersion(int id, int versionId, bool deletePriorVersions, int userId = Constants.Security.SuperUserId)
        => VersionOperationService.DeleteVersion(id, versionId, deletePriorVersions, userId);

    #endregion

    #region Move, RecycleBin

    /// <inheritdoc />
    public OperationResult MoveToRecycleBin(IContent content, int userId = Constants.Security.SuperUserId)
    {
        EventMessages eventMessages = EventMessagesFactory.Get();
        var moves = new List<(IContent, string)>();

        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            scope.WriteLock(Constants.Locks.ContentTree);

            var originalPath = content.Path;
            var moveEventInfo =
                new MoveToRecycleBinEventInfo<IContent>(content, originalPath);

            var movingToRecycleBinNotification =
                new ContentMovingToRecycleBinNotification(moveEventInfo, eventMessages);
            if (scope.Notifications.PublishCancelable(movingToRecycleBinNotification))
            {
                scope.Complete();
                return OperationResult.Cancel(eventMessages); // causes rollback
            }

            // if it's published we may want to force-unpublish it - that would be backward-compatible... but...
            // making a radical decision here: trashing is equivalent to moving under an unpublished node so
            // it's NOT unpublishing, only the content is now masked - allowing us to restore it if wanted
            // if (content.HasPublishedVersion)
            // { }
            PerformMoveLocked(content, Constants.System.RecycleBinContent, null, userId, moves, true);
            scope.Notifications.Publish(
                new ContentTreeChangeNotification(content, TreeChangeTypes.RefreshBranch, eventMessages));

            MoveToRecycleBinEventInfo<IContent>[] moveInfo = moves
                .Select(x => new MoveToRecycleBinEventInfo<IContent>(x.Item1, x.Item2))
                .ToArray();

            scope.Notifications.Publish(
                new ContentMovedToRecycleBinNotification(moveInfo, eventMessages).WithStateFrom(
                    movingToRecycleBinNotification));
            Audit(AuditType.Move, userId, content.Id, "Moved to recycle bin");

            scope.Complete();
        }

        return OperationResult.Succeed(eventMessages);
    }

    /// <summary>
    ///     Moves an <see cref="IContent" /> object to a new location by changing its parent id.
    /// </summary>
    /// <remarks>
    ///     If the <see cref="IContent" /> object is already published it will be
    ///     published after being moved to its new location. Otherwise it'll just
    ///     be saved with a new parent id.
    /// </remarks>
    /// <param name="content">The <see cref="IContent" /> to move</param>
    /// <param name="parentId">Id of the Content's new Parent</param>
    /// <param name="userId">Optional Id of the User moving the Content</param>
    public OperationResult Move(IContent content, int parentId, int userId = Constants.Security.SuperUserId)
    {
        // If moving to recycle bin, use MoveToRecycleBin which handles unpublish
        if (parentId == Constants.System.RecycleBinContent)
        {
            return MoveToRecycleBin(content, userId);
        }

        return MoveOperationService.Move(content, parentId, userId);
    }

    // MUST be called from within WriteLock
    // trash indicates whether we are trashing, un-trashing, or not changing anything
    private void PerformMoveLocked(IContent content, int parentId, IContent? parent, int userId, ICollection<(IContent, string)> moves, bool? trash)
    {
        content.WriterId = userId;
        content.ParentId = parentId;

        // get the level delta (old pos to new pos)
        // note that recycle bin (id:-20) level is 0!
        var levelDelta = 1 - content.Level + (parent?.Level ?? 0);

        var paths = new Dictionary<int, string>();

        moves.Add((content, content.Path)); // capture original path

        // need to store the original path to lookup descendants based on it below
        var originalPath = content.Path;

        // these will be updated by the repo because we changed parentId
        // content.Path = (parent == null ? "-1" : parent.Path) + "," + content.Id;
        // content.SortOrder = ((ContentRepository) repository).NextChildSortOrder(parentId);
        // content.Level += levelDelta;
        PerformMoveContentLocked(content, userId, trash);

        // if uow is not immediate, content.Path will be updated only when the UOW commits,
        // and because we want it now, we have to calculate it by ourselves
        // paths[content.Id] = content.Path;
        paths[content.Id] =
            (parent == null
                ? parentId == Constants.System.RecycleBinContent ? "-1,-20" : Constants.System.RootString
                : parent.Path) + "," + content.Id;

        const int pageSize = 500;
        IQuery<IContent>? query = GetPagedDescendantQuery(originalPath);
        long total;
        do
        {
            // We always page a page 0 because for each page, we are moving the result so the resulting total will be reduced
            IEnumerable<IContent> descendants =
                GetPagedLocked(query, 0, pageSize, out total, null, Ordering.By("Path"));

            foreach (IContent descendant in descendants)
            {
                moves.Add((descendant, descendant.Path)); // capture original path

                // update path and level since we do not update parentId
                descendant.Path = paths[descendant.Id] = paths[descendant.ParentId] + "," + descendant.Id;
                descendant.Level += levelDelta;
                PerformMoveContentLocked(descendant, userId, trash);
            }
        }
        while (total > pageSize);
    }

    private void PerformMoveContentLocked(IContent content, int userId, bool? trash)
    {
        if (trash.HasValue)
        {
            ((ContentBase)content).Trashed = trash.Value;
        }

        content.WriterId = userId;
        _documentRepository.Save(content);
    }

    public async Task<OperationResult> EmptyRecycleBinAsync(Guid userId)
        => await MoveOperationService.EmptyRecycleBinAsync(userId);

    /// <summary>
    ///     Empties the Recycle Bin by deleting all <see cref="IContent" /> that resides in the bin
    /// </summary>
    public OperationResult EmptyRecycleBin(int userId = Constants.Security.SuperUserId)
        => MoveOperationService.EmptyRecycleBin(userId);

    public bool RecycleBinSmells()
        => MoveOperationService.RecycleBinSmells();

    #endregion

    #region Others

    /// <summary>
    ///     Copies an <see cref="IContent" /> object by creating a new Content object of the same type and copies all data from
    ///     the current
    ///     to the new copy which is returned. Recursively copies all children.
    /// </summary>
    /// <param name="content">The <see cref="IContent" /> to copy</param>
    /// <param name="parentId">Id of the Content's new Parent</param>
    /// <param name="relateToOriginal">Boolean indicating whether the copy should be related to the original</param>
    /// <param name="userId">Optional Id of the User copying the Content</param>
    /// <returns>The newly created <see cref="IContent" /> object</returns>
    public IContent? Copy(IContent content, int parentId, bool relateToOriginal, int userId = Constants.Security.SuperUserId)
        => MoveOperationService.Copy(content, parentId, relateToOriginal, userId);

    /// <summary>
    ///     Copies an <see cref="IContent" /> object by creating a new Content object of the same type and copies all data from
    ///     the current
    ///     to the new copy which is returned.
    /// </summary>
    /// <param name="content">The <see cref="IContent" /> to copy</param>
    /// <param name="parentId">Id of the Content's new Parent</param>
    /// <param name="relateToOriginal">Boolean indicating whether the copy should be related to the original</param>
    /// <param name="recursive">A value indicating whether to recursively copy children.</param>
    /// <param name="userId">Optional Id of the User copying the Content</param>
    /// <returns>The newly created <see cref="IContent" /> object</returns>
    public IContent? Copy(IContent content, int parentId, bool relateToOriginal, bool recursive, int userId = Constants.Security.SuperUserId)
        => MoveOperationService.Copy(content, parentId, relateToOriginal, recursive, userId);

    private bool TryGetParentKey(int parentId, [NotNullWhen(true)] out Guid? parentKey)
    {
        Attempt<Guid> parentKeyAttempt = _idKeyMap.GetKeyForId(parentId, UmbracoObjectTypes.Document);
        parentKey = parentKeyAttempt.Success ? parentKeyAttempt.Result : null;
        return parentKeyAttempt.Success;
    }

    /// <inheritdoc />
    public bool SendToPublication(IContent? content, int userId = Constants.Security.SuperUserId)
        => PublishOperationService.SendToPublication(content, userId);

    /// <summary>
    ///     Sorts a collection of <see cref="IContent" /> objects by updating the SortOrder according
    ///     to the ordering of items in the passed in <paramref name="items" />.
    /// </summary>
    /// <remarks>
    ///     Using this method will ensure that the Published-state is maintained upon sorting
    ///     so the cache is updated accordingly - as needed.
    /// </remarks>
    /// <param name="items"></param>
    /// <param name="userId"></param>
    /// <returns>Result indicating what action was taken when handling the command.</returns>
    public OperationResult Sort(IEnumerable<IContent> items, int userId = Constants.Security.SuperUserId)
        => MoveOperationService.Sort(items, userId);

    /// <summary>
    ///     Sorts a collection of <see cref="IContent" /> objects by updating the SortOrder according
    ///     to the ordering of items identified by the <paramref name="ids" />.
    /// </summary>
    /// <remarks>
    ///     Using this method will ensure that the Published-state is maintained upon sorting
    ///     so the cache is updated accordingly - as needed.
    /// </remarks>
    /// <param name="ids"></param>
    /// <param name="userId"></param>
    /// <returns>Result indicating what action was taken when handling the command.</returns>
    public OperationResult Sort(IEnumerable<int>? ids, int userId = Constants.Security.SuperUserId)
        => MoveOperationService.Sort(ids, userId);

    private static bool HasUnsavedChanges(IContent content) => content.HasIdentity is false || content.IsDirty();

    public ContentDataIntegrityReport CheckDataIntegrity(ContentDataIntegrityReportOptions options)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            scope.WriteLock(Constants.Locks.ContentTree);

            ContentDataIntegrityReport report = _documentRepository.CheckDataIntegrity(options);

            if (report.FixedIssues.Count > 0)
            {
                // The event args needs a content item so we'll make a fake one with enough properties to not cause a null ref
                var root = new Content("root", -1, new ContentType(_shortStringHelper, -1)) { Id = -1, Key = Guid.Empty };
                scope.Notifications.Publish(new ContentTreeChangeNotification(root, TreeChangeTypes.RefreshAll, EventMessagesFactory.Get()));
            }

            scope.Complete();

            return report;
        }
    }

    #endregion

    #region Private Methods

    private void Audit(AuditType type, int userId, int objectId, string? message = null, string? parameters = null) =>
        AuditAsync(type, userId, objectId, message, parameters).GetAwaiter().GetResult();

    private async Task AuditAsync(AuditType type, int userId, int objectId, string? message = null, string? parameters = null)
    {
        Guid userKey = await _userIdKeyResolver.GetAsync(userId);

        await _auditService.AddAsync(
            type,
            userKey,
            objectId,
            UmbracoObjectTypes.Document.GetName(),
            message,
            parameters);
    }

    #endregion

    #region Content Types

    /// <summary>
    ///     Deletes all content of specified type. All children of deleted content is moved to Recycle Bin.
    /// </summary>
    /// <remarks>
    ///     <para>This needs extra care and attention as its potentially a dangerous and extensive operation.</para>
    ///     <para>
    ///         Deletes content items of the specified type, and only that type. Does *not* handle content types
    ///         inheritance and compositions, which need to be managed outside of this method.
    ///     </para>
    /// </remarks>
    /// <param name="contentTypeIds">Id of the <see cref="IContentType" /></param>
    /// <param name="userId">Optional Id of the user issuing the delete operation</param>
    public void DeleteOfTypes(IEnumerable<int> contentTypeIds, int userId = Constants.Security.SuperUserId)
    {
        // TODO: This currently this is called from the ContentTypeService but that needs to change,
        // if we are deleting a content type, we should just delete the data and do this operation slightly differently.
        // This method will recursively go lookup every content item, check if any of it's descendants are
        // of a different type, move them to the recycle bin, then permanently delete the content items.
        // The main problem with this is that for every content item being deleted, events are raised...
        // which we need for many things like keeping caches in sync, but we can surely do this MUCH better.
        var changes = new List<TreeChange<IContent>>();
        var moves = new List<(IContent, string)>();
        var contentTypeIdsA = contentTypeIds.ToArray();
        EventMessages eventMessages = EventMessagesFactory.Get();

        // using an immediate uow here because we keep making changes with
        // PerformMoveLocked and DeleteLocked that must be applied immediately,
        // no point queuing operations
        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            scope.WriteLock(Constants.Locks.ContentTree);

            IQuery<IContent> query = Query<IContent>().WhereIn(x => x.ContentTypeId, contentTypeIdsA);
            IContent[] contents = _documentRepository.Get(query).ToArray();

            if (contents is null)
            {
                return;
            }

            if (scope.Notifications.PublishCancelable(new ContentDeletingNotification(contents, eventMessages)))
            {
                scope.Complete();
                return;
            }

            // order by level, descending, so deepest first - that way, we cannot move
            // a content of the deleted type, to the recycle bin (and then delete it...)
            foreach (IContent content in contents.OrderByDescending(x => x.ParentId))
            {
                // if it's not trashed yet, and published, we should unpublish
                // but... Unpublishing event makes no sense (not going to cancel?) and no need to save
                // just raise the event
                if (content.Trashed == false && content.Published)
                {
                    scope.Notifications.Publish(new ContentUnpublishedNotification(content, eventMessages));
                }

                // if current content has children, move them to trash
                IContent c = content;
                IQuery<IContent> childQuery = Query<IContent>().Where(x => x.ParentId == c.Id);
                IEnumerable<IContent> children = _documentRepository.Get(childQuery);
                foreach (IContent child in children)
                {
                    // see MoveToRecycleBin
                    PerformMoveLocked(child, Constants.System.RecycleBinContent, null, userId, moves, true);
                    changes.Add(new TreeChange<IContent>(content, TreeChangeTypes.RefreshBranch));
                }

                // delete content
                // triggers the deleted event (and handles the files)
                DeleteLocked(scope, content, eventMessages);
                changes.Add(new TreeChange<IContent>(content, TreeChangeTypes.Remove));
            }

            MoveToRecycleBinEventInfo<IContent>[] moveInfos = moves
                .Select(x => new MoveToRecycleBinEventInfo<IContent>(x.Item1, x.Item2))
                .ToArray();
            if (moveInfos.Length > 0)
            {
                scope.Notifications.Publish(new ContentMovedToRecycleBinNotification(moveInfos, eventMessages));
            }

            scope.Notifications.Publish(new ContentTreeChangeNotification(changes, eventMessages));

            Audit(AuditType.Delete, userId, Constants.System.Root, $"Delete content of type {string.Join(",", contentTypeIdsA)}");

            scope.Complete();
        }
    }

    /// <summary>
    ///     Deletes all content items of specified type. All children of deleted content item is moved to Recycle Bin.
    /// </summary>
    /// <remarks>This needs extra care and attention as its potentially a dangerous and extensive operation</remarks>
    /// <param name="contentTypeId">Id of the <see cref="IContentType" /></param>
    /// <param name="userId">Optional id of the user deleting the media</param>
    public void DeleteOfType(int contentTypeId, int userId = Constants.Security.SuperUserId) =>
        DeleteOfTypes(new[] { contentTypeId }, userId);

    private IContentType GetContentType(ICoreScope scope, string contentTypeAlias)
    {
        if (contentTypeAlias == null)
        {
            throw new ArgumentNullException(nameof(contentTypeAlias));
        }

        if (string.IsNullOrWhiteSpace(contentTypeAlias))
        {
            throw new ArgumentException("Value can't be empty or consist only of white-space characters.", nameof(contentTypeAlias));
        }

        scope.ReadLock(Constants.Locks.ContentTypes);

        IQuery<IContentType> query = Query<IContentType>().Where(x => x.Alias == contentTypeAlias);
        IContentType? contentType = _contentTypeRepository.Get(query).FirstOrDefault()
            ??
        // causes rollback
            throw new Exception($"No ContentType matching the passed in Alias: '{contentTypeAlias}'" +
            $" was found");

        return contentType;
    }

    private IContentType GetContentType(string contentTypeAlias)
    {
        if (contentTypeAlias == null)
        {
            throw new ArgumentNullException(nameof(contentTypeAlias));
        }

        if (string.IsNullOrWhiteSpace(contentTypeAlias))
        {
            throw new ArgumentException("Value can't be empty or consist only of white-space characters.", nameof(contentTypeAlias));
        }

        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            return GetContentType(scope, contentTypeAlias);
        }
    }

    #endregion

    #region Blueprints

    public IContent? GetBlueprintById(int id)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            scope.ReadLock(Constants.Locks.ContentTree);
            IContent? blueprint = _documentBlueprintRepository.Get(id);
            if (blueprint != null)
            {
                blueprint.Blueprint = true;
            }

            return blueprint;
        }
    }

    public IContent? GetBlueprintById(Guid id)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            scope.ReadLock(Constants.Locks.ContentTree);
            IContent? blueprint = _documentBlueprintRepository.Get(id);
            if (blueprint != null)
            {
                blueprint.Blueprint = true;
            }

            return blueprint;
        }
    }

    public void SaveBlueprint(IContent content, int userId = Constants.Security.SuperUserId)
        => SaveBlueprint(content, null, userId);

    public void SaveBlueprint(IContent content, IContent? createdFromContent, int userId = Constants.Security.SuperUserId)
    {
        EventMessages evtMsgs = EventMessagesFactory.Get();

        content.Blueprint = true;

        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            scope.WriteLock(Constants.Locks.ContentTree);

            if (content.HasIdentity == false)
            {
                content.CreatorId = userId;
            }

            content.WriterId = userId;

            _documentBlueprintRepository.Save(content);

            Audit(AuditType.Save, userId, content.Id, $"Saved content template: {content.Name}");

            scope.Notifications.Publish(new ContentSavedBlueprintNotification(content, createdFromContent, evtMsgs));
            scope.Notifications.Publish(new ContentTreeChangeNotification(content, TreeChangeTypes.RefreshNode, evtMsgs));

            scope.Complete();
        }
    }

    public void DeleteBlueprint(IContent content, int userId = Constants.Security.SuperUserId)
    {
        EventMessages evtMsgs = EventMessagesFactory.Get();

        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            scope.WriteLock(Constants.Locks.ContentTree);
            _documentBlueprintRepository.Delete(content);
            scope.Notifications.Publish(new ContentDeletedBlueprintNotification(content, evtMsgs));
            scope.Notifications.Publish(new ContentTreeChangeNotification(content, TreeChangeTypes.Remove, evtMsgs));
            scope.Complete();
        }
    }

    private static readonly string?[] ArrayOfOneNullString = { null };

    public IContent CreateBlueprintFromContent(
        IContent blueprint,
        string name,
        int userId = Constants.Security.SuperUserId)
    {
        ArgumentNullException.ThrowIfNull(blueprint);

        IContentType contentType = GetContentType(blueprint.ContentType.Alias);
        var content = new Content(name, -1, contentType);
        content.Path = string.Concat(content.ParentId.ToString(), ",", content.Id);

        content.CreatorId = userId;
        content.WriterId = userId;

        IEnumerable<string?> cultures = ArrayOfOneNullString;
        if (blueprint.CultureInfos?.Count > 0)
        {
            cultures = blueprint.CultureInfos.Values.Select(x => x.Culture);
            using ICoreScope scope = ScopeProvider.CreateCoreScope();
            if (blueprint.CultureInfos.TryGetValue(_languageRepository.GetDefaultIsoCode(), out ContentCultureInfos defaultCulture))
            {
                defaultCulture.Name = name;
            }

            scope.Complete();
        }

        DateTime now = DateTime.UtcNow;
        foreach (var culture in cultures)
        {
            foreach (IProperty property in blueprint.Properties)
            {
                var propertyCulture = property.PropertyType.VariesByCulture() ? culture : null;
                content.SetValue(property.Alias, property.GetValue(propertyCulture), propertyCulture);
            }

            if (!string.IsNullOrEmpty(culture))
            {
                content.SetCultureInfo(culture, blueprint.GetCultureName(culture), now);
            }
        }

        return content;
    }

    /// <inheritdoc />
    [Obsolete("Use IContentBlueprintEditingService.GetScaffoldedAsync() instead. Scheduled for removal in V18.")]
    public IContent CreateContentFromBlueprint(IContent blueprint, string name, int userId = Constants.Security.SuperUserId)
        => CreateBlueprintFromContent(blueprint, name, userId);

    public IEnumerable<IContent> GetBlueprintsForContentTypes(params int[] contentTypeId)
    {
        using (ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            IQuery<IContent> query = Query<IContent>();
            if (contentTypeId.Length > 0)
            {
                // Need to use a List here because the expression tree cannot convert the array when used in Contains.
                // See ExpressionTests.Sql_In().
                List<int> contentTypeIdsAsList = [.. contentTypeId];
                query.Where(x => contentTypeIdsAsList.Contains(x.ContentTypeId));
            }

            return _documentBlueprintRepository.Get(query).Select(x =>
            {
                x.Blueprint = true;
                return x;
            });
        }
    }

    public void DeleteBlueprintsOfTypes(IEnumerable<int> contentTypeIds, int userId = Constants.Security.SuperUserId)
    {
        EventMessages evtMsgs = EventMessagesFactory.Get();

        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            scope.WriteLock(Constants.Locks.ContentTree);

            // Need to use a List here because the expression tree cannot convert an array when used in Contains.
            // See ExpressionTests.Sql_In().
            var contentTypeIdsAsList = contentTypeIds.ToList();

            IQuery<IContent> query = Query<IContent>();
            if (contentTypeIdsAsList.Count > 0)
            {
                query.Where(x => contentTypeIdsAsList.Contains(x.ContentTypeId));
            }

            IContent[]? blueprints = _documentBlueprintRepository.Get(query)?.Select(x =>
            {
                x.Blueprint = true;
                return x;
            }).ToArray();

            if (blueprints is not null)
            {
                foreach (IContent blueprint in blueprints)
                {
                    _documentBlueprintRepository.Delete(blueprint);
                }

                scope.Notifications.Publish(new ContentDeletedBlueprintNotification(blueprints, evtMsgs));
                scope.Notifications.Publish(new ContentTreeChangeNotification(blueprints, TreeChangeTypes.Remove, evtMsgs));
                scope.Complete();
            }
        }
    }

    public void DeleteBlueprintsOfType(int contentTypeId, int userId = Constants.Security.SuperUserId) =>
        DeleteBlueprintsOfTypes(new[] { contentTypeId }, userId);

    #endregion

}
