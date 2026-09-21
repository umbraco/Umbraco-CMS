using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Events;
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
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Implements the content service.
/// </summary>
public class ContentService : AsyncPublishableContentServiceBase<IContent>, IContentService
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IDocumentBlueprintRepository _documentBlueprintRepository;
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

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="ContentService"/> class.
    /// </summary>
    /// <param name="provider">The core scope provider.</param>
    /// <param name="loggerFactory">The logger factory.</param>
    /// <param name="eventMessagesFactory">The event messages factory.</param>
    /// <param name="auditService">The audit service.</param>
    /// <param name="contentTypeRepository">The content type repository.</param>
    /// <param name="languageRepository">The language repository.</param>
    /// <param name="propertyValidationService">The property validation service.</param>
    /// <param name="shortStringHelper">The short string helper.</param>
    /// <param name="cultureImpactFactory">The culture impact factory.</param>
    /// <param name="userIdKeyResolver">The user ID key resolver.</param>
    /// <param name="propertyEditorCollection">The property editor collection.</param>
    /// <param name="idKeyMap">The ID key map.</param>
    /// <param name="optionsMonitor">The content settings options monitor.</param>
    /// <param name="relationService">The relation service.</param>
    /// <param name="documentRepository">The document repository.</param>
    /// <param name="documentBlueprintRepository">The document blueprint repository.</param>
    public ContentService(
        ICoreScopeProvider provider,
        ILoggerFactory loggerFactory,
        IEventMessagesFactory eventMessagesFactory,
        IAuditService auditService,
        IContentTypeRepository contentTypeRepository,
        ILanguageRepository languageRepository,
        Lazy<IPropertyValidationService> propertyValidationService,
        IShortStringHelper shortStringHelper,
        ICultureImpactFactory cultureImpactFactory,
        IUserIdKeyResolver userIdKeyResolver,
        PropertyEditorCollection propertyEditorCollection,
        IIdKeyMap idKeyMap,
        IOptionsMonitor<ContentSettings> optionsMonitor,
        IRelationService relationService,
        IDocumentRepository documentRepository,
        IDocumentBlueprintRepository documentBlueprintRepository)
        : base(
            provider,
            loggerFactory,
            eventMessagesFactory,
            auditService,
            contentTypeRepository,
            documentRepository,
            languageRepository,
            propertyValidationService,
            cultureImpactFactory,
            userIdKeyResolver,
            propertyEditorCollection,
            idKeyMap)
    {
        _documentRepository = documentRepository;
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
    }

    #endregion

    #region Permissions

    /// <inheritdoc />
#pragma warning disable CS0618 // Type or member is obsolete
    public async Task SetPermissionsAsync(EntityPermissionSet permissionSet, CancellationToken cancellationToken)
#pragma warning restore CS0618 // Type or member is obsolete
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope();
        scope.WriteLock(Constants.Locks.ContentTree);
        await _documentRepository.ReplaceContentPermissionsAsync(permissionSet, cancellationToken);
        scope.Complete();
    }

    /// <inheritdoc />
#pragma warning disable CS0618 // Type or member is obsolete
    public async Task SetPermissionAsync(IContent entity, string permission, IEnumerable<Guid> groupKeys, CancellationToken cancellationToken)
