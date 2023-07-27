using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Exceptions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services.Changes;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Services;

// FIXME: add granular permissions check (for inspiration, check how the old ContentController utilizes IAuthorizationService)
internal sealed class ContentEditingService
    : ContentEditingServiceBase<IContent, IContentType, IContentService, IContentTypeService>, IContentEditingService
{
    private readonly ITemplateService _templateService;
    private readonly ILogger<ContentEditingService> _logger;
    private readonly IUserIdKeyResolver _userIdKeyResolver;
    private readonly ILanguageService _languageService;
    private readonly ICultureImpactFactory _cultureImpactFactory;
    private readonly IDocumentRepository _documentRepository;
    private readonly IEventMessagesFactory _eventMessagesFactory;
    private readonly IAuditRepository _auditRepository;

    public ContentEditingService(
        IContentService contentService,
        IContentTypeService contentTypeService,
        PropertyEditorCollection propertyEditorCollection,
        IDataTypeService dataTypeService,
        ITemplateService templateService,
        ILogger<ContentEditingService> logger,
        ICoreScopeProvider scopeProvider,
        IUserIdKeyResolver userIdKeyResolver,
        ILanguageService languageService,
        ICultureImpactFactory cultureImpactFactory,
        IDocumentRepository documentRepository,
        IEventMessagesFactory eventMessagesFactory,
        IAuditRepository auditRepository)
        : base(contentService, contentTypeService, propertyEditorCollection, dataTypeService, logger, scopeProvider)
    {
        _templateService = templateService;
        _logger = logger;
        _userIdKeyResolver = userIdKeyResolver;
        _languageService = languageService;
        _cultureImpactFactory = cultureImpactFactory;
        _documentRepository = documentRepository;
        _eventMessagesFactory = eventMessagesFactory;
        _auditRepository = auditRepository;
    }

    public async Task<IContent?> GetAsync(Guid id)
    {
        IContent? content = ContentService.GetById(id);
        return await Task.FromResult(content);
    }

    public async Task<Attempt<IContent?, ContentEditingOperationStatus>> CreateAsync(ContentCreateModel createModel,
        Guid userKey)
    {
        Attempt<IContent?, ContentEditingOperationStatus> result = await MapCreate(createModel);
        if (result.Success == false)
        {
            return result;
        }

        IContent content = result.Result!;
        ContentEditingOperationStatus operationStatus = await UpdateTemplateAsync(content, createModel.TemplateKey);
        if (operationStatus != ContentEditingOperationStatus.Success)
        {
            return Attempt.FailWithStatus<IContent?, ContentEditingOperationStatus>(operationStatus, content);
        }

        operationStatus = await Save(content, userKey);
        return operationStatus == ContentEditingOperationStatus.Success
            ? Attempt.SucceedWithStatus<IContent?, ContentEditingOperationStatus>(ContentEditingOperationStatus.Success,
                content)
            : Attempt.FailWithStatus<IContent?, ContentEditingOperationStatus>(operationStatus, content);
    }

    public async Task<Attempt<IContent, ContentEditingOperationStatus>> UpdateAsync(IContent content,
        ContentUpdateModel updateModel, Guid userKey)
    {
        Attempt<ContentEditingOperationStatus> result = await MapUpdate(content, updateModel);
        if (result.Success == false)
        {
            return Attempt.FailWithStatus(result.Result, content);
        }

        ContentEditingOperationStatus operationStatus = await UpdateTemplateAsync(content, updateModel.TemplateKey);
        if (operationStatus != ContentEditingOperationStatus.Success)
        {
            return Attempt.FailWithStatus(operationStatus, content);
        }

        operationStatus = await Save(content, userKey);
        return operationStatus == ContentEditingOperationStatus.Success
            ? Attempt.SucceedWithStatus(ContentEditingOperationStatus.Success, content)
            : Attempt.FailWithStatus(operationStatus, content);
    }

    public async Task<Attempt<IContent?, ContentEditingOperationStatus>> MoveToRecycleBinAsync(Guid id, Guid userKey)
    {
        var currentUserId = await GetUserIdAsync(userKey);
        return await HandleDeletionAsync(id, content => ContentService.MoveToRecycleBin(content, currentUserId), false);
    }

    public async Task<Attempt<IContent?, ContentEditingOperationStatus>> DeleteAsync(Guid id, Guid userKey)
    {
        var currentUserId = await GetUserIdAsync(userKey);
        return await HandleDeletionAsync(id, content => ContentService.Delete(content, currentUserId), false);
    }

    public async Task<Attempt<IContent?, ContentEditingOperationStatus>> MoveAsync(Guid id, Guid? parentId,
        Guid userKey)
    {
        var currentUserId = await GetUserIdAsync(userKey);
        return await HandleMoveAsync(id, parentId,
            (content, newParentId) => ContentService.Move(content, newParentId, currentUserId));
    }

    public async Task<Attempt<IContent?, ContentEditingOperationStatus>> CopyAsync(Guid id, Guid? parentId,
        bool relateToOriginal, bool includeDescendants, Guid userKey)
    {
        var currentUserId = await GetUserIdAsync(userKey);
        return await HandleCopyAsync(id, parentId,
            (content, newParentId) =>
                ContentService.Copy(content, newParentId, relateToOriginal, includeDescendants, currentUserId));
    }

    public async Task<Attempt<ContentPublishingOperationStatus>> PublishAsync(Guid id, Guid userKey, string culture = "*")
    {
        IContent? foundContent = ContentService.GetById(id);

        if (foundContent is null)
        {
            return Attempt.Fail(ContentPublishingOperationStatus.ContentNotFound);
        }

        // cannot accept invariant (null or empty) culture for variant content type
        // cannot accept a specific culture for invariant content type (but '*' is ok)
        if (foundContent.ContentType.VariesByCulture())
        {
            if (culture.IsNullOrWhiteSpace())
            {
                throw new NotSupportedException("Invariant culture is not supported by variant content types.");
            }
        }
        else
        {
            if (!culture.IsNullOrWhiteSpace() && culture != "*")
            {
                throw new NotSupportedException(
                    $"Culture \"{culture}\" is not supported by invariant content types.");
            }
        }

        using ICoreScope scope = CoreScopeProvider.CreateCoreScope();
        scope.WriteLock(Constants.Locks.ContentTree);

        IEnumerable<ILanguage> allLangs = await _languageService.GetAllAsync();

        // Change state to publishing
        foundContent.PublishedState = PublishedState.Publishing;

        // if culture is specific, first publish the invariant values, then publish the culture itself.
        // if culture is '*', then publish them all (including variants)

        // this will create the correct culture impact even if culture is * or null
        CultureImpact? impact =
            _cultureImpactFactory.Create(culture, IsDefaultCulture(allLangs, culture), foundContent);

        // publish the culture(s)
        // we don't care about the response here, this response will be rechecked below but we need to set the culture info values now.
        foundContent.PublishCulture(impact);

        ContentPublishingOperationStatus contentPublishingOperationStatus = Publish(foundContent, allLangs, userKey, scope);

        scope.Complete();

        if (contentPublishingOperationStatus is ContentPublishingOperationStatus.Success or ContentPublishingOperationStatus.SuccessPublishCulture)
        {
            return Attempt.Succeed(contentPublishingOperationStatus);
        }

        return Attempt.Fail(contentPublishingOperationStatus);
    }

    public async Task<Attempt<ContentPublishingOperationStatus>> PublishAsync(Guid id, Guid userKey, string[] cultures)
    {
        IContent? content = ContentService.GetById(id);

        if (content is null)
        {
            return Attempt.Fail(ContentPublishingOperationStatus.ContentNotFound);
        }

        if (content.Name != null && content.Name.Length > 255)
        {
            throw new InvalidOperationException("Name cannot be more than 255 characters in length.");
        }

        using ICoreScope scope = CoreScopeProvider.CreateCoreScope();
        scope.WriteLock(Constants.Locks.ContentTree);

        IEnumerable<ILanguage> allLangs = await _languageService.GetAllAsync();

        var varies = content.ContentType.VariesByCulture();

        if (cultures.Length == 0 && !varies)
        {
            // No cultures specified and doesn't vary, so publish it, else nothing to publish
            return await PublishAsync(id, userKey);
        }

        if (cultures.Any(x => x == null || x == "*"))
        {
            throw new InvalidOperationException(
                "Only valid cultures are allowed to be used in this method, wildcards or nulls are not allowed");
        }

        IEnumerable<CultureImpact> impacts =
            cultures.Select(x => _cultureImpactFactory.ImpactExplicit(x, IsDefaultCulture(allLangs, x)));

        // publish the culture(s)
        // we don't care about the response here, this response will be rechecked below but we need to set the culture info values now.
        foreach (CultureImpact impact in impacts)
        {
            content.PublishCulture(impact);
        }

        ContentPublishingOperationStatus contentPublishingOperationStatus = Publish(content, allLangs, userKey, scope);

        scope.Complete();

        if (contentPublishingOperationStatus is ContentPublishingOperationStatus.Success or ContentPublishingOperationStatus.SuccessPublishCulture)
        {
            Attempt.Succeed(contentPublishingOperationStatus);
        }

        return Attempt.Fail(contentPublishingOperationStatus);
    }

    protected override IContent Create(string? name, int parentId, IContentType contentType) =>
        new Content(name, parentId, contentType);

    private bool IsDefaultCulture(IEnumerable<ILanguage>? langs, string culture) =>
        langs?.Any(x => x.IsDefault && x.IsoCode.InvariantEquals(culture)) ?? false;

    private ContentPublishingOperationStatus Publish(IContent content, IEnumerable<ILanguage> languages, Guid userKey, ICoreScope scope, bool branchOne = false, bool branchRoot = false)
    {
        var userId = -1;
        PublishResult? unpublishResult = null;
        EventMessages eventMessages = _eventMessagesFactory.Get();
        var isNew = !content.HasIdentity;
        var previouslyPublished = content.HasIdentity && content.Published;
        TreeChangeTypes changeType = isNew ? TreeChangeTypes.RefreshNode : TreeChangeTypes.RefreshBranch;
        var variesByCulture = content.ContentType.VariesByCulture();

        List<string>? culturesPublishing = variesByCulture
            ? content.PublishCultureInfos?.Values.Where(x => x.IsDirty()).Select(x => x.Culture).ToList()
            : null;

        IReadOnlyList<string>? culturesChanging = variesByCulture
            ? content.CultureInfos?.Values.Where(x => x.IsDirty()).Select(x => x.Culture).ToList()
            : null;

        // ensure that the document can be published, and publish handling events, business rules, etc
        ContentPublishingOperationStatus publishOperationStatus = StrategyCanPublish(
            scope,
            content, /*checkPath:*/
            !branchOne || branchRoot,
            culturesPublishing,
            languages,
            eventMessages);
        if (publishOperationStatus is ContentPublishingOperationStatus.Success)
        {
            // note: StrategyPublish flips the PublishedState to Publishing!
            publishOperationStatus = StrategyPublish(content, culturesPublishing, eventMessages);
        }
        else
        {
            // in a branch, just give up
            if (branchOne && !branchRoot)
            {
                return publishOperationStatus;
            }

            // // Check for mandatory culture missing, and then unpublish document as a whole
            // if (publishResult.Result == PublishResultType.FailedPublishMandatoryCultureMissing)
            // {
            //     publishing = false;
            //     unpublishing = content.Published; // if not published yet, nothing to do
            //
            //     // we may end up in a state where we won't publish nor unpublish
            //     // keep going, though, as we want to save anyways
            // }

            // reset published state from temp values (publishing, unpublishing) to original value
            // (published, unpublished) in order to save the document, unchanged - yes, this is odd,
            // but: (a) it means we don't reproduce the PublishState logic here and (b) setting the
            // PublishState to anything other than Publishing or Unpublishing - which is precisely
            // what we want to do here - throws
            content.Published = content.Published;
        }

        _documentRepository.Save(content);

        // raise the Saved event, always
        scope.Notifications.Publish(
            new ContentSavedNotification(content, eventMessages));

        // and succeeded, trigger events
        if (publishOperationStatus is not ContentPublishingOperationStatus.Success)
        {
            if (isNew == false && previouslyPublished == false)
            {
                changeType = TreeChangeTypes.RefreshBranch; // whole branch
            }
            else if (isNew == false && previouslyPublished)
            {
                changeType = TreeChangeTypes.RefreshNode; // single node
            }

            // invalidate the node/branch
            // for branches, handled by SaveAndPublishBranch
            if (!branchOne)
            {
                scope.Notifications.Publish(
                    new ContentTreeChangeNotification(content, changeType, eventMessages));
                scope.Notifications.Publish(
                    new ContentPublishedNotification(content, eventMessages));
            }

            // it was not published and now is... descendants that were 'published' (but
            // had an unpublished ancestor) are 're-published' ie not explicitly published
            // but back as 'published' nevertheless
            if (!branchOne && isNew == false && previouslyPublished == false && HasChildren(content.Id))
            {
                IContent[] descendants = GetPublishedDescendantsLocked(content).ToArray();
                scope.Notifications.Publish(
                    new ContentPublishedNotification(descendants, eventMessages));
            }

            switch (publishOperationStatus)
            {
                case ContentPublishingOperationStatus.Success:
                    Audit(AuditType.Publish, userId, content.Id);
                    break;
                case ContentPublishingOperationStatus.SuccessPublishCulture:
                    if (culturesPublishing != null)
                    {
                        var langs = string.Join(", ", languages
                            .Where(x => culturesPublishing.InvariantContains(x.IsoCode))
                            .Select(x => x.CultureName));
                        Audit(AuditType.PublishVariant, userId, content.Id, $"Published languages: {langs}", langs);
                    }

                    break;
            }

            return publishOperationStatus;
        }

        // should not happen
        if (branchOne && !branchRoot)
        {
            throw new PanicException("branchOne && !branchRoot - should not happen");
        }

        // if publishing didn't happen or if it has failed, we still need to log which cultures were saved
        if (!branchOne && (publishOperationStatus is not ContentPublishingOperationStatus.Success || publishOperationStatus is not ContentPublishingOperationStatus.SuccessPublishCulture))
        {
            if (culturesChanging != null)
            {
                var langs = string.Join(", ", languages
                    .Where(x => culturesChanging.InvariantContains(x.IsoCode))
                    .Select(x => x.CultureName));
                Audit(AuditType.SaveVariant, userId, content.Id, $"Saved languages: {langs}", langs);
            }
            else
            {
                Audit(AuditType.Save, userId, content.Id);
            }
        }

        // or, failed
        scope.Notifications.Publish(new ContentTreeChangeNotification(content, changeType, eventMessages));

        return publishOperationStatus!;
    }

    internal IEnumerable<IContent> GetPublishedDescendantsLocked(IContent content)
    {
        var pathMatch = content.Path + ",";
        IQuery<IContent> query = CoreScopeProvider.CreateQuery<IContent>()
            .Where(x => x.Id != content.Id && x.Path.StartsWith(pathMatch) /*&& x.Trashed == false*/);
        IEnumerable<IContent> contents = _documentRepository.Get(query);

        // beware! contents contains all published version below content
        // including those that are not directly published because below an unpublished content
        // these must be filtered out here
        var parents = new List<int> {content.Id};
        if (contents is not null)
        {
            foreach (IContent c in contents)
            {
                if (parents.Contains(c.ParentId))
                {
                    yield return c;
                    parents.Add(c.Id);
                }
            }
        }
    }

    private bool HasChildren(int id) => CountChildren(id) > 0;

    private int CountChildren(int parentId, string? contentTypeAlias = null)
    {
        using (ICoreScope scope = CoreScopeProvider.CreateCoreScope(autoComplete: true))
        {
            scope.ReadLock(Constants.Locks.ContentTree);
            return _documentRepository.CountChildren(parentId, contentTypeAlias);
        }
    }

    private ContentPublishingOperationStatus StrategyPublish(
        IContent content,
        IReadOnlyCollection<string>? culturesPublishing,
        EventMessages eventMessages)
    {
        // change state to publishing
        content.PublishedState = PublishedState.Publishing;

        // if this is a variant then we need to log which cultures have been published/unpublished and return an appropriate result
        if (content.ContentType.VariesByCulture())
        {
            if (content.Published && culturesPublishing?.Count == 0)
            {
                return ContentPublishingOperationStatus.FailedNothingToPublish;
            }

            if (culturesPublishing?.Count > 0)
            {
                _logger.LogInformation(
                    "Document {ContentName} (id={ContentId}) cultures: {Cultures} have been published.",
                    content.Name,
                    content.Id,
                    string.Join(",", culturesPublishing));
            }

            return ContentPublishingOperationStatus.SuccessPublishCulture;
        }

        _logger.LogInformation("Document {ContentName} (id={ContentId}) has been published.", content.Name, content.Id);
        return ContentPublishingOperationStatus.Success;
    }

    private ContentPublishingOperationStatus StrategyCanPublish(
        ICoreScope scope,
        IContent content,
        bool checkPath,
        IEnumerable<string>? culturesPublishing,
        IEnumerable<ILanguage> allLangs,
        EventMessages eventMessages)
    {
        // raise Publishing notification
        if (scope.Notifications.PublishCancelable(
                new ContentPublishingNotification(content, eventMessages)))
        {
            _logger.LogInformation("Document {ContentName} (id={ContentId}) cannot be published: {Reason}", content.Name, content.Id, "publishing was cancelled");
            return ContentPublishingOperationStatus.FailedCancelledByEvent;
        }

        var variesByCulture = content.ContentType.VariesByCulture();

        // If it's null it's invariant
        CultureImpact[] impactsToPublish = culturesPublishing == null
            ? new[] {_cultureImpactFactory.ImpactInvariant()}
            : culturesPublishing.Select(x =>
                    _cultureImpactFactory.ImpactExplicit(
                        x,
                        allLangs.Any(lang => lang.IsoCode.InvariantEquals(x) && lang.IsMandatory)))
                .ToArray();

        // publish the culture(s)
        if (!impactsToPublish.All(content.PublishCulture))
        {
            return ContentPublishingOperationStatus.FailedContentInvalid;
        }

        // Check if mandatory languages fails, if this fails it will mean anything that the published flag on the document will
        // be changed to Unpublished and any culture currently published will not be visible.
        if (variesByCulture)
        {
            if (culturesPublishing == null)
            {
                throw new InvalidOperationException(
                    "Internal error, variesByCulture but culturesPublishing is null.");
            }

            if (content.Published && culturesPublishing.Any() is false)
            {
                // no published cultures = cannot be published
                // there will be nothing to publish
                return ContentPublishingOperationStatus.FailedContentInvalid;
            }

            // missing mandatory culture = cannot be published
            IEnumerable<string> mandatoryCultures = allLangs.Where(x => x.IsMandatory).Select(x => x.IsoCode);
            var mandatoryMissing = mandatoryCultures.Any(x =>
                !content.PublishedCultures.Contains(x, StringComparer.OrdinalIgnoreCase));
            if (mandatoryMissing)
            {
                return ContentPublishingOperationStatus.FailedMandatoryCultureMissing;
            }
        }

        // ensure that the document has published values
        // either because it is 'publishing' or because it already has a published version
        if (content.PublishedState != PublishedState.Publishing && content.PublishedVersionId == 0)
        {
            _logger.LogInformation(
                "Document {ContentName} (id={ContentId}) cannot be published: {Reason}",
                content.Name,
                content.Id,
                "document does not have published values");
            return ContentPublishingOperationStatus.FailedNothingToPublish;
        }

        ContentScheduleCollection contentSchedule = _documentRepository.GetContentSchedule(content.Id);

        // loop over each culture publishing - or string.Empty for invariant
        foreach (var culture in culturesPublishing ?? new[] {string.Empty})
        {
            // ensure that the document status is correct
            // note: culture will be string.Empty for invariant
            switch (content.GetStatus(contentSchedule, culture))
            {
                case ContentStatus.Expired:
                    if (!variesByCulture)
                    {
                        _logger.LogInformation(
                            "Document {ContentName} (id={ContentId}) cannot be published: {Reason}", content.Name, content.Id, "document has expired");
                    }
                    else
                    {
                        _logger.LogInformation(
                            "Document {ContentName} (id={ContentId}) culture {Culture} cannot be published: {Reason}", content.Name, content.Id, culture, "document culture has expired");
                    }

                    return !variesByCulture
                        ? ContentPublishingOperationStatus.FailedHasExpired
                        : ContentPublishingOperationStatus.FailedCultureHasExpired;

                case ContentStatus.AwaitingRelease:
                    if (!variesByCulture)
                    {
                        _logger.LogInformation(
                            "Document {ContentName} (id={ContentId}) cannot be published: {Reason}",
                            content.Name,
                            content.Id,
                            "document is awaiting release");
                    }
                    else
                    {
                        _logger.LogInformation(
                            "Document {ContentName} (id={ContentId}) culture {Culture} cannot be published: {Reason}",
                            content.Name,
                            content.Id,
                            culture,
                            "document is culture awaiting release");
                    }

                    return !variesByCulture
                        ? ContentPublishingOperationStatus.FailedAwaitingRelease
                        : ContentPublishingOperationStatus.FailedCultureAwaitingRelease;

                case ContentStatus.Trashed:
                    _logger.LogInformation(
                        "Document {ContentName} (id={ContentId}) cannot be published: {Reason}",
                        content.Name,
                        content.Id,
                        "document is trashed");
                    return ContentPublishingOperationStatus.FailedIsTrashed;
            }
        }

        if (checkPath)
        {
            // check if the content can be path-published
            // root content can be published
            // else check ancestors - we know we are not trashed
            var pathIsOk = content.ParentId == Constants.System.Root || IsPathPublished(GetParent(content));
            if (!pathIsOk)
            {
                _logger.LogInformation(
                    "Document {ContentName} (id={ContentId}) cannot be published: {Reason}",
                    content.Name,
                    content.Id,
                    "parent is not published");
                return ContentPublishingOperationStatus.FailedPathNotPublished;
            }
        }

        return ContentPublishingOperationStatus.Success;
    }

    public IContent? GetParent(IContent? content)
    {
        if (content?.ParentId == Constants.System.Root || content?.ParentId == Constants.System.RecycleBinContent ||
            content is null)
        {
            return null;
        }

        return GetById(content.ParentId);
    }

    public IContent? GetById(int id)
    {
        using (ICoreScope scope = CoreScopeProvider.CreateCoreScope(autoComplete: true))
        {
            scope.ReadLock(Constants.Locks.ContentTree);
            return _documentRepository.Get(id);
        }
    }

    private bool IsPathPublished(IContent? content)
    {
        using (ICoreScope scope = CoreScopeProvider.CreateCoreScope(autoComplete: true))
        {
            scope.ReadLock(Constants.Locks.ContentTree);
            return _documentRepository.IsPathPublished(content);
        }
    }

    private async Task<ContentEditingOperationStatus> UpdateTemplateAsync(IContent content, Guid? templateKey)
    {
        if (templateKey == null)
        {
            content.TemplateId = null;
            return ContentEditingOperationStatus.Success;
        }

        ITemplate? template = await _templateService.GetAsync(templateKey.Value);
        if (template == null)
        {
            return ContentEditingOperationStatus.TemplateNotFound;
        }

        IContentType contentType = ContentTypeService.Get(content.ContentTypeId)
                                   ?? throw new ArgumentException("The content type was not found", nameof(content));
        if (contentType.IsAllowedTemplate(template.Alias) == false)
        {
            return ContentEditingOperationStatus.TemplateNotAllowed;
        }

        content.TemplateId = template.Id;
        return ContentEditingOperationStatus.Success;
    }

    private async Task<ContentEditingOperationStatus> Save(IContent content, Guid userKey)
    {
        try
        {
            var currentUserId = await GetUserIdAsync(userKey);
            OperationResult saveResult = ContentService.Save(content, currentUserId);
            return saveResult.Result switch
            {
                // these are the only result states currently expected from Save
                OperationResultType.Success => ContentEditingOperationStatus.Success,
                OperationResultType.FailedCancelledByEvent => ContentEditingOperationStatus.CancelledByNotification,

                // for any other state we'll return "unknown" so we know that we need to amend this
                _ => ContentEditingOperationStatus.Unknown
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Content save operation failed");
            return ContentEditingOperationStatus.Unknown;
        }
    }

    private async Task<int> GetUserIdAsync(Guid userKey) => await _userIdKeyResolver.GetAsync(userKey);

    private void Audit(AuditType type, int userId, int objectId, string? message = null, string? parameters = null) =>
        _auditRepository.Save(new AuditItem(objectId, type, userId, UmbracoObjectTypes.Document.GetName(), message,
            parameters));
}