#pragma warning restore CS0618 // Type or member is obsolete
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope();
        scope.WriteLock(Constants.Locks.ContentTree);
        await _documentRepository.AssignEntityPermissionAsync(entity, permission, groupKeys, cancellationToken);
        scope.Complete();
    }

    /// <inheritdoc />
    public async Task<EntityPermissionCollection> GetPermissionsAsync(Guid contentKey, CancellationToken cancellationToken)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope();
        scope.ReadLock(Constants.Locks.ContentTree);
        EntityPermissionCollection result = await _documentRepository.GetPermissionsForEntityAsync(contentKey, cancellationToken);
        scope.Complete();
        return result;
    }

    #endregion

    #region Create

    /// <inheritdoc />
    public async Task<IContent> CreateAsync(string name, Guid? parentKey, string contentTypeAlias, Guid userKey, CancellationToken cancellationToken)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope();

        IContentType contentType = await GetContentTypeAsync(scope, contentTypeAlias, cancellationToken);
        IContent content = await CreateAsync(name, parentKey, contentType, userKey, cancellationToken);

        scope.Complete();
        return content;
    }

    /// <inheritdoc />
    public async Task<IContent> CreateAsync(string name, Guid? parentKey, IContentType contentType, Guid userKey, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(contentType);

        using ICoreScope scope = ScopeProvider.CreateCoreScope();

        int userId = await _userIdKeyResolver.GetAsync(userKey);

        IContent? parent = null;
        if (parentKey.HasValue)
        {
            parent = await GetByIdAsync(parentKey.Value, cancellationToken);
            if (parent is null)
            {
                throw new ArgumentException("No content with that key.", nameof(parentKey));
            }
        }

        Content content = parent is not null
            ? new Content(name, parent, contentType, userId)
            : new Content(name, Constants.System.Root, contentType, userId);

        scope.Complete();
        return content;
    }

    /// <inheritdoc />
    public async Task<IContent> CreateAsync(string name, IContent parent, string contentTypeAlias, Guid userKey, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(parent);

        using ICoreScope scope = ScopeProvider.CreateCoreScope();

        int userId = await _userIdKeyResolver.GetAsync(userKey);
        IContentType contentType = await GetContentTypeAsync(scope, contentTypeAlias, cancellationToken);

        var content = new Content(name, parent, contentType, userId);

        scope.Complete();
        return content;
    }

    /// <inheritdoc />
    public async Task<IContent> CreateAndSaveAsync(string name, Guid? parentKey, string contentTypeAlias, Guid userKey, CancellationToken cancellationToken)
    {
        // TODO: what about culture?
        using ICoreScope scope = ScopeProvider.CreateCoreScope();

        // locking the content tree secures content types too
        scope.WriteLock(Constants.Locks.ContentTree);

        int userId = await _userIdKeyResolver.GetAsync(userKey);

        IContentType contentType = GetContentType(contentTypeAlias)
            // + locks
            ??
            // causes rollback
            throw new ArgumentException("No content type with that alias.", nameof(contentTypeAlias));

        IContent? parent = null;
        if (parentKey.HasValue)
        {
            parent = await GetByIdAsync(parentKey.Value, cancellationToken); // + locks
            if (parent is null)
            {
                throw new ArgumentException("No content with that key.", nameof(parentKey)); // causes rollback
            }
        }

        Content content = parent is not null
            ? new Content(name, parent, contentType, userId)
            : new Content(name, Constants.System.Root, contentType, userId);

        await SaveAsync(content, userKey, null, cancellationToken);

        scope.Complete();

        return content;
    }

    /// <inheritdoc />
    public async Task<IContent> CreateAndSaveAsync(string name, IContent parent, string contentTypeAlias, Guid userKey, CancellationToken cancellationToken)
    {
        // TODO: what about culture?
        ArgumentNullException.ThrowIfNull(parent);

        using ICoreScope scope = ScopeProvider.CreateCoreScope();

        // locking the content tree secures content types too
        scope.WriteLock(Constants.Locks.ContentTree);

        int userId = await _userIdKeyResolver.GetAsync(userKey);

        IContentType contentType = GetContentType(contentTypeAlias)
        // + locks
            ??
            // causes rollback
            throw new ArgumentException("No content type with that alias.", nameof(contentTypeAlias));

        var content = new Content(name, parent, contentType, userId);

        await SaveAsync(content, userKey, null, cancellationToken);

        scope.Complete();
        return content;
    }

    #endregion

    #region Get, Has, Is

    /// <inheritdoc />
    public async Task<PagedModel<IContent>> GetByLevelAsync(int level, int skip, int take, Ordering? ordering, CancellationToken cancellationToken)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
        scope.ReadLock(Constants.Locks.ContentTree);
        return await _documentRepository.GetByLevelAsync(level, skip, take, ordering, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<PagedModel<IContent>> GetAncestorsAsync(Guid key, int skip, int take, CancellationToken cancellationToken)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
        scope.ReadLock(Constants.Locks.ContentTree);
        return await _documentRepository.GetAncestorsAsync(key, skip, take, cancellationToken);
    }

    /// <inheritdoc />
    public Task<PagedModel<IContent>> GetAncestorsAsync(IContent content, int skip, int take, CancellationToken cancellationToken) =>
        GetAncestorsAsync(content.Key, skip, take, cancellationToken);

    /// <inheritdoc />
    public async Task<PagedModel<IContent>> GetChildrenAsync(Guid? parentKey, int skip, int take, string[]? propertyAliases, Ordering? ordering, CancellationToken cancellationToken)
    {
        ordering ??= Ordering.By("sortOrder");

        using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
        scope.ReadLock(Constants.Locks.ContentTree);
        return await _documentRepository.GetChildrenAsync(parentKey, skip, take, propertyAliases, ordering, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<PagedModel<IContent>> GetChildrenWithoutTemplatesAsync(Guid? parentKey, int skip, int take, string[]? propertyAliases, Ordering? ordering, CancellationToken cancellationToken)
    {
        ordering ??= Ordering.By("sortOrder");

        using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
        scope.ReadLock(Constants.Locks.ContentTree);
        return await _documentRepository.GetChildrenWithoutTemplatesAsync(parentKey, skip, take, propertyAliases, ordering, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<PagedModel<IContent>> GetDescendantsAsync(Guid ancestorKey, int skip, int take, Ordering? ordering, CancellationToken cancellationToken, bool includeTrashed = true)
    {
        ordering ??= Ordering.By("Path");

        using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
        scope.ReadLock(Constants.Locks.ContentTree);
        return await _documentRepository.GetDescendantsAsync(ancestorKey, skip, take, ordering, cancellationToken, includeTrashed);
    }

    /// <inheritdoc />
    public async Task<PagedModel<IContent>> GetDescendantsWithoutTemplatesAsync(Guid ancestorKey, int skip, int take, Ordering? ordering, CancellationToken cancellationToken, bool includeTrashed = true)
    {
        ordering ??= Ordering.By("Path");

        using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
        scope.ReadLock(Constants.Locks.ContentTree);
        return await _documentRepository.GetDescendantsWithoutTemplatesAsync(ancestorKey, skip, take, ordering, cancellationToken, includeTrashed);
    }

    /// <inheritdoc />
    public async Task<IContent?> GetParentAsync(Guid key, CancellationToken cancellationToken)
    {
        IContent? content = await GetByIdAsync(key, cancellationToken);
        if (content is null || content.ParentId == Constants.System.Root || content.ParentId == Constants.System.RecycleBinContent)
        {
            return null;
        }

        Guid? parentKey = content.ParentKey;
        return parentKey is null
            ? null
            : await GetByIdAsync(parentKey.Value, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<IContent>> GetRootContentAsync(CancellationToken cancellationToken)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope();
        scope.ReadLock(Constants.Locks.ContentTree);
        IEnumerable<IContent> result = await _documentRepository.GetRootContentAsync(cancellationToken);
        scope.Complete();
        return result;
    }

    /// <inheritdoc />
    public async Task<PagedModel<IContent>> GetPagedContentInRecycleBinAsync(int skip, int take, Ordering? ordering, CancellationToken cancellationToken)
    {
        ordering ??= Ordering.By("Path");

        using ICoreScope scope = ScopeProvider.CreateCoreScope();
        scope.ReadLock(Constants.Locks.ContentTree);
        PagedModel<IContent> result = await _documentRepository.GetPagedRecycleBinAsync(skip, take, ordering, cancellationToken);
        scope.Complete();
        return result;
    }

    /// <inheritdoc />
    public async Task<bool> RecycleBinSmellsAsync(CancellationToken cancellationToken)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope();
        scope.ReadLock(Constants.Locks.ContentTree);
        bool result = await _documentRepository.RecycleBinSmellsAsync(cancellationToken);
        scope.Complete();
        return result;
    }

    /// <inheritdoc />
    public async Task<bool> IsPathPublishableAsync(IContent content, CancellationToken cancellationToken)
    {
        // fast
        if (content.ParentId == Constants.System.Root)
        {
            return true; // root content is always publishable
        }

        if (content.Trashed)
        {
            return false; // trashed content is never publishable
        }

        // not trashed and has a parent: publishable if the parent is path-published
        Guid? parentKey;
        try
        {
            parentKey = content.ParentKey;
        }
        catch (NotSupportedException)
        {
            TryGetParentKey(content.ParentId, out parentKey);
        }

        IContent? parent = parentKey is null
            ? null
            : await GetByIdAsync(parentKey.Value, cancellationToken);
        return parent is null || await IsPathPublishedAsync(parent, cancellationToken);
    }

    /// <summary>
    /// Checks if the <see cref="IContent"/> and all its ancestors are published.
    /// </summary>
    /// <param name="content">The content to check.</param>
    /// <returns><c>true</c> if the content and all its ancestors are published; otherwise, <c>false</c>.</returns>
    #endregion

    #region Save, Publish, Unpublish

    /// <inheritdoc />
    /// <summary>
    ///     Publishes/unpublishes any pending publishing changes made to the document.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This MUST NOT be called from within this service, this used to be a public API and must only be used outside of
    ///         this service.
    ///         Internally in this service, calls must be made to CommitContentChangesInternalAsync
    ///     </para>
    ///     <para>This is the underlying logic for both publishing and unpublishing any document</para>
    ///     <para>
    ///         Pending publishing/unpublishing changes on a document are made with calls to
    ///         <see cref="ContentRepositoryExtensions.PublishCulture" /> and
    ///         <see cref="ContentRepositoryExtensions.UnpublishCulture" />.
    ///     </para>
    ///     <para>
    ///         When publishing or unpublishing a single culture, or all cultures, use the publishing operations
    ///         and <see cref="IPublishableContentService{TContent}.UnpublishAsync" />. But if the flexibility to both publish and unpublish in a single operation is
    ///         required, then this method needs to be used in combination with <see cref="ContentRepositoryExtensions.PublishCulture" />
    ///         and <see cref="ContentRepositoryExtensions.UnpublishCulture" />
    ///         on the content itself - this prepares the content, but does not commit anything - and then, invoke
    ///         <see cref="CommitDocumentChangesAsync" /> to actually commit the changes to the database.
    ///     </para>
    ///     <para>The document is *always* saved, even when publishing fails.</para>
    /// </remarks>
    internal async Task<PublishResult> CommitDocumentChangesAsync(IContent content, Guid userKey, CancellationToken cancellationToken)
    {
        int userId = await _userIdKeyResolver.GetAsync(userKey);

        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            EventMessages evtMsgs = EventMessagesFactory.Get();

            scope.WriteLock(Constants.Locks.ContentTree);

            var savingNotification = new ContentSavingNotification(content, evtMsgs);
            if (await scope.Notifications.PublishCancelableAsync(savingNotification))
            {
                return new PublishResult(PublishResultType.FailedPublishCancelledByEvent, evtMsgs, content);
            }

            var allLangs = (await _languageRepository.GetAllAsync(cancellationToken)).ToList();

            PublishResult result = await CommitContentChangesInternalAsync(scope, content, evtMsgs, allLangs, savingNotification.State, userId, cancellationToken);
            scope.Complete();
            return result;
        }
    }

    // utility 'PublishCultures' func used by SaveAndPublishBranch
    private bool PublishBranch_PublishCultures(IContent content, HashSet<string> culturesToPublish, IReadOnlyCollection<ILanguage> allLangs)
    {
        // variant content type - publish specified cultures
        // invariant content type - publish only the invariant culture

        var publishTime = DateTime.UtcNow;
        if (content.ContentType.VariesByCulture())
        {
            return culturesToPublish.All(culture =>
            {
                CultureImpact? impact = _cultureImpactFactory.Create(culture, IsDefaultCulture(allLangs, culture), content);
                return content.PublishCulture(impact, publishTime, _propertyEditorCollection) &&
                       _propertyValidationService.Value.IsPropertyDataValid(content, out _, impact);
            });
        }

        return content.PublishCulture(_cultureImpactFactory.ImpactInvariant(), publishTime, _propertyEditorCollection)
               && _propertyValidationService.Value.IsPropertyDataValid(content, out _, _cultureImpactFactory.ImpactInvariant());
    }

    // utility 'ShouldPublish' func used by PublishBranch
    private static HashSet<string>? PublishBranch_ShouldPublish(ref HashSet<string>? cultures, string c, bool published, bool edited, bool isRoot, PublishBranchFilter publishBranchFilter)
    {
        // if published, republish
        if (published)
        {
            cultures ??= new HashSet<string>(); // empty means 'already published'

            if (edited || publishBranchFilter.HasFlag(PublishBranchFilter.ForceRepublish))
            {
                cultures.Add(c); // <culture> means 'republish this culture'
            }

            return cultures;
        }

        // if not published, publish if force/root else do nothing
        if (!publishBranchFilter.HasFlag(PublishBranchFilter.IncludeUnpublished) && !isRoot)
        {
            return cultures; // null means 'nothing to do'
        }

        cultures ??= new HashSet<string>();

        cultures.Add(c); // <culture> means 'publish this culture'
        return cultures;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<PublishResult>> PublishBranchAsync(IContent content, PublishBranchFilter publishBranchFilter, string[] cultures, Guid userKey, CancellationToken cancellationToken)
    {
        // note: EditedValue and PublishedValue are objects here, so it is important to .Equals()
        // and not to == them, else we would be comparing references, and that is a bad thing

        int userId = await _userIdKeyResolver.GetAsync(userKey);

        cultures = EnsureCultures(content, cultures);

        string? defaultCulture;
        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            defaultCulture = await _languageRepository.GetDefaultIsoCodeAsync();
            scope.Complete();
        }

        // determines cultures to be published
        // can be: null (content is not impacted), an empty set (content is impacted but already published), or cultures
        HashSet<string>? ShouldPublish(IContent c)
        {
            var isRoot = c.Id == content.Id;
            HashSet<string>? culturesToPublish = null;

            // invariant content type
            if (!c.ContentType.VariesByCulture())
            {
                return PublishBranch_ShouldPublish(ref culturesToPublish, "*", c.Published, c.Edited, isRoot, publishBranchFilter);
            }

            // variant content type, specific cultures
            if (c.Published)
            {
                // then some (and maybe all) cultures will be 'already published' (unless forcing),
                // others will have to 'republish this culture'
                foreach (var culture in cultures)
                {
                    // We could be publishing a parent invariant page, with descendents that are variant.
                    // So convert the invariant request to a request for the default culture.
                    var specificCulture = culture == "*" ? defaultCulture : culture;

                    PublishBranch_ShouldPublish(ref culturesToPublish, specificCulture, c.IsCulturePublished(specificCulture), c.IsCultureEdited(specificCulture), isRoot, publishBranchFilter);
                }

                return culturesToPublish;
            }

            // if not published, publish if forcing unpublished/root else do nothing
            return publishBranchFilter.HasFlag(PublishBranchFilter.IncludeUnpublished) || isRoot
                ? new HashSet<string>(cultures) // means 'publish specified cultures'
                : null; // null means 'nothing to do'
        }

        return await PublishBranchAsync(content, ShouldPublish, PublishBranch_PublishCultures, userId, cancellationToken);
    }

    private static string[] EnsureCultures(IContent content, string[] cultures)
    {
        // Ensure consistent indication of "all cultures" for variant content.
        if (content.ContentType.VariesByCulture() is false && ProvidedCulturesIndicatePublishAll(cultures))
        {
            cultures = ["*"];
        }

        return cultures.Select(x => x.EnsureCultureCode()!).ToArray();
    }

    private static bool ProvidedCulturesIndicatePublishAll(string[] cultures) => cultures.Length == 0 || (cultures.Length == 1 && cultures[0] == "invariant");

    /// <summary>
    /// Publishes a branch of content items starting from the specified document.
    /// </summary>
    /// <param name="document">The content item to start publishing from.</param>
    /// <param name="shouldPublish">A function that determines which cultures should be published for each content item. Returns null if the item should not be published.</param>
    /// <param name="publishCultures">A function that handles the actual publishing of cultures for each content item.</param>
    /// <param name="userId">The identifier of the user performing the publish operation.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A collection of <see cref="PublishResult"/> representing the results of publishing each content item in the branch.</returns>
    internal async Task<IEnumerable<PublishResult>> PublishBranchAsync(
        IContent document,
        Func<IContent, HashSet<string>?> shouldPublish,
        Func<IContent, HashSet<string>, IReadOnlyCollection<ILanguage>, bool> publishCultures,
        int userId,
        CancellationToken cancellationToken)
    {
        if (shouldPublish == null)
        {
            throw new ArgumentNullException(nameof(shouldPublish));
        }

        if (publishCultures == null)
        {
            throw new ArgumentNullException(nameof(publishCultures));
        }

        EventMessages eventMessages = EventMessagesFactory.Get();
        var results = new List<PublishResult>();
        var publishedDocuments = new List<IContent>();

        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            scope.WriteLock(Constants.Locks.ContentTree);

            var allLangs = (await _languageRepository.GetAllAsync(cancellationToken)).ToList();

            if (!document.HasIdentity)
            {
                throw new InvalidOperationException("Cannot not branch-publish a new document.");
            }

            PublishedState publishedState = document.PublishedState;
            if (publishedState == PublishedState.Publishing)
            {
                throw new InvalidOperationException("Cannot mix PublishCulture and SaveAndPublishBranch.");
            }

            // captures the cultures published per document, so the notification can report them per item
            var publishedCulturesByDocument = new Dictionary<Guid, IReadOnlyCollection<string>>();
            var variesByCulture = document.ContentType.VariesByCulture();

            void TrackPublishedCultures(IContent publishedDocument, HashSet<string>? documentCulturesToPublish)
            {
                // a branch can mix variant and invariant content types, so determine variance per document rather
                // than from the branch root; snapshot the cultures so the notification never holds a mutable set
                string[] cultures = publishedDocument.ContentType.VariesByCulture()
                    ? documentCulturesToPublish?.ToArray() ?? []
                    : ["*"];
                if (cultures.Length > 0)
                {
                    publishedCulturesByDocument[publishedDocument.Key] = cultures;
                }
            }

            // deal with the branch root - if it fails, abort
            HashSet<string>? culturesToPublish = shouldPublish(document);
            (PublishResult? result, IDictionary<string, object?>? notificationState) =
                await PublishBranchItemAsync(scope, document, culturesToPublish, publishCultures, true, publishedDocuments, eventMessages, userId, allLangs, cancellationToken);
            if (result != null)
            {
                results.Add(result);
                if (!result.Success)
                {
                    return results;
                }

                TrackPublishedCultures(document, culturesToPublish);
            }

            HashSet<string> culturesPublished = culturesToPublish ?? [];

            // deal with descendants
            // if one fails, abort its branch
            var exclude = new HashSet<int>();

            int count;
            var page = 0;
            const int pageSize = 100;
            do
            {
                count = 0;

                // important to order by Path ASC so make it explicit in case defaults change
                // ReSharper disable once RedundantArgumentDefaultValue
                foreach (IContent d in (await GetDescendantsAsync(document.Key, page * pageSize, pageSize, Ordering.By("Path", Direction.Ascending), cancellationToken)).Items)
                {
                    count++;

                    // if parent is excluded, exclude child too
                    if (exclude.Contains(d.ParentId))
                    {
                        exclude.Add(d.Id);
                        continue;
                    }

                    // no need to check path here, parent has to be published here
                    culturesToPublish = shouldPublish(d);
                    (result, _) = await PublishBranchItemAsync(scope, d, culturesToPublish, publishCultures, false, publishedDocuments, eventMessages, userId, allLangs, cancellationToken);
                    if (result != null)
                    {
                        results.Add(result);
                        if (result.Success)
                        {
                            culturesPublished.UnionWith(culturesToPublish ?? []);
                            TrackPublishedCultures(d, culturesToPublish);
                            continue;
                        }
                    }

                    // if we could not publish the document, cut its branch
                    exclude.Add(d.Id);
                }

                page++;
            }
            while (count > 0);

            await AuditAsync(AuditType.Publish, userId, document.Id, "Branch published");

            // trigger events for the entire branch
            // (SaveAndPublishBranchOne does *not* do it)
            scope.Notifications.Publish(
                new ContentTreeChangeNotification(
                    document,
                    TreeChangeTypes.RefreshBranch,
                    variesByCulture ? culturesPublished.IsCollectionEmpty() ? null : culturesPublished : ["*"],
                    null,
                    eventMessages));
            scope.Notifications.Publish(
                new ContentPublishedNotification(
                    publishedDocuments,
                    eventMessages,
                    true,
                    publishedCulturesByDocument,
                    null)
                .WithState(notificationState));

            scope.Complete();
        }

        return results;
    }

    // shouldPublish: a function determining whether the document has changes that need to be published
    //  note - 'force' is handled by 'editing'
    // publishValues: a function publishing values (using the appropriate PublishCulture calls)
    private async Task<(PublishResult? Result, IDictionary<string, object?>? NotificationState)> PublishBranchItemAsync(
        ICoreScope scope,
        IContent document,
        HashSet<string>? culturesToPublish,
        Func<IContent, HashSet<string>, IReadOnlyCollection<ILanguage>,
            bool> publishCultures,
        bool isRoot,
        ICollection<IContent> publishedDocuments,
        EventMessages evtMsgs,
        int userId,
        IReadOnlyCollection<ILanguage> allLangs,
        CancellationToken cancellationToken)
    {
        // TODO: this is never written to, so the branch ContentPublishedNotification always carries an empty state
        // and cannot see what a ContentSavingNotification handler wrote. Return savingNotification.State instead. [NL]
        IDictionary<string, object?>? initialNotificationState = new Dictionary<string, object?>();

        // we need to guard against unsaved changes before proceeding; the document will be saved, but we're not firing any saved notifications
        if (HasUnsavedChanges(document))
        {
            return (new PublishResult(PublishResultType.FailedPublishUnsavedChanges, evtMsgs, document), initialNotificationState);
        }

        // null = do not include
        if (culturesToPublish == null)
        {
            return (null, initialNotificationState);
        }

        // empty = already published
        if (culturesToPublish.Count == 0)
        {
            return (new PublishResult(PublishResultType.SuccessPublishAlready, evtMsgs, document), initialNotificationState);
        }

        var savingNotification = new ContentSavingNotification(document, evtMsgs);
        if (await scope.Notifications.PublishCancelableAsync(savingNotification))
        {
            return (new PublishResult(PublishResultType.FailedPublishCancelledByEvent, evtMsgs, document), initialNotificationState);
        }

        // publish & check if values are valid
        if (!publishCultures(document, culturesToPublish, allLangs))
        {
            // TODO: Based on this callback behavior there is no way to know which properties may have been invalid if this failed, see other results of FailedPublishContentInvalid
            return (new PublishResult(PublishResultType.FailedPublishContentInvalid, evtMsgs, document), initialNotificationState);
        }

        PublishResult result = await CommitContentChangesInternalAsync(scope, document, evtMsgs, allLangs, savingNotification.State, userId, cancellationToken, branchOne: true, branchRoot: isRoot);
        if (result.Success)
        {
            publishedDocuments.Add(document);
        }

        return (result, initialNotificationState);
    }

    #endregion

    #region Move, RecycleBin

    /// <inheritdoc />
    public async Task<Attempt<ContentMoveToRecycleBinOperationStatus>> MoveToRecycleBinAsync(IContent content, Guid userKey, CancellationToken cancellationToken)
    {
        EventMessages eventMessages = EventMessagesFactory.Get();
        var moves = new List<(IContent, string)>();

        using ICoreScope scope = ScopeProvider.CreateCoreScope();
        scope.WriteLock(Constants.Locks.ContentTree);

        var originalPath = content.Path;
        var moveEventInfo = new MoveToRecycleBinEventInfo<IContent>(content, originalPath);

        var movingToRecycleBinNotification = new ContentMovingToRecycleBinNotification(moveEventInfo, eventMessages);
        if (await scope.Notifications.PublishCancelableAsync(movingToRecycleBinNotification))
        {
            scope.Complete();
            return Attempt.Fail(ContentMoveToRecycleBinOperationStatus.CancelledByNotification);
        }

        int userId = await _userIdKeyResolver.GetAsync(userKey);

        // if it's published we may want to force-unpublish it - that would be backward-compatible... but...
        // making a radical decision here: trashing is equivalent to moving under an unpublished node so
        // it's NOT unpublishing, only the content is now masked - allowing us to restore it if wanted
        // if (content.HasPublishedVersion)
        // { }
        await PerformMoveLockedAsync(content, Constants.System.RecycleBinContent, null, userId, moves, true, cancellationToken);
        scope.Notifications.Publish(
            new ContentTreeChangeNotification(content, TreeChangeTypes.RefreshBranch, eventMessages));

        MoveToRecycleBinEventInfo<IContent>[] moveInfo = moves
            .Select(x => new MoveToRecycleBinEventInfo<IContent>(x.Item1, x.Item2))
            .ToArray();

        scope.Notifications.Publish(
            new ContentMovedToRecycleBinNotification(moveInfo, eventMessages).WithStateFrom(
                movingToRecycleBinNotification));

        await AuditAsync(AuditType.Move, userId, content.Id, $"Moved to recycle bin from parent {originalPath.GetParentIdFromPath()}");

        scope.Complete();

        return Attempt.Succeed(ContentMoveToRecycleBinOperationStatus.Success);
    }

    /// <inheritdoc />
    public async Task<Attempt<ContentMoveOperationStatus>> MoveAsync(IContent content, Guid? parentKey, bool includeDescendants, Guid userKey, CancellationToken cancellationToken)
    {
        EventMessages eventMessages = EventMessagesFactory.Get();

        // if moving to the recycle bin then use the proper method
        if (parentKey == Constants.System.RecycleBinContentKey)
        {
            if (content.ParentId == Constants.System.RecycleBinContent)
            {
                return Attempt.Succeed(ContentMoveOperationStatus.Success);
            }

            Attempt<ContentMoveToRecycleBinOperationStatus> recycleBinResult = await MoveToRecycleBinAsync(content, userKey, cancellationToken);
            return recycleBinResult.Success
                ? Attempt.Succeed(ContentMoveOperationStatus.Success)
                : Attempt.Fail(ContentMoveOperationStatus.CancelledByNotification);
        }

        if (parentKey is null && content.ParentId == Constants.System.Root)
        {
            return Attempt.Succeed(ContentMoveOperationStatus.Success);
        }

        int userId = await _userIdKeyResolver.GetAsync(userKey);
        var moves = new List<(IContent, string)>();

        using ICoreScope scope = ScopeProvider.CreateCoreScope();
        scope.WriteLock(Constants.Locks.ContentTree);

        IContent? parent = parentKey.HasValue ? await GetByIdAsync(parentKey.Value, cancellationToken) : null;
        if (parentKey.HasValue && (parent is null || parent.Trashed))
        {
            throw new InvalidOperationException("Parent does not exist or is trashed."); // causes rollback
        }

        int parentId = parent?.Id ?? Constants.System.Root;

        // Content.ParentKey can throw for content whose parent key was never populated (e.g. built and
        // saved without a subsequent reload) - comparing the resolved int id instead is always safe.
        if (content.ParentId == parentId)
        {
            scope.Complete();
            return Attempt.Succeed(ContentMoveOperationStatus.Success);
        }

        var moveEventInfo = new MoveEventInfo<IContent>(content, content.Path, parentKey);

        var movingNotification = new ContentMovingNotification(moveEventInfo, eventMessages);
        if (await scope.Notifications.PublishCancelableAsync(movingNotification))
        {
            scope.Complete();
            return Attempt.Fail(ContentMoveOperationStatus.CancelledByNotification);
        }

        // if content was trashed, and since we're not moving to the recycle bin,
        // indicate that the trashed status should be changed to false, else just
        // leave it unchanged
        var trashed = content.Trashed ? false : (bool?)null;

        // when restoring a single item out of the recycle bin without its descendants, those descendants stay
        // trashed and are re-homed under the recycle bin root - see PerformMoveLockedAsync
        var leaveDescendantsInRecycleBin = includeDescendants is false && content.Trashed;

        // if the content was trashed under another content, and so has a published version,
        // it cannot move back as published but has to be unpublished first - that's for the
        // root content, everything underneath will retain its published status
        if (content.Trashed && content.Published)
        {
            // however, it had been masked when being trashed, so there's no need for
            // any special event here - just change its state
            content.PublishedState = PublishedState.Unpublishing;
        }

        await PerformMoveLockedAsync(content, parentId, parent, userId, moves, trashed, cancellationToken, includeDescendants);

        if (leaveDescendantsInRecycleBin)
        {
            // The single RefreshBranch above cannot reconcile the descendants left in the bin (they are no longer
            // descendants of the restored item), so also refresh the re-homed direct children. The navigation
            // reconciler then moves them - and their sub-trees - back under the recycle bin root.
            IContent[] rehomedChildren = moves
                .Select(x => x.Item1)
                .Where(x => x.ParentId == Constants.System.RecycleBinContent)
                .ToArray();
            scope.Notifications.Publish(
                new ContentTreeChangeNotification(content.Yield().Concat(rehomedChildren), TreeChangeTypes.RefreshBranch, eventMessages));
        }
        else
        {
            scope.Notifications.Publish(
                new ContentTreeChangeNotification(content, TreeChangeTypes.RefreshBranch, eventMessages));
        }

        // changes
        MoveEventInfo<IContent>[] moveInfo = moves
            .Select(x => new MoveEventInfo<IContent>(x.Item1, x.Item2, x.Item1.ParentKey))
            .ToArray();

        scope.Notifications.Publish(
            new ContentMovedNotification(moveInfo, eventMessages).WithStateFrom(movingNotification));

        await AuditAsync(AuditType.Move, userId, content.Id);

        scope.Complete();
        return Attempt.Succeed(ContentMoveOperationStatus.Success);
    }

    // MUST be called from within a write lock on Constants.Locks.ContentTree.
    // trash indicates whether we are trashing, un-trashing, or not changing anything.
    private async Task PerformMoveLockedAsync(IContent content, int parentId, IContent? parent, int userId, List<(IContent Content, string OriginalPath)> moves, bool? trash, CancellationToken cancellationToken, bool includeDescendants = true)
    {
        content.WriterId = userId;
        content.ParentId = parentId;
        content.ParentKey = parentId switch
        {
            Constants.System.Root => null,
            Constants.System.RecycleBinContent => Constants.System.RecycleBinContentKey,
            _ => parent?.Key,
        };

        var levelDelta = 1 - content.Level + (parent?.Level ?? 0);
        var originalLevel = content.Level;
        var originalPath = content.Path;
        moves.Add((content, originalPath));

        // Fetch descendants by content's key before saving its own new parent below - GetDescendantsAsync
        // matches descendants via content's NodeId embedded in their own Path strings, so this must happen
        // before those descendant rows are mutated by the moves later in this method.
        var descendants = new List<IContent>();
        const int pageSize = 500;
        var page = 0;
        var total = long.MaxValue;
        while (page * pageSize < total)
        {
            PagedModel<IContent> descendantsPage = await GetDescendantsAsync(content.Key, page++ * pageSize, pageSize, Ordering.By("Path"), cancellationToken);
            descendants.AddRange(descendantsPage.Items);
            total = descendantsPage.Total;
        }

        await PerformMoveContentLockedAsync(content, userId, trash, cancellationToken);

        var paths = new Dictionary<int, string>
        {
            [content.Id] = (parent == null
                ? parentId == Constants.System.RecycleBinContent ? "-1,-20" : Constants.System.RootString
                : parent.Path) + "," + content.Id,
        };

        // When restoring a single item out of the recycle bin without its descendants, the descendants must stay
        // trashed: the item's direct children are re-homed to the recycle bin root (and the rest of the subtree keeps
        // its relative structure), so nothing is orphaned and it can still be restored on its own later.
        var leaveDescendantsInRecycleBin = includeDescendants is false
            && parentId != Constants.System.RecycleBinContent
            && originalPath.Contains(Constants.System.RecycleBinContentString);

        foreach (IContent descendant in descendants)
        {
            moves.Add((descendant, descendant.Path));

            if (leaveDescendantsInRecycleBin)
            {
                await LeaveDescendantInRecycleBinLockedAsync(descendant, content.Id, originalLevel, userId, paths, cancellationToken);
            }
            else
            {
                await PerformMoveDescendantLockedAsync(descendant, levelDelta, userId, trash, paths, cancellationToken);
            }
        }
    }

    // Re-homes a descendant of a restored item within the recycle bin: the restored item's direct children become
    // top-level recycle bin items, while deeper descendants keep their relative structure below their (now re-homed)
    // ancestor. The trashed state is left untouched so these items remain in the recycle bin.
    private async Task LeaveDescendantInRecycleBinLockedAsync(IContent descendant, int restoredItemId, int originalLevel, int userId, Dictionary<int, string> paths, CancellationToken cancellationToken)
    {
        var isDirectChild = descendant.ParentId == restoredItemId;
        descendant.Path = paths[descendant.Id] = isDirectChild
            ? Constants.System.RecycleBinContentPathPrefix + descendant.Id
            : paths[descendant.ParentId] + "," + descendant.Id;
        descendant.Level -= originalLevel;
        if (isDirectChild)
        {
            descendant.ParentId = Constants.System.RecycleBinContent;
            descendant.ParentKey = Constants.System.RecycleBinContentKey;
        }

        await PerformMoveContentLockedAsync(descendant, userId, null, cancellationToken);
    }

    // Moves a descendant along with the item being moved, updating its path and level (parentId is unchanged).
    private async Task PerformMoveDescendantLockedAsync(IContent descendant, int levelDelta, int userId, bool? trash, Dictionary<int, string> paths, CancellationToken cancellationToken)
    {
        descendant.Path = paths[descendant.Id] = paths[descendant.ParentId] + "," + descendant.Id;
        descendant.Level += levelDelta;
        await PerformMoveContentLockedAsync(descendant, userId, trash, cancellationToken);
    }

    private async Task PerformMoveContentLockedAsync(IContent content, int userId, bool? trash, CancellationToken cancellationToken)
    {
        if (trash.HasValue)
        {
            ((ContentBase)content).Trashed = trash.Value;
        }

        content.WriterId = userId;
        await _documentRepository.SaveAsync(content, cancellationToken);
    }

    /// <summary>
    /// Empties the Recycle Bin by deleting all <see cref="IContent"/> items that reside in the bin asynchronously.
    /// </summary>
    /// <param name="userKey">The unique key of the user performing the operation.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An attempt carrying the operation status.</returns>
    public async Task<Attempt<ContentEmptyRecycleBinOperationStatus>> EmptyRecycleBinAsync(Guid userKey, CancellationToken cancellationToken)
    {
        var deleted = new List<IContent>();
        EventMessages eventMessages = EventMessagesFactory.Get();

        using ICoreScope scope = ScopeProvider.CreateCoreScope();
        scope.WriteLock(Constants.Locks.ContentTree);

        int intUserId = await _userIdKeyResolver.GetAsync(userKey);

        // emptying the recycle bin means deleting whatever is in there - do it properly!
        PagedModel<IContent> contentsPage = await GetChildrenAsync(Constants.System.RecycleBinContentKey, 0, int.MaxValue, propertyAliases: null, ordering: null, CancellationToken.None);
        IContent[] contents = contentsPage.Items.ToArray();

        var emptyingRecycleBinNotification = new ContentEmptyingRecycleBinNotification(contents, eventMessages);
        var deletingContentNotification = new ContentDeletingNotification(contents, eventMessages);
        if (await scope.Notifications.PublishCancelableAsync(emptyingRecycleBinNotification)
            || await scope.Notifications.PublishCancelableAsync(deletingContentNotification))
        {
            scope.Complete();
            return Attempt.Fail(ContentEmptyRecycleBinOperationStatus.CancelledByNotification);
        }

        // When checking if an item is related, we need to exclude the "relate parent on delete" relation type,
        // as this is automatically created when items are trashed and would prevent emptying the recycle bin.
        int[]? relateParentOnDeleteRelationTypeIds = null;
        if (_contentSettings.DisableDeleteWhenReferenced)
        {
            IRelationType? relateParentOnDeleteRelationType = await _relationService
                .GetRelationTypeByAliasAsync(Constants.Conventions.RelationTypes.RelateParentDocumentOnDeleteAlias);
            if (relateParentOnDeleteRelationType is not null)
            {
                relateParentOnDeleteRelationTypeIds = [relateParentOnDeleteRelationType.Id];
            }
        }

        foreach (IContent content in contents)
        {
            if (_contentSettings.DisableDeleteWhenReferenced
                && await _relationService.IsRelatedAsync(content.Id, RelationDirectionFilter.Child, excludeRelationTypeIds: relateParentOnDeleteRelationTypeIds))
            {
                continue;
            }

            await DeleteLockedAsync(scope, content, eventMessages, CancellationToken.None);
            deleted.Add(content);
        }

        scope.Notifications.Publish(
            new ContentEmptiedRecycleBinNotification(deleted, eventMessages).WithStateFrom(
                emptyingRecycleBinNotification));
        scope.Notifications.Publish(
            new ContentTreeChangeNotification(deleted, TreeChangeTypes.Remove, eventMessages));
        await AuditAsync(AuditType.Delete, intUserId, Constants.System.RecycleBinContent, "Recycle bin emptied");

        scope.Complete();

        return Attempt.Succeed(ContentEmptyRecycleBinOperationStatus.Success);
    }

    #endregion

    #region Others

    /// <inheritdoc />
    public async Task<Attempt<IContent?, ContentCopyOperationStatus>> CopyAsync(IContent content, Guid? parentKey, bool relateToOriginal, bool recursive, Guid userKey, CancellationToken cancellationToken)
    {
        EventMessages eventMessages = EventMessagesFactory.Get();

        // keep track of updates (copied item key and parent key) for the in-memory navigation structure
        var navigationUpdates = new List<Tuple<Guid, Guid?>>();

        using ICoreScope scope = ScopeProvider.CreateCoreScope();
        scope.WriteLock(Constants.Locks.ContentTree);

        IContent? parent = parentKey.HasValue ? await GetByIdAsync(parentKey.Value, cancellationToken) : null;
        if (parentKey.HasValue && parent is null)
        {
            scope.Complete();
            return Attempt.FailWithStatus<IContent?, ContentCopyOperationStatus>(ContentCopyOperationStatus.ParentNotFound, null);
        }

        int userId = await _userIdKeyResolver.GetAsync(userKey);

        IContent copy = content.DeepCloneWithResetIdentities();
        copy.ParentId = parent?.Id ?? Constants.System.Root;
        copy.ParentKey = parentKey;

        if (await scope.Notifications.PublishCancelableAsync(new ContentCopyingNotification(content, copy, parentKey, eventMessages)))
        {
            scope.Complete();
            return Attempt.FailWithStatus<IContent?, ContentCopyOperationStatus>(ContentCopyOperationStatus.CancelledByNotification, null);
        }

        // note - relateToOriginal is not managed here,
        // it's just part of the Copied event args so the RelateOnCopyHandler knows what to do
        // meaning that the event has to trigger for every copied content including descendants
        var copies = new List<Tuple<IContent, IContent>>();

        // a copy is not published (but not really unpublishing either)
        // update the create author and last edit author
        if (copy.Published)
        {
            copy.Published = false;
        }

        // clear any per-culture published state copied from the source - the copy is unpublished,
        // so no culture variations should be marked as published either (see #22540).
        copy.ClearPublishInfos();

        // a copy must not inherit the source's trashed state - it's being placed at a new location,
        // not restored from the recycle bin.
        if (copy.Trashed)
        {
            ((ContentBase)copy).Trashed = false;
        }

        copy.CreatorId = userId;
        copy.WriterId = userId;

        // get the current permissions, if there are any explicit ones they need to be copied
        EntityPermissionCollection currentPermissions = await GetPermissionsAsync(content.Key, cancellationToken);
        currentPermissions.RemoveWhere(p => p.IsDefaultPermissions);

        // save and flush because we need the ID for the recursive Copying events
        await _documentRepository.SaveAsync(copy, cancellationToken);

        // store navigation update information for copied item
        var copyHasRealParent = parentKey.HasValue && parentKey != Constants.System.RecycleBinContentKey;
        navigationUpdates.Add(Tuple.Create(copy.Key, copyHasRealParent ? copy.ParentKey : null));

        // add permissions
        if (currentPermissions.Count > 0)
        {
            var permissionSet = new ContentPermissionSet(copy, currentPermissions);
            await _documentRepository.AddOrUpdatePermissionsAsync(permissionSet, cancellationToken);
        }

        // keep track of copies
        copies.Add(Tuple.Create(content, copy));
        var idmap = new Dictionary<int, int> { [content.Id] = copy.Id };
        var copyIdToKeyMap = new Dictionary<int, Guid> { [copy.Id] = copy.Key };

        // process descendants
        if (recursive)
        {
            const int pageSize = 500;
            var page = 0;
            var total = long.MaxValue;
            while (page * pageSize < total)
            {
                PagedModel<IContent> descendantsPage = await GetDescendantsAsync(content.Key, page++ * pageSize, pageSize, ordering: null, cancellationToken);
                IEnumerable<IContent> descendants = descendantsPage.Items;
                total = descendantsPage.Total;
                foreach (IContent descendant in descendants)
                {
                    // when copying a branch into itself, the copy of a root would be seen as a descendant
                    // and would be copied again => filter it out.
                    if (descendant.Id == copy.Id)
                    {
                        continue;
                    }

                    // if parent has not been copied, skip, else gets its copy id
                    if (idmap.TryGetValue(descendant.ParentId, out int descendantParentId) == false)
                    {
                        continue;
                    }

                    IContent descendantCopy = descendant.DeepCloneWithResetIdentities();
                    descendantCopy.ParentId = descendantParentId;
                    descendantCopy.ParentKey = copyIdToKeyMap[descendantParentId];

                    if (await scope.Notifications.PublishCancelableAsync(new ContentCopyingNotification(descendant, descendantCopy, descendantCopy.ParentKey, eventMessages)))
                    {
                        continue;
                    }

                    // a copy is not published (but not really unpublishing either)
                    // update the create author and last edit author
                    if (descendantCopy.Published)
                    {
                        descendantCopy.Published = false;
                    }

                    // clear any per-culture published state copied from the source - the copy is unpublished,
                    // so no culture variations should be marked as published either (see #22540).
                    descendantCopy.ClearPublishInfos();

                    // a copy must not inherit the source's trashed state - it's being placed at a new
                    // location, not restored from the recycle bin.
                    if (descendantCopy.Trashed)
                    {
                        ((ContentBase)descendantCopy).Trashed = false;
                    }

                    descendantCopy.CreatorId = userId;
                    descendantCopy.WriterId = userId;

                    // since the repository relies on the dirty state to figure out whether it needs to update the sort order, we mark it dirty here
                    descendantCopy.SortOrder = descendantCopy.SortOrder;

                    // save and flush (see above)
                    await _documentRepository.SaveAsync(descendantCopy, cancellationToken);

                    // store navigation update information for descendants
                    navigationUpdates.Add(Tuple.Create(descendantCopy.Key, descendantCopy.ParentKey));

                    copies.Add(Tuple.Create(descendant, descendantCopy));
                    idmap[descendant.Id] = descendantCopy.Id;
                    copyIdToKeyMap[descendantCopy.Id] = descendantCopy.Key;
                }
            }
        }

        // not handling tags here, because
        // - tags should be handled by the content repository
        // - a copy is unpublished and therefore has no impact on tags in DB
        scope.Notifications.Publish(
            new ContentTreeChangeNotification(copy, TreeChangeTypes.RefreshBranch, eventMessages));
        foreach (Tuple<IContent, IContent> x in CollectionsMarshal.AsSpan(copies))
        {
            scope.Notifications.Publish(new ContentCopiedNotification(x.Item1, x.Item2, x.Item2.ParentKey, relateToOriginal, eventMessages));
        }

        await AuditAsync(AuditType.Copy, userId, content.Id);

        scope.Complete();
        return Attempt.SucceedWithStatus<IContent?, ContentCopyOperationStatus>(ContentCopyOperationStatus.Success, copy);
    }

    private bool TryGetParentKey(int parentId, [NotNullWhen(true)] out Guid? parentKey)
    {
        Attempt<Guid> parentKeyAttempt = _idKeyMap.GetKeyForIdAsync(parentId, UmbracoObjectTypes.Document).GetAwaiter().GetResult();
        parentKey = parentKeyAttempt.Success ? parentKeyAttempt.Result : null;
        return parentKeyAttempt.Success;
    }

    private Guid[] ResolveKeys(IEnumerable<int> ids) =>
        ids.Select(id => _idKeyMap.GetKeyForIdAsync(id, UmbracoObjectTypes.Document).GetAwaiter().GetResult())
            .Where(attempt => attempt.Success)
            .Select(attempt => attempt.Result)
            .ToArray();

    /// <inheritdoc />
    public async Task<Attempt<ContentSendToPublicationOperationStatus>> SendToPublicationAsync(IContent? content, Guid userKey, CancellationToken cancellationToken)
    {
        if (content is null)
        {
            return Attempt.Fail(ContentSendToPublicationOperationStatus.NotFound);
        }

        EventMessages evtMsgs = EventMessagesFactory.Get();

        using ICoreScope scope = ScopeProvider.CreateCoreScope();

        int userId = await _userIdKeyResolver.GetAsync(userKey);

        var sendingToPublishNotification = new ContentSendingToPublishNotification(content, evtMsgs);
        if (await scope.Notifications.PublishCancelableAsync(sendingToPublishNotification))
        {
            scope.Complete();
            return Attempt.Fail(ContentSendToPublicationOperationStatus.CancelledByNotification);
        }

        // track the cultures changing for auditing
        var culturesChanging = content.ContentType.VariesByCulture()
            ? string.Join(",", content.CultureInfos!.Values.Where(x => x.IsDirty()).Select(x => x.Culture))
            : null;

        // TODO: Currently there's no way to change track which variant properties have changed, we only have change
        // tracking enabled on all values on the Property which doesn't allow us to know which variants have changed.
        // in this particular case, determining which cultures have changed works with the above with names since it will
        // have always changed if it's been saved in the back office but that's not really fail safe.

        // Save before raising event
        Attempt<ContentSaveOperationStatus> saveResult = await SaveAsync(content, userKey, null, cancellationToken);

        // always complete (but maybe return a failed status)
        scope.Complete();

        if (!saveResult.Success)
        {
            return Attempt.Fail(ContentSendToPublicationOperationStatus.SaveFailed);
        }

        scope.Notifications.Publish(
            new ContentSentToPublishNotification(content, evtMsgs).WithStateFrom(sendingToPublishNotification));

        if (culturesChanging != null)
        {
            await AuditAsync(AuditType.SendToPublishVariant, userId, content.Id, $"Send To Publish for cultures: {culturesChanging}", culturesChanging);
        }
        else
        {
            await AuditAsync(AuditType.SendToPublish, userId, content.Id);
        }

        return Attempt.Succeed(ContentSendToPublicationOperationStatus.Success);
    }

    /// <inheritdoc />
    public async Task<Attempt<ContentSortOperationStatus>> SortAsync(IReadOnlyList<Guid> orderedKeys, Guid userKey, CancellationToken cancellationToken)
    {
        if (orderedKeys.Count == 0)
        {
            return Attempt.Fail(ContentSortOperationStatus.NoOperation);
        }

        EventMessages eventMessages = EventMessagesFactory.Get();

        using ICoreScope scope = ScopeProvider.CreateCoreScope();
        scope.WriteLock(Constants.Locks.ContentTree);

        // Reload within the lock so sorting operates on fully-loaded entities. Callers may pass
        // partially-loaded content (e.g. loaded with loadTemplates: false or without property data),
        // and saving those directly would wipe the template and property data (#23120).
        // GetByIdsAsync returns items in the requested order, preserving the caller's ordering that drives the sort.
        IContent[] itemsA = (await GetByIdsAsync(orderedKeys, cancellationToken)).ToArray();

        var sortingNotification = new ContentSortingNotification(itemsA, eventMessages);
        if (await scope.Notifications.PublishCancelableAsync(sortingNotification))
        {
            scope.Complete();
            return Attempt.Fail(ContentSortOperationStatus.CancelledByNotification);
        }

        var savingNotification = new ContentSavingNotification(itemsA, eventMessages);
        if (await scope.Notifications.PublishCancelableAsync(savingNotification))
        {
            scope.Complete();
            return Attempt.Fail(ContentSortOperationStatus.CancelledByNotification);
        }

        int userId = await _userIdKeyResolver.GetAsync(userKey);

        var published = new List<IContent>();
        var saved = new List<IContent>();
        var sortOrder = 0;

        foreach (IContent content in itemsA)
        {
            // if the current sort order equals that of the content we don't
            // need to update it, so just increment the sort order and continue.
            if (content.SortOrder == sortOrder)
            {
                sortOrder++;
                continue;
            }

            // else update
            content.SortOrder = sortOrder++;
            content.WriterId = userId;

            // if it's published, register it, no point running StrategyPublish
            // since we're not really publishing it and it cannot be cancelled etc
            if (content.Published)
            {
                published.Add(content);
            }

            // save
            saved.Add(content);
            await _documentRepository.SaveAsync(content, cancellationToken);
            await AuditAsync(AuditType.Sort, userId, content.Id, "Sorting content performed by user");
        }

        // first saved, then sorted
        scope.Notifications.Publish(
            new ContentSavedNotification(itemsA, eventMessages).WithStateFrom(savingNotification));
        scope.Notifications.Publish(
            new ContentSortedNotification(itemsA, eventMessages).WithStateFrom(sortingNotification));

        scope.Notifications.Publish(
            new ContentTreeChangeNotification(saved, TreeChangeTypes.RefreshNode, eventMessages));

        if (published.Count > 0)
        {
            scope.Notifications.Publish(new ContentPublishedNotification(published, eventMessages));
        }

        scope.Complete();
        return Attempt.Succeed(ContentSortOperationStatus.Success);
    }

    /// <inheritdoc />
    public async Task<Attempt<ContentSortChildrenOperationStatus>> SortChildrenAsync(Guid? parentKey, IReadOnlyList<Guid> orderedChildKeys, Guid userKey, CancellationToken cancellationToken)
    {
        EventMessages evtMsgs = EventMessagesFactory.Get();
        if (orderedChildKeys.Count == 0)
        {
            return Attempt.Fail(ContentSortChildrenOperationStatus.NoOperation);
        }

        using ICoreScope scope = ScopeProvider.CreateCoreScope();
        scope.WriteLock(Constants.Locks.ContentTree);

        await _documentRepository.UpdateSortOrderAsync(orderedChildKeys, cancellationToken);

        // Sort order lives in umbracoNode; neither the published cache nor the content repository cache keeps
        // a separate serialized copy of it, so refreshing the affected branch (which invalidates both and has
        // them reload from umbracoNode) is enough to pick up the new order without re-saving each child.
        int parentId;
        if (parentKey.HasValue)
        {
            IContent? parent = await GetByIdAsync(parentKey.Value, cancellationToken);
            parentId = parent?.Id ?? Constants.System.Root;
            if (parent is not null)
            {
                scope.Notifications.Publish(new ContentTreeChangeNotification(parent, TreeChangeTypes.RefreshBranch, evtMsgs));
            }
        }
        else
        {
            parentId = Constants.System.Root;
            IEnumerable<IContent> roots = await GetByIdsAsync(orderedChildKeys, cancellationToken);
            scope.Notifications.Publish(new ContentTreeChangeNotification(roots, TreeChangeTypes.RefreshNode, evtMsgs));
        }

        int userId = await _userIdKeyResolver.GetAsync(userKey);
        await AuditAsync(AuditType.Sort, userId, parentId);

        scope.Complete();
        return Attempt.Succeed(ContentSortChildrenOperationStatus.Success);
    }

    /// <inheritdoc />
    public override Task<ContentDataIntegrityReport> CheckDataIntegrityAsync(ContentDataIntegrityReportOptions options, CancellationToken cancellationToken)
        => CheckDataIntegrityAsync(
            options,
            scope =>
            {
                // The event args needs a content item so we'll make a fake one with enough properties to not cause a null ref
                var root = new Content("root", -1, new ContentType(_shortStringHelper, -1)) { Id = -1, Key = Guid.Empty };
                scope.Notifications.Publish(new ContentTreeChangeNotification(root, TreeChangeTypes.RefreshAll, EventMessagesFactory.Get()));
                return Task.CompletedTask;
            },
            cancellationToken);

    #endregion

    #region Internal Methods

    /// <summary>
    ///     Gets a collection of <see cref="IContent" /> descendants by the first Parent.
    /// </summary>
    /// <param name="content"><see cref="IContent" /> item to retrieve Descendants from</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An Enumerable list of <see cref="IContent" /> objects</returns>
    internal async Task<IReadOnlyCollection<IContent>> GetPublishedDescendantsAsync(IContent content, CancellationToken cancellationToken)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            scope.ReadLock(Constants.Locks.ContentTree);
            return await GetPublishedDescendantsLockedAsync(content, cancellationToken);
        }
    }

    /// <summary>
    /// Gets the published descendants of the specified content item while holding the content tree lock.
    /// </summary>
    /// <param name="content">The content item to retrieve published descendants from.</param>
    /// <returns>An enumerable of published <see cref="IContent"/> descendants.</returns>
    /// <remarks>
    /// This method should only be called within a scope that already holds the content tree read lock.
    /// The returned contents include all published versions below the content, but are filtered to exclude
    /// items that are not directly published because they are below an unpublished content.
    /// </remarks>
    #endregion

    #region Content Types

    /// <inheritdoc />
    public override async Task<Attempt<ContentDeleteOfTypesOperationStatus>> DeleteOfTypesAsync(IEnumerable<Guid> contentTypeKeys, Guid userKey, CancellationToken cancellationToken)
    {
        var changes = new List<TreeChange<IContent>>();
        var moves = new List<(IContent, string)>();
        Guid[] contentTypeKeysArray = contentTypeKeys.ToArray();
        EventMessages eventMessages = EventMessagesFactory.Get();

        using ICoreScope scope = ScopeProvider.CreateCoreScope();
        scope.WriteLock(Constants.Locks.ContentTree);

        var contents = new List<IContent>();
        const int pageSize = 500;
        var page = 0;
        var total = long.MaxValue;
        while (page * pageSize < total)
        {
            PagedModel<IContent> contentsPage = await GetPagedOfTypesAsync(contentTypeKeysArray, page++ * pageSize, pageSize, ordering: null, cancellationToken);
            contents.AddRange(contentsPage.Items);
            total = contentsPage.Total;
        }

        if (await scope.Notifications.PublishCancelableAsync(new ContentDeletingNotification(contents, eventMessages)))
        {
            scope.Complete();
            return Attempt.Fail(ContentDeleteOfTypesOperationStatus.CancelledByNotification);
        }

        int userId = await _userIdKeyResolver.GetAsync(userKey);

        // order by level, descending, so deepest first - that way, we cannot move
        // a content of the deleted type, to the recycle bin (and then delete it...)
        foreach (IContent content in contents.OrderByDescending(x => x.ParentId))
        {
            // if it's not trashed yet, and published, we should unpublish
            // but... Unpublishing event makes no sense (not going to cancel?) and no need to save
            // just raise the event
            if (content.Trashed == false && content.Published)
            {
                scope.Notifications.Publish(new ContentUnpublishedNotification(
                    content,
                    eventMessages,
                    BuildCultureMap(content, content.ContentType.VariesByCulture() ? content.PublishedCultures : ["*"])));
            }

            // if current content has children, move them to trash
            PagedModel<IContent> childrenPage = await GetChildrenAsync(content.Key, 0, int.MaxValue, propertyAliases: null, ordering: null, cancellationToken);
            foreach (IContent child in childrenPage.Items)
            {
                // see MoveToRecycleBinAsync
                await PerformMoveLockedAsync(child, Constants.System.RecycleBinContent, null, userId, moves, true, cancellationToken);
                changes.Add(new TreeChange<IContent>(content, TreeChangeTypes.RefreshBranch));
            }

            // delete content
            // triggers the deleted event (and handles the files)
            await DeleteLockedAsync(scope, content, eventMessages, cancellationToken);
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

        await AuditAsync(AuditType.Delete, userId, Constants.System.Root, $"Delete content of type {string.Join(",", contentTypeKeysArray)}");

        scope.Complete();
        return Attempt.Succeed(ContentDeleteOfTypesOperationStatus.Success);
    }

    /// <inheritdoc />
    public Task<Attempt<ContentDeleteOfTypesOperationStatus>> DeleteOfTypeAsync(Guid contentTypeKey, Guid userKey, CancellationToken cancellationToken) =>
        DeleteOfTypesAsync(new[] { contentTypeKey }, userKey, cancellationToken);

    #endregion

    #region Blueprints

    /// <inheritdoc />
    public async Task<IContent?> GetBlueprintByIdAsync(Guid key, CancellationToken cancellationToken)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope();
        scope.ReadLock(Constants.Locks.ContentTree);
        IContent? blueprint = await _documentBlueprintRepository.GetAsync(key, cancellationToken);
        if (blueprint is not null)
        {
            blueprint.Blueprint = true;
        }

        scope.Complete();
        return blueprint;
    }

    /// <inheritdoc />
    public async Task<Attempt<ContentBlueprintOperationStatus>> SaveBlueprintAsync(IContent content, IContent? createdFromContent, Guid userKey, CancellationToken cancellationToken)
    {
        EventMessages evtMsgs = EventMessagesFactory.Get();

        content.Blueprint = true;

        using ICoreScope scope = ScopeProvider.CreateCoreScope();
        scope.WriteLock(Constants.Locks.ContentTree);

        int userId = await _userIdKeyResolver.GetAsync(userKey);

        if (content.HasIdentity == false)
        {
            content.CreatorId = userId;
        }

        content.WriterId = userId;

        await _documentBlueprintRepository.SaveAsync(content, cancellationToken);

        await AuditAsync(AuditType.Save, userId, content.Id, $"Saved content template: {content.Name}");

        scope.Notifications.Publish(new ContentSavedBlueprintNotification(content, createdFromContent, evtMsgs));
        scope.Notifications.Publish(new ContentTreeChangeNotification(content, TreeChangeTypes.RefreshNode, evtMsgs));

        scope.Complete();

        return Attempt.Succeed(ContentBlueprintOperationStatus.Success);
    }

    /// <summary>
    /// Moves a content blueprint to a different container.
    /// </summary>
    /// <param name="content">The blueprint content to move.</param>
    /// <param name="userId">The optional ID of the user moving the blueprint.</param>
    public async Task<Attempt<ContentBlueprintOperationStatus>> MoveBlueprintAsync(IContent content, Guid userKey, CancellationToken cancellationToken)
    {
        EventMessages evtMsgs = EventMessagesFactory.Get();

        content.Blueprint = true;

        using ICoreScope scope = ScopeProvider.CreateCoreScope();
        scope.WriteLock(Constants.Locks.ContentTree);

        int userId = await _userIdKeyResolver.GetAsync(userKey);
        content.WriterId = userId;

        await _documentBlueprintRepository.SaveAsync(content, cancellationToken);

        await AuditAsync(AuditType.Move, userId, content.Id);

        scope.Notifications.Publish(new ContentTreeChangeNotification(content, TreeChangeTypes.RefreshNode, evtMsgs));

        scope.Complete();

        return Attempt.Succeed(ContentBlueprintOperationStatus.Success);
    }

    /// <summary>
    /// Deletes a content blueprint.
    /// </summary>
    /// <param name="content">The blueprint content to delete.</param>
    /// <param name="userId">The optional ID of the user deleting the blueprint.</param>
    public async Task<Attempt<ContentBlueprintOperationStatus>> DeleteBlueprintAsync(IContent content, Guid userKey, CancellationToken cancellationToken)
    {
        EventMessages evtMsgs = EventMessagesFactory.Get();

        using ICoreScope scope = ScopeProvider.CreateCoreScope();
        scope.WriteLock(Constants.Locks.ContentTree);

        await _documentBlueprintRepository.DeleteAsync(content, cancellationToken);

        scope.Notifications.Publish(new ContentDeletedBlueprintNotification(content, evtMsgs));
        scope.Notifications.Publish(new ContentTreeChangeNotification(content, TreeChangeTypes.Remove, evtMsgs));
        scope.Complete();

        return Attempt.Succeed(ContentBlueprintOperationStatus.Success);
    }

    private static readonly string?[] ArrayOfOneNullString = { null };

    /// <summary>
    /// Creates a new <see cref="IContent"/> from a blueprint.
    /// </summary>
    /// <param name="blueprint">The blueprint to create the content from.</param>
    /// <param name="name">The name for the new content.</param>
    /// <param name="userId">The optional ID of the user creating the content.</param>
    /// <returns>The newly created <see cref="IContent"/> based on the blueprint.</returns>
    public async Task<IContent> CreateBlueprintFromContentAsync(
        IContent blueprint,
        string name,
        Guid userKey,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(blueprint);

        using ICoreScope scope = ScopeProvider.CreateCoreScope();

        int userId = await _userIdKeyResolver.GetAsync(userKey);

        IContentType contentType = await GetContentTypeAsync(scope, blueprint.ContentType.Alias, cancellationToken);
        var content = new Content(name, -1, contentType);
        content.Path = string.Concat(content.ParentId.ToString(), ",", content.Id);

        content.CreatorId = userId;
        content.WriterId = userId;

        IEnumerable<string?> cultures = ArrayOfOneNullString;
        if (blueprint.CultureInfos?.Count > 0)
        {
            cultures = blueprint.CultureInfos.Values.Select(x => x.Culture);

            if (blueprint.CultureInfos.TryGetValue(await _languageRepository.GetDefaultIsoCodeAsync(), out ContentCultureInfos defaultCulture))
            {
                defaultCulture.Name = name;
            }
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

        scope.Complete();

        return content;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<IContent>> GetBlueprintsForContentTypesAsync(CancellationToken cancellationToken, params Guid[] contentTypeKeys)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope();
        scope.ReadLock(Constants.Locks.ContentTree);

        IEnumerable<IContent> blueprints;
        if (contentTypeKeys.Length == 0)
        {
            blueprints = await _documentBlueprintRepository.GetAllAsync(cancellationToken);
        }
        else
        {
            PagedModel<IContent> paged = await _documentBlueprintRepository.GetPagedOfContentTypesAsync(
                contentTypeKeys, 0, int.MaxValue, Ordering.By("sortOrder"), cancellationToken);
            blueprints = paged.Items;
        }

        foreach (IContent blueprint in blueprints)
        {
            blueprint.Blueprint = true;
        }

        scope.Complete();
        return blueprints;
    }

    /// <inheritdoc />
    public Task<Attempt<ContentBlueprintOperationStatus>> DeleteBlueprintsOfTypeAsync(Guid contentTypeKey, Guid userKey, CancellationToken cancellationToken) =>
        DeleteBlueprintsOfTypesAsync(new[] { contentTypeKey }, userKey, cancellationToken);

    /// <inheritdoc />
    public async Task<Attempt<ContentBlueprintOperationStatus>> DeleteBlueprintsOfTypesAsync(IEnumerable<Guid> contentTypeKeys, Guid userKey, CancellationToken cancellationToken)
    {
        EventMessages evtMsgs = EventMessagesFactory.Get();

        using ICoreScope scope = ScopeProvider.CreateCoreScope();
        scope.WriteLock(Constants.Locks.ContentTree);

        Guid[] contentTypeKeysArray = contentTypeKeys.ToArray();

        IEnumerable<IContent> blueprints;
        if (contentTypeKeysArray.Length == 0)
        {
            blueprints = await _documentBlueprintRepository.GetAllAsync(cancellationToken);
        }
        else
        {
            PagedModel<IContent> paged = await _documentBlueprintRepository.GetPagedOfContentTypesAsync(
                contentTypeKeysArray, 0, int.MaxValue, Ordering.By("sortOrder"), cancellationToken);
            blueprints = paged.Items;
        }

        IContent[] blueprintsArray = blueprints.ToArray();

        if (blueprintsArray.Length > 0)
        {
            foreach (IContent blueprint in blueprintsArray)
            {
                blueprint.Blueprint = true;
                await _documentBlueprintRepository.DeleteAsync(blueprint, cancellationToken);
            }

            scope.Notifications.Publish(new ContentDeletedBlueprintNotification(blueprintsArray, evtMsgs));
            scope.Notifications.Publish(new ContentTreeChangeNotification(blueprintsArray, TreeChangeTypes.Remove, evtMsgs));
        }

        scope.Complete();
        return Attempt.Succeed(ContentBlueprintOperationStatus.Success);
    }

    #endregion

    #region Abstract implementations

    protected override UmbracoObjectTypes ContentObjectType => UmbracoObjectTypes.Document;

    protected override int[] ReadLockIds => WriteLockIds;

    protected override int[] WriteLockIds => new[] { Constants.Locks.ContentTree };

    protected override bool SupportsBranchPublishing => true;

    protected override ILogger<ContentService> Logger => _logger;

    /// <inheritdoc cref="AsyncPublishableContentServiceBase{TContent}.DeleteLockedAsync" />
    protected override async Task DeleteLockedAsync(ICoreScope scope, IContent content, EventMessages evtMsgs, CancellationToken cancellationToken)
    {
        async Task DoDeleteAsync(IContent c)
        {
            await _documentRepository.DeleteAsync(c, cancellationToken);
            scope.Notifications.Publish(new ContentDeletedNotification(c, evtMsgs));

            // media files deleted by QueuingEventDispatcher
        }

        const int pageSize = 500;
        var total = long.MaxValue;
        while (total > 0)
        {
            // get descendants - ordered from deepest to shallowest
            PagedModel<IContent> descendantsPage = await GetDescendantsAsync(content.Key, 0, pageSize, Ordering.By("Path", Direction.Descending), cancellationToken);
            total = descendantsPage.Total;
            foreach (IContent c in descendantsPage.Items)
            {
                await DoDeleteAsync(c);
            }
        }

        await DoDeleteAsync(content);
    }

    protected override SavingNotification<IContent> SavingNotification(IContent content, EventMessages eventMessages)
        => new ContentSavingNotification(content, eventMessages);

    protected override SavedNotification<IContent> SavedNotification(IContent content, EventMessages eventMessages)
        => new ContentSavedNotification(content, eventMessages);

    protected override SavedNotification<IContent> SavedNotification(IContent content, EventMessages eventMessages, IReadOnlyDictionary<Guid, IReadOnlyCollection<string>>? savedCultures)
        => new ContentSavedNotification(content, eventMessages, savedCultures);

    protected override SavingNotification<IContent> SavingNotification(IEnumerable<IContent> content, EventMessages eventMessages)
        => new ContentSavingNotification(content, eventMessages);

    protected override SavedNotification<IContent> SavedNotification(IEnumerable<IContent> content, EventMessages eventMessages)
        => new ContentSavedNotification(content, eventMessages);

    protected override SavedNotification<IContent> SavedNotification(IEnumerable<IContent> content, EventMessages eventMessages, IReadOnlyDictionary<Guid, IReadOnlyCollection<string>>? savedCultures)
        => new ContentSavedNotification(content, eventMessages, savedCultures);

    protected override TreeChangeNotification<IContent> TreeChangeNotification(IContent content, TreeChangeTypes changeTypes, EventMessages eventMessages)
        => new ContentTreeChangeNotification(content, changeTypes, eventMessages);

    protected override TreeChangeNotification<IContent> TreeChangeNotification(IContent content, TreeChangeTypes changeTypes, IEnumerable<string>? publishedCultures, IEnumerable<string>? unpublishedCultures, EventMessages eventMessages)
        => new ContentTreeChangeNotification(content, changeTypes, publishedCultures, unpublishedCultures, eventMessages);

    protected override TreeChangeNotification<IContent> TreeChangeNotification(IEnumerable<IContent> content, TreeChangeTypes changeTypes, EventMessages eventMessages)
        => new ContentTreeChangeNotification(content, changeTypes, eventMessages);

    protected override DeletingNotification<IContent> DeletingNotification(IContent content, EventMessages eventMessages)
        => new ContentDeletingNotification(content, eventMessages);

    protected override CancelableEnumerableObjectNotification<IContent> PublishingNotification(IContent content, EventMessages eventMessages)
        => new ContentPublishingNotification(content, eventMessages);

    protected override IStatefulNotification PublishedNotification(IContent content, EventMessages eventMessages)
        => new ContentPublishedNotification(content, eventMessages);

    protected override IStatefulNotification PublishedNotification(IContent content, EventMessages eventMessages, IReadOnlyDictionary<Guid, IReadOnlyCollection<string>>? publishedCultures, IReadOnlyDictionary<Guid, IReadOnlyCollection<string>>? unpublishedCultures)
        => new ContentPublishedNotification(content, eventMessages, publishedCultures, unpublishedCultures);

    protected override IStatefulNotification PublishedNotification(IEnumerable<IContent> content, EventMessages eventMessages)
        => new ContentPublishedNotification(content, eventMessages);

    protected override CancelableEnumerableObjectNotification<IContent> UnpublishingNotification(IContent content, EventMessages eventMessages)
        => new ContentUnpublishingNotification(content, eventMessages);

    protected override IStatefulNotification UnpublishedNotification(IContent content, EventMessages eventMessages)
        => new ContentUnpublishedNotification(content, eventMessages);

    protected override IStatefulNotification UnpublishedNotification(IContent content, EventMessages eventMessages, IReadOnlyDictionary<Guid, IReadOnlyCollection<string>>? unpublishedCultures)
        => new ContentUnpublishedNotification(content, eventMessages, unpublishedCultures);

    protected override RollingBackNotification<IContent> RollingBackNotification(IContent target, EventMessages messages)
        => new ContentRollingBackNotification(target, messages);

    protected override RolledBackNotification<IContent> RolledBackNotification(IContent target, EventMessages messages)
        => new ContentRolledBackNotification(target, messages);

    #endregion
}
