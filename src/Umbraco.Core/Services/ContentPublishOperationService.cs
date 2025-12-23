using System.ComponentModel;
using System.Collections.Immutable;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Exceptions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Persistence;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services.Changes;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Services;

/// <summary>
/// Implements content publishing operations.
/// </summary>
public class ContentPublishOperationService : ContentServiceBase, IContentPublishOperationService
{
    private readonly ILogger<ContentPublishOperationService> _logger;
    private readonly IContentCrudService _crudService;
    private readonly ILanguageRepository _languageRepository;
    private readonly Lazy<IPropertyValidationService> _propertyValidationService;
    private readonly ICultureImpactFactory _cultureImpactFactory;
    private readonly PropertyEditorCollection _propertyEditorCollection;
    private readonly IIdKeyMap _idKeyMap;

    // Thread-safe ContentSettings (Critical Review fix 2.1)
    private ContentSettings _contentSettings;
    private readonly object _contentSettingsLock = new object();

    public ContentPublishOperationService(
        ICoreScopeProvider provider,
        ILoggerFactory loggerFactory,
        IEventMessagesFactory eventMessagesFactory,
        IDocumentRepository documentRepository,
        IAuditService auditService,
        IUserIdKeyResolver userIdKeyResolver,
        IContentCrudService crudService,
        ILanguageRepository languageRepository,
        Lazy<IPropertyValidationService> propertyValidationService,
        ICultureImpactFactory cultureImpactFactory,
        PropertyEditorCollection propertyEditorCollection,
        IIdKeyMap idKeyMap,
        IOptionsMonitor<ContentSettings> optionsMonitor)
        : base(provider, loggerFactory, eventMessagesFactory, documentRepository, auditService, userIdKeyResolver)
    {
        _logger = loggerFactory.CreateLogger<ContentPublishOperationService>();
        _crudService = crudService ?? throw new ArgumentNullException(nameof(crudService));
        _languageRepository = languageRepository ?? throw new ArgumentNullException(nameof(languageRepository));
        _propertyValidationService = propertyValidationService ?? throw new ArgumentNullException(nameof(propertyValidationService));
        _cultureImpactFactory = cultureImpactFactory ?? throw new ArgumentNullException(nameof(cultureImpactFactory));
        _propertyEditorCollection = propertyEditorCollection ?? throw new ArgumentNullException(nameof(propertyEditorCollection));
        _idKeyMap = idKeyMap ?? throw new ArgumentNullException(nameof(idKeyMap));

        // Thread-safe settings initialization and subscription (Critical Review fix 2.1)
        ArgumentNullException.ThrowIfNull(optionsMonitor);
        lock (_contentSettingsLock)
        {
            _contentSettings = optionsMonitor.CurrentValue;
        }
        optionsMonitor.OnChange(settings =>
        {
            lock (_contentSettingsLock)
            {
                _contentSettings = settings;
            }
        });
    }

    /// <summary>
    /// Thread-safe accessor for ContentSettings.
    /// </summary>
    private ContentSettings ContentSettings
    {
        get
        {
            lock (_contentSettingsLock)
            {
                return _contentSettings;
            }
        }
    }

    #region Publishing

    /// <inheritdoc/>
    public PublishResult Publish(IContent content, string[] cultures, int userId = Constants.Security.SuperUserId)
    {
        if (content == null)
        {
            throw new ArgumentNullException(nameof(content));
        }

        if (cultures is null)
        {
            throw new ArgumentNullException(nameof(cultures));
        }

        if (cultures.Any(c => c.IsNullOrWhiteSpace()) || cultures.Distinct().Count() != cultures.Length)
        {
            throw new ArgumentException("Cultures cannot be null or whitespace", nameof(cultures));
        }

        cultures = cultures.Select(x => x.EnsureCultureCode()!).ToArray();

        EventMessages evtMsgs = EventMessagesFactory.Get();

        // we need to guard against unsaved changes before proceeding; the content will be saved, but we're not firing any saved notifications
        if (HasUnsavedChanges(content))
        {
            return new PublishResult(PublishResultType.FailedPublishUnsavedChanges, evtMsgs, content);
        }

        if (content.Name != null && content.Name.Length > 255)
        {
            throw new InvalidOperationException("Name cannot be more than 255 characters in length.");
        }

        PublishedState publishedState = content.PublishedState;
        if (publishedState != PublishedState.Published && publishedState != PublishedState.Unpublished)
        {
            throw new InvalidOperationException(
                $"Cannot save-and-publish (un)publishing content, use the dedicated {nameof(CommitDocumentChanges)} method.");
        }

        // cannot accept invariant (null or empty) culture for variant content type
        // cannot accept a specific culture for invariant content type (but '*' is ok)
        if (content.ContentType.VariesByCulture())
        {
            if (cultures.Length > 1 && cultures.Contains("*"))
            {
                throw new ArgumentException("Cannot combine wildcard and specific cultures when publishing variant content types.", nameof(cultures));
            }
        }
        else
        {
            if (cultures.Length == 0)
            {
                cultures = new[] { "*" };
            }

            if (cultures[0] != "*" || cultures.Length > 1)
            {
                throw new ArgumentException($"Only wildcard culture is supported when publishing invariant content types.", nameof(cultures));
            }
        }

        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            scope.WriteLock(Constants.Locks.ContentTree);

            var allLangs = _languageRepository.GetMany().ToList();

            // this will create the correct culture impact even if culture is * or null
            IEnumerable<CultureImpact?> impacts =
                cultures.Select(culture => _cultureImpactFactory.Create(culture, IsDefaultCulture(allLangs, culture), content));

            // publish the culture(s)
            // we don't care about the response here, this response will be rechecked below but we need to set the culture info values now.
            var publishTime = DateTime.UtcNow;
            foreach (CultureImpact? impact in impacts)
            {
                content.PublishCulture(impact, publishTime, _propertyEditorCollection);
            }

            // Change state to publishing
            content.PublishedState = PublishedState.Publishing;

            PublishResult result = CommitDocumentChangesInternal(scope, content, evtMsgs, allLangs, new Dictionary<string, object?>(), userId);
            scope.Complete();
            return result;
        }
    }

    #endregion

    #region Unpublishing

    /// <inheritdoc />
    public PublishResult Unpublish(IContent content, string? culture = "*", int userId = Constants.Security.SuperUserId)
    {
        if (content == null)
        {
            throw new ArgumentNullException(nameof(content));
        }

        EventMessages evtMsgs = EventMessagesFactory.Get();

        culture = culture?.NullOrWhiteSpaceAsNull().EnsureCultureCode();

        PublishedState publishedState = content.PublishedState;
        if (publishedState != PublishedState.Published && publishedState != PublishedState.Unpublished)
        {
            throw new InvalidOperationException(
                $"Cannot save-and-publish (un)publishing content, use the dedicated {nameof(CommitDocumentChanges)} method.");
        }

        // cannot accept invariant (null or empty) culture for variant content type
        // cannot accept a specific culture for invariant content type (but '*' is ok)
        if (content.ContentType.VariesByCulture())
        {
            if (culture == null)
            {
                throw new NotSupportedException("Invariant culture is not supported by variant content types.");
            }
        }
        else
        {
            if (culture != null && culture != "*")
            {
                throw new NotSupportedException(
                    $"Culture \"{culture}\" is not supported by invariant content types.");
            }
        }

        // if the content is not published, nothing to do
        if (!content.Published)
        {
            return new PublishResult(PublishResultType.SuccessUnpublishAlready, evtMsgs, content);
        }

        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            scope.WriteLock(Constants.Locks.ContentTree);

            var allLangs = _languageRepository.GetMany().ToList();

            var savingNotification = new ContentSavingNotification(content, evtMsgs);
            if (scope.Notifications.PublishCancelable(savingNotification))
            {
                return new PublishResult(PublishResultType.FailedPublishCancelledByEvent, evtMsgs, content);
            }

            // all cultures = unpublish whole
            if (culture == "*" || (!content.ContentType.VariesByCulture() && culture == null))
            {
                // Unpublish the culture, this will change the document state to Publishing! ... which is expected because this will
                // essentially be re-publishing the document with the requested culture removed
                // We are however unpublishing all cultures, so we will set this to unpublishing.
                content.UnpublishCulture(culture);
                content.PublishedState = PublishedState.Unpublishing;
                PublishResult result = CommitDocumentChangesInternal(scope, content, evtMsgs, allLangs, savingNotification.State, userId);
                scope.Complete();
                return result;
            }
            else
            {
                // Unpublish the culture, this will change the document state to Publishing! ... which is expected because this will
                // essentially be re-publishing the document with the requested culture removed.
                // The call to CommitDocumentChangesInternal will perform all the checks like if this is a mandatory culture or the last culture being unpublished
                // and will then unpublish the document accordingly.
                // If the result of this is false it means there was no culture to unpublish (i.e. it was already unpublished or it did not exist)
                var removed = content.UnpublishCulture(culture);

                // Save and publish any changes
                PublishResult result = CommitDocumentChangesInternal(scope, content, evtMsgs, allLangs, savingNotification.State, userId);

                scope.Complete();

                // In one case the result will be PublishStatusType.FailedPublishNothingToPublish which means that no cultures
                // were specified to be published which will be the case when removed is false. In that case
                // we want to swap the result type to PublishResultType.SuccessUnpublishAlready (that was the expectation before).
                if (result.Result == PublishResultType.FailedPublishNothingToPublish && !removed)
                {
                    return new PublishResult(PublishResultType.SuccessUnpublishAlready, evtMsgs, content);
                }

                return result;
            }
        }
    }

    #endregion

    #region Document Changes (Advanced API)

    /// <inheritdoc />
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public PublishResult CommitDocumentChanges(IContent content, int userId = Constants.Security.SuperUserId, IDictionary<string, object?>? notificationState = null)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            EventMessages evtMsgs = EventMessagesFactory.Get();

            scope.WriteLock(Constants.Locks.ContentTree);

            var savingNotification = new ContentSavingNotification(content, evtMsgs);
            if (scope.Notifications.PublishCancelable(savingNotification))
            {
                return new PublishResult(PublishResultType.FailedPublishCancelledByEvent, evtMsgs, content);
            }

            var allLangs = _languageRepository.GetMany().ToList();

            PublishResult result = CommitDocumentChangesInternal(scope, content, evtMsgs, allLangs, notificationState ?? savingNotification.State, userId);
            scope.Complete();
            return result;
        }
    }

    /// <summary>
    ///     Handles a lot of business logic cases for how the document should be persisted
    /// </summary>
    /// <param name="scope"></param>
    /// <param name="content"></param>
    /// <param name="allLangs"></param>
    /// <param name="notificationState"></param>
    /// <param name="userId"></param>
    /// <param name="branchOne"></param>
    /// <param name="branchRoot"></param>
    /// <param name="eventMessages"></param>
    /// <returns></returns>
    /// <remarks>
    ///     <para>
    ///         Business logic cases such: as unpublishing a mandatory culture, or unpublishing the last culture, checking for
    ///         pending scheduled publishing, etc... is dealt with in this method.
    ///         There is quite a lot of cases to take into account along with logic that needs to deal with scheduled
    ///         saving/publishing, branch saving/publishing, etc...
    ///     </para>
    /// </remarks>
    private PublishResult CommitDocumentChangesInternal(
        ICoreScope scope,
        IContent content,
        EventMessages eventMessages,
        IReadOnlyCollection<ILanguage> allLangs,
        IDictionary<string, object?>? notificationState,
        int userId,
        bool branchOne = false,
        bool branchRoot = false)
    {
        if (scope == null)
        {
            throw new ArgumentNullException(nameof(scope));
        }

        if (content == null)
        {
            throw new ArgumentNullException(nameof(content));
        }

        if (eventMessages == null)
        {
            throw new ArgumentNullException(nameof(eventMessages));
        }

        PublishResult? publishResult = null;
        PublishResult? unpublishResult = null;

        // nothing set = republish it all
        if (content.PublishedState != PublishedState.Publishing &&
            content.PublishedState != PublishedState.Unpublishing)
        {
            content.PublishedState = PublishedState.Publishing;
        }

        // State here is either Publishing or Unpublishing
        // Publishing to unpublish a culture may end up unpublishing everything so these flags can be flipped later
        var publishing = content.PublishedState == PublishedState.Publishing;
        var unpublishing = content.PublishedState == PublishedState.Unpublishing;

        var variesByCulture = content.ContentType.VariesByCulture();

        // Track cultures that are being published, changed, unpublished
        IReadOnlyList<string>? culturesPublishing = null;
        IReadOnlyList<string>? culturesUnpublishing = null;
        IReadOnlyList<string>? culturesChanging = variesByCulture
            ? content.CultureInfos?.Values.Where(x => x.IsDirty()).Select(x => x.Culture).ToList()
            : null;

        var isNew = !content.HasIdentity;
        TreeChangeTypes changeType = isNew ? TreeChangeTypes.RefreshNode : TreeChangeTypes.RefreshBranch;
        var previouslyPublished = content.HasIdentity && content.Published;

        // Inline method to persist the document with the documentRepository since this logic could be called a couple times below
        void SaveDocument(IContent c)
        {
            // save, always
            if (c.HasIdentity == false)
            {
                c.CreatorId = userId;
            }

            c.WriterId = userId;

            // saving does NOT change the published version, unless PublishedState is Publishing or Unpublishing
            DocumentRepository.Save(c);
        }

        if (publishing)
        {
            // Determine cultures publishing/unpublishing which will be based on previous calls to content.PublishCulture and ClearPublishInfo
            culturesUnpublishing = content.GetCulturesUnpublishing();
            culturesPublishing = variesByCulture
                ? content.PublishCultureInfos?.Values.Where(x => x.IsDirty()).Select(x => x.Culture).ToList()
                : null;

            // ensure that the document can be published, and publish handling events, business rules, etc
            publishResult = StrategyCanPublish(
                scope,
                content, /*checkPath:*/
                !branchOne || branchRoot,
                culturesPublishing,
                culturesUnpublishing,
                eventMessages,
                allLangs,
                notificationState);

            if (publishResult.Success)
            {
                // raise Publishing notification
                if (scope.Notifications.PublishCancelable(
                        new ContentPublishingNotification(content, eventMessages).WithState(notificationState)))
                {
                    _logger.LogInformation("Document {ContentName} (id={ContentId}) cannot be published: {Reason}", content.Name, content.Id, "publishing was cancelled");
                    return new PublishResult(PublishResultType.FailedPublishCancelledByEvent, eventMessages, content);
                }

                // note: StrategyPublish flips the PublishedState to Publishing!
                publishResult = StrategyPublish(content, culturesPublishing, culturesUnpublishing, eventMessages);

                // Check if a culture has been unpublished and if there are no cultures left, and then unpublish document as a whole
                if (publishResult.Result == PublishResultType.SuccessUnpublishCulture &&
                    content.PublishCultureInfos?.Count == 0)
                {
                    // This is a special case! We are unpublishing the last culture and to persist that we need to re-publish without any cultures
                    // so the state needs to remain Publishing to do that. However, we then also need to unpublish the document and to do that
                    // the state needs to be Unpublishing and it cannot be both. This state is used within the documentRepository to know how to
                    // persist certain things. So before proceeding below, we need to save the Publishing state to publish no cultures, then we can
                    // mark the document for Unpublishing.
                    SaveDocument(content);

                    // Set the flag to unpublish and continue
                    unpublishing = content.Published; // if not published yet, nothing to do
                }
            }
            else
            {
                // in a branch, just give up
                if (branchOne && !branchRoot)
                {
                    return publishResult;
                }

                // Check for mandatory culture missing, and then unpublish document as a whole
                if (publishResult.Result == PublishResultType.FailedPublishMandatoryCultureMissing)
                {
                    publishing = false;
                    unpublishing = content.Published; // if not published yet, nothing to do

                    // we may end up in a state where we won't publish nor unpublish
                    // keep going, though, as we want to save anyways
                }

                // reset published state from temp values (publishing, unpublishing) to original value
                // (published, unpublished) in order to save the document, unchanged - yes, this is odd,
                // but: (a) it means we don't reproduce the PublishState logic here and (b) setting the
                // PublishState to anything other than Publishing or Unpublishing - which is precisely
                // what we want to do here - throws
                content.Published = content.Published;
            }
        }

        // won't happen in a branch
        if (unpublishing)
        {
            IContent? newest = _crudService.GetById(content.Id); // ensure we have the newest version - in scope
            if (content.VersionId != newest?.VersionId)
            {
                return new PublishResult(PublishResultType.FailedPublishConcurrencyViolation, eventMessages, content);
            }

            if (content.Published)
            {
                // ensure that the document can be unpublished, and unpublish
                // handling events, business rules, etc
                // note: StrategyUnpublish flips the PublishedState to Unpublishing!
                // note: This unpublishes the entire document (not different variants)
                unpublishResult = StrategyCanUnpublish(scope, content, eventMessages, notificationState);
                if (unpublishResult.Success)
                {
                    unpublishResult = StrategyUnpublish(content, eventMessages);
                }
                else
                {
                    // reset published state from temp values (publishing, unpublishing) to original value
                    // (published, unpublished) in order to save the document, unchanged - yes, this is odd,
                    // but: (a) it means we don't reproduce the PublishState logic here and (b) setting the
                    // PublishState to anything other than Publishing or Unpublishing - which is precisely
                    // what we want to do here - throws
                    content.Published = content.Published;
                    return unpublishResult;
                }
            }
            else
            {
                // already unpublished - optimistic concurrency collision, really,
                // and I am not sure at all what we should do, better die fast, else
                // we may end up corrupting the db
                throw new InvalidOperationException("Concurrency collision.");
            }
        }

        // Persist the document
        SaveDocument(content);

        // we have tried to unpublish - won't happen in a branch
        if (unpublishing)
        {
            // and succeeded, trigger events
            if (unpublishResult?.Success ?? false)
            {
                // events and audit
                scope.Notifications.Publish(
                    new ContentUnpublishedNotification(content, eventMessages).WithState(notificationState));
                scope.Notifications.Publish(new ContentTreeChangeNotification(
                    content,
                    TreeChangeTypes.RefreshBranch,
                    variesByCulture ? culturesPublishing.IsCollectionEmpty() ? null : culturesPublishing : null,
                    variesByCulture ? culturesUnpublishing.IsCollectionEmpty() ? null : culturesUnpublishing : ["*"],
                    eventMessages));

                if (culturesUnpublishing != null)
                {
                    // This will mean that that we unpublished a mandatory culture or we unpublished the last culture.
                    var langs = GetLanguageDetailsForAuditEntry(allLangs, culturesUnpublishing);
                    Audit(AuditType.UnpublishVariant, userId, content.Id, $"Unpublished languages: {langs}", langs);

                    if (publishResult == null)
                    {
                        throw new PanicException("publishResult == null - should not happen");
                    }

                    switch (publishResult.Result)
                    {
                        case PublishResultType.FailedPublishMandatoryCultureMissing:
                            // Occurs when a mandatory culture was unpublished (which means we tried publishing the document without a mandatory culture)

                            // Log that the whole content item has been unpublished due to mandatory culture unpublished
                            Audit(AuditType.Unpublish, userId, content.Id, "Unpublished (mandatory language unpublished)");
                            return new PublishResult(PublishResultType.SuccessUnpublishMandatoryCulture, eventMessages, content);
                        case PublishResultType.SuccessUnpublishCulture:
                            // Occurs when the last culture is unpublished
                            Audit(AuditType.Unpublish, userId, content.Id, "Unpublished (last language unpublished)");
                            return new PublishResult(PublishResultType.SuccessUnpublishLastCulture, eventMessages, content);
                    }
                }

                Audit(AuditType.Unpublish, userId, content.Id);
                return new PublishResult(PublishResultType.SuccessUnpublish, eventMessages, content);
            }

            // or, failed
            scope.Notifications.Publish(new ContentTreeChangeNotification(content, changeType, eventMessages));
            return new PublishResult(PublishResultType.FailedUnpublish, eventMessages, content); // bah
        }

        // we have tried to publish
        if (publishing)
        {
            // and succeeded, trigger events
            if (publishResult?.Success ?? false)
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
                        new ContentTreeChangeNotification(
                            content,
                            changeType,
                            variesByCulture ? culturesPublishing.IsCollectionEmpty() ? null : culturesPublishing : ["*"],
                            variesByCulture ? culturesUnpublishing.IsCollectionEmpty() ? null : culturesUnpublishing : null,
                            eventMessages));
                    scope.Notifications.Publish(
                        new ContentPublishedNotification(content, eventMessages).WithState(notificationState));
                }

                // it was not published and now is... descendants that were 'published' (but
                // had an unpublished ancestor) are 're-published' ie not explicitly published
                // but back as 'published' nevertheless
                if (!branchOne && isNew == false && previouslyPublished == false && _crudService.HasChildren(content.Id))
                {
                    IContent[] descendants = GetPublishedDescendantsLocked(content).ToArray();
                    scope.Notifications.Publish(
                        new ContentPublishedNotification(descendants, eventMessages).WithState(notificationState));
                }

                switch (publishResult.Result)
                {
                    case PublishResultType.SuccessPublish:
                        Audit(AuditType.Publish, userId, content.Id);
                        break;
                    case PublishResultType.SuccessPublishCulture:
                        if (culturesPublishing != null)
                        {
                            var langs = GetLanguageDetailsForAuditEntry(allLangs, culturesPublishing);
                            Audit(AuditType.PublishVariant, userId, content.Id, $"Published languages: {langs}", langs);
                        }

                        break;
                    case PublishResultType.SuccessUnpublishCulture:
                        if (culturesUnpublishing != null)
                        {
                            var langs = GetLanguageDetailsForAuditEntry(allLangs, culturesUnpublishing);
                            Audit(AuditType.UnpublishVariant, userId, content.Id, $"Unpublished languages: {langs}", langs);
                        }

                        break;
                }

                return publishResult;
            }
        }

        // should not happen
        if (branchOne && !branchRoot)
        {
            throw new PanicException("branchOne && !branchRoot - should not happen");
        }

        // if publishing didn't happen or if it has failed, we still need to log which cultures were saved
        if (!branchOne && (publishResult == null || !publishResult.Success))
        {
            if (culturesChanging != null)
            {
                var langs = GetLanguageDetailsForAuditEntry(allLangs, culturesChanging);
                Audit(AuditType.SaveVariant, userId, content.Id, $"Saved languages: {langs}", langs);
            }
            else
            {
                Audit(AuditType.Save, userId, content.Id);
            }
        }

        // or, failed
        scope.Notifications.Publish(new ContentTreeChangeNotification(content, changeType, eventMessages));
        return publishResult!;
    }

    #endregion

    #region Scheduled Publishing

    /// <inheritdoc />
    public IEnumerable<PublishResult> PerformScheduledPublish(DateTime date)
    {
        var allLangs = new Lazy<List<ILanguage>>(() => _languageRepository.GetMany().ToList());
        EventMessages evtMsgs = EventMessagesFactory.Get();
        var results = new List<PublishResult>();

        PerformScheduledPublishingRelease(date, results, evtMsgs, allLangs);
        PerformScheduledPublishingExpiration(date, results, evtMsgs, allLangs);

        // Explicit failure logging (Critical Review fix)
        foreach (var result in results.Where(r => !r.Success))
        {
            _logger.LogError("Scheduled publishing failed for document id={DocumentId}, reason={Reason}", result.Content?.Id, result.Result);
        }

        return results;
    }

    private void PerformScheduledPublishingExpiration(DateTime date, List<PublishResult> results, EventMessages evtMsgs, Lazy<List<ILanguage>> allLangs)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope();

        // do a fast read without any locks since this executes often to see if we even need to proceed
        if (DocumentRepository.HasContentForExpiration(date))
        {
            // now take a write lock since we'll be updating
            scope.WriteLock(Constants.Locks.ContentTree);

            foreach (IContent d in DocumentRepository.GetContentForExpiration(date))
            {
                ContentScheduleCollection contentSchedule = DocumentRepository.GetContentSchedule(d.Id);
                if (d.ContentType.VariesByCulture())
                {
                    // find which cultures have pending schedules
                    var pendingCultures = contentSchedule.GetPending(ContentScheduleAction.Expire, date)
                        .Select(x => x.Culture)
                        .Distinct()
                        .ToList();

                    if (pendingCultures.Count == 0)
                    {
                        continue; // shouldn't happen but no point in processing this document if there's nothing there
                    }

                    var savingNotification = new ContentSavingNotification(d, evtMsgs);
                    if (scope.Notifications.PublishCancelable(savingNotification))
                    {
                        results.Add(new PublishResult(PublishResultType.FailedPublishCancelledByEvent, evtMsgs, d));
                        continue;
                    }

                    foreach (var c in pendingCultures)
                    {
                        // Clear this schedule for this culture
                        contentSchedule.Clear(c, ContentScheduleAction.Expire, date);

                        // set the culture to be published
                        d.UnpublishCulture(c);
                    }

                    DocumentRepository.PersistContentSchedule(d, contentSchedule);
                    PublishResult result = CommitDocumentChangesInternal(scope, d, evtMsgs, allLangs.Value, savingNotification.State, d.WriterId);
                    if (result.Success == false)
                    {
                        _logger.LogError(null, "Failed to publish document id={DocumentId}, reason={Reason}.", d.Id, result.Result);
                    }

                    results.Add(result);
                }
                else
                {
                    // Clear this schedule for this culture
                    contentSchedule.Clear(ContentScheduleAction.Expire, date);
                    DocumentRepository.PersistContentSchedule(d, contentSchedule);
                    PublishResult result = Unpublish(d, userId: d.WriterId);
                    if (result.Success == false)
                    {
                        _logger.LogError(null, "Failed to unpublish document id={DocumentId}, reason={Reason}.", d.Id, result.Result);
                    }

                    results.Add(result);
                }
            }

            DocumentRepository.ClearSchedule(date, ContentScheduleAction.Expire);
        }

        scope.Complete();
    }

    private void PerformScheduledPublishingRelease(DateTime date, List<PublishResult> results, EventMessages evtMsgs, Lazy<List<ILanguage>> allLangs)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope();

        // do a fast read without any locks since this executes often to see if we even need to proceed
        if (DocumentRepository.HasContentForRelease(date))
        {
            // now take a write lock since we'll be updating
            scope.WriteLock(Constants.Locks.ContentTree);

            foreach (IContent d in DocumentRepository.GetContentForRelease(date))
            {
                ContentScheduleCollection contentSchedule = DocumentRepository.GetContentSchedule(d.Id);
                if (d.ContentType.VariesByCulture())
                {
                    // find which cultures have pending schedules
                    var pendingCultures = contentSchedule.GetPending(ContentScheduleAction.Release, date)
                        .Select(x => x.Culture)
                        .Distinct()
                        .ToList();

                    if (pendingCultures.Count == 0)
                    {
                        continue; // shouldn't happen but no point in processing this document if there's nothing there
                    }
                    var savingNotification = new ContentSavingNotification(d, evtMsgs);
                    if (scope.Notifications.PublishCancelable(savingNotification))
                    {
                        results.Add(new PublishResult(PublishResultType.FailedPublishCancelledByEvent, evtMsgs, d));
                        continue;
                    }


                    var publishing = true;
                    foreach (var culture in pendingCultures)
                    {
                        // Clear this schedule for this culture
                        contentSchedule.Clear(culture, ContentScheduleAction.Release, date);

                        if (d.Trashed)
                        {
                            continue; // won't publish
                        }

                        // publish the culture values and validate the property values, if validation fails, log the invalid properties so the develeper has an idea of what has failed
                        IProperty[]? invalidProperties = null;
                        CultureImpact impact = _cultureImpactFactory.ImpactExplicit(culture, IsDefaultCulture(allLangs.Value, culture));
                        var tryPublish = d.PublishCulture(impact, date, _propertyEditorCollection) &&
                                         _propertyValidationService.Value.IsPropertyDataValid(d, out invalidProperties, impact);
                        if (invalidProperties != null && invalidProperties.Length > 0)
                        {
                            _logger.LogWarning(
                                "Scheduled publishing will fail for document {DocumentId} and culture {Culture} because of invalid properties {InvalidProperties}",
                                d.Id,
                                culture,
                                string.Join(",", invalidProperties.Select(x => x.Alias)));
                        }

                        publishing &= tryPublish; // set the culture to be published
                    }

                    PublishResult result;

                    if (d.Trashed)
                    {
                        result = new PublishResult(PublishResultType.FailedPublishIsTrashed, evtMsgs, d);
                    }
                    else if (!publishing)
                    {
                        result = new PublishResult(PublishResultType.FailedPublishContentInvalid, evtMsgs, d);
                    }
                    else
                    {
                        DocumentRepository.PersistContentSchedule(d, contentSchedule);
                        result = CommitDocumentChangesInternal(scope, d, evtMsgs, allLangs.Value, savingNotification.State, d.WriterId);
                    }

                    if (result.Success == false)
                    {
                        _logger.LogError(null, "Failed to publish document id={DocumentId}, reason={Reason}.", d.Id, result.Result);
                    }

                    results.Add(result);
                }
                else
                {
                    // Clear this schedule
                    contentSchedule.Clear(ContentScheduleAction.Release, date);

                    PublishResult? result = null;

                    if (d.Trashed)
                    {
                        result = new PublishResult(PublishResultType.FailedPublishIsTrashed, evtMsgs, d);
                    }
                    else
                    {
                        DocumentRepository.PersistContentSchedule(d, contentSchedule);
                        result = Publish(d, d.AvailableCultures.ToArray(), userId: d.WriterId);
                    }

                    if (result.Success == false)
                    {
                        _logger.LogError(null, "Failed to publish document id={DocumentId}, reason={Reason}.", d.Id, result.Result);
                    }

                    results.Add(result);
                }
            }

            DocumentRepository.ClearSchedule(date, ContentScheduleAction.Release);
        }

        scope.Complete();
    }

    /// <inheritdoc />
    public IEnumerable<IContent> GetContentForExpiration(DateTime date)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            scope.ReadLock(Constants.Locks.ContentTree);
            return DocumentRepository.GetContentForExpiration(date);
        }
    }

    /// <inheritdoc />
    public IEnumerable<IContent> GetContentForRelease(DateTime date)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            scope.ReadLock(Constants.Locks.ContentTree);
            return DocumentRepository.GetContentForRelease(date);
        }
    }

    #endregion

    #region Schedule Management

    /// <inheritdoc />
    public ContentScheduleCollection GetContentScheduleByContentId(int contentId)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            scope.ReadLock(Constants.Locks.ContentTree);
            return DocumentRepository.GetContentSchedule(contentId);
        }
    }

    public ContentScheduleCollection GetContentScheduleByContentId(Guid contentId)
    {
        Attempt<int> idAttempt = _idKeyMap.GetIdForKey(contentId, UmbracoObjectTypes.Document);
        if (idAttempt.Success is false)
        {
            return new ContentScheduleCollection();
        }

        return GetContentScheduleByContentId(idAttempt.Result);
    }

    /// <inheritdoc />
    public void PersistContentSchedule(IContent content, ContentScheduleCollection contentSchedule)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            scope.WriteLock(Constants.Locks.ContentTree);
            DocumentRepository.PersistContentSchedule(content, contentSchedule);
            scope.Complete();
        }
    }

    /// <inheritdoc/>
    public IDictionary<int, IEnumerable<ContentSchedule>> GetContentSchedulesByIds(Guid[] keys)
    {
        // Critical Review fix 2.4: Add null/empty check
        if (keys == null || keys.Length == 0)
        {
            return ImmutableDictionary<int, IEnumerable<ContentSchedule>>.Empty;
        }

        List<int> contentIds = [];
        foreach (var key in keys)
        {
            Attempt<int> contentId = _idKeyMap.GetIdForKey(key, UmbracoObjectTypes.Document);
            if (contentId.Success is false)
            {
                continue;
            }

            contentIds.Add(contentId.Result);
        }

        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            scope.ReadLock(Constants.Locks.ContentTree);
            return DocumentRepository.GetContentSchedulesByIds(contentIds.ToArray());
        }
    }

    #endregion

    #region Path Checks

    /// <summary>
    ///     Checks if the passed in <see cref="IContent" /> can be published based on the ancestors publish state.
    /// </summary>
    /// <param name="content"><see cref="IContent" /> to check if ancestors are published</param>
    /// <returns>True if the Content can be published, otherwise False</returns>
    public bool IsPathPublishable(IContent content)
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
        IContent? parent = _crudService.GetById(content.ParentId);
        return parent == null || IsPathPublished(parent);
    }

    public bool IsPathPublished(IContent? content)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            scope.ReadLock(Constants.Locks.ContentTree);
            return DocumentRepository.IsPathPublished(content);
        }
    }

    #endregion

    #region Workflow

    /// <summary>
    ///     Sends an <see cref="IContent" /> to Publication, which executes handlers and events for the 'Send to Publication'
    ///     action.
    /// </summary>
    /// <param name="content">The <see cref="IContent" /> to send to publication</param>
    /// <param name="userId">Optional Id of the User issuing the send to publication</param>
    /// <returns>True if sending publication was successful otherwise false</returns>
    public bool SendToPublication(IContent? content, int userId = Constants.Security.SuperUserId)
    {
        if (content is null)
        {
            return false;
        }

        EventMessages evtMsgs = EventMessagesFactory.Get();

        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            var sendingToPublishNotification = new ContentSendingToPublishNotification(content, evtMsgs);
            if (scope.Notifications.PublishCancelable(sendingToPublishNotification))
            {
                scope.Complete();
                return false;
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
            OperationResult saveResult = _crudService.Save(content, userId);

            // always complete (but maybe return a failed status)
            scope.Complete();

            if (!saveResult.Success)
            {
                return saveResult.Success;
            }

            scope.Notifications.Publish(
                new ContentSentToPublishNotification(content, evtMsgs).WithStateFrom(sendingToPublishNotification));

            if (culturesChanging != null)
            {
                Audit(AuditType.SendToPublishVariant, userId, content.Id, $"Send To Publish for cultures: {culturesChanging}", culturesChanging);
            }
            else
            {
                Audit(AuditType.SendToPublish, userId, content.Id);
            }

            return saveResult.Success;
        }
    }

    #endregion

    #region Published Content Queries

    /// <summary>
    ///     Gets a collection of published <see cref="IContent" /> objects by Parent Id
    /// </summary>
    /// <param name="id">Id of the Parent to retrieve Children from</param>
    /// <returns>An Enumerable list of published <see cref="IContent" /> objects</returns>
    public IEnumerable<IContent> GetPublishedChildren(int id)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            scope.ReadLock(Constants.Locks.ContentTree);
            IQuery<IContent>? query = Query<IContent>().Where(x => x.ParentId == id && x.Published);
            return DocumentRepository.Get(query).OrderBy(x => x.SortOrder);
        }
    }

    #endregion

    #region Branch Publishing

    /// <inheritdoc />
    public IEnumerable<PublishResult> PublishBranch(IContent content, PublishBranchFilter publishBranchFilter, string[] cultures, int userId = Constants.Security.SuperUserId)
    {
        // note: EditedValue and PublishedValue are objects here, so it is important to .Equals()
        // and not to == them, else we would be comparing references, and that is a bad thing

        cultures = EnsureCultures(content, cultures);

        string? defaultCulture;
        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            defaultCulture = _languageRepository.GetDefaultIsoCode();
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

        return PublishBranch(content, ShouldPublish, PublishBranch_PublishCultures, userId);
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

    internal IEnumerable<PublishResult> PublishBranch(
        IContent document,
        Func<IContent, HashSet<string>?> shouldPublish,
        Func<IContent, HashSet<string>, IReadOnlyCollection<ILanguage>, bool> publishCultures,
        int userId = Constants.Security.SuperUserId)
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

            var allLangs = _languageRepository.GetMany().ToList();

            if (!document.HasIdentity)
            {
                throw new InvalidOperationException("Cannot not branch-publish a new document.");
            }

            PublishedState publishedState = document.PublishedState;
            if (publishedState == PublishedState.Publishing)
            {
                throw new InvalidOperationException("Cannot mix PublishCulture and SaveAndPublishBranch.");
            }

            // deal with the branch root - if it fails, abort
            HashSet<string>? culturesToPublish = shouldPublish(document);
            PublishResult? result = PublishBranchItem(scope, document, culturesToPublish, publishCultures, true, publishedDocuments, eventMessages, userId, allLangs, out IDictionary<string, object?>? notificationState);
            if (result != null)
            {
                results.Add(result);
                if (!result.Success)
                {
                    return results;
                }
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
                foreach (IContent d in _crudService.GetPagedDescendants(document.Id, page, pageSize, out _, ordering: Ordering.By("Path", Direction.Ascending)))
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
                    result = PublishBranchItem(scope, d, culturesToPublish, publishCultures, false, publishedDocuments, eventMessages, userId, allLangs, out _);
                    if (result != null)
                    {
                        results.Add(result);
                        if (result.Success)
                        {
                            culturesPublished.UnionWith(culturesToPublish ?? []);
                            continue;
                        }
                    }

                    // if we could not publish the document, cut its branch
                    exclude.Add(d.Id);
                }

                page++;
            }
            while (count > 0);

            Audit(AuditType.Publish, userId, document.Id, "Branch published");

            // trigger events for the entire branch
            // (SaveAndPublishBranchOne does *not* do it)
            var variesByCulture = document.ContentType.VariesByCulture();
            scope.Notifications.Publish(
                new ContentTreeChangeNotification(
                    document,
                    TreeChangeTypes.RefreshBranch,
                    variesByCulture ? culturesPublished.IsCollectionEmpty() ? null : culturesPublished : ["*"],
                    null,
                    eventMessages));
            scope.Notifications.Publish(new ContentPublishedNotification(publishedDocuments, eventMessages, true).WithState(notificationState));

            scope.Complete();
        }

        return results;
    }

    // shouldPublish: a function determining whether the document has changes that need to be published
    //  note - 'force' is handled by 'editing'
    // publishValues: a function publishing values (using the appropriate PublishCulture calls)
    private PublishResult? PublishBranchItem(
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
        out IDictionary<string, object?>? initialNotificationState)
    {
        initialNotificationState = new Dictionary<string, object?>();

        // we need to guard against unsaved changes before proceeding; the document will be saved, but we're not firing any saved notifications
        if (HasUnsavedChanges(document))
        {
            return new PublishResult(PublishResultType.FailedPublishUnsavedChanges, evtMsgs, document);
        }

        // null = do not include
        if (culturesToPublish == null)
        {
            return null;
        }

        // empty = already published
        if (culturesToPublish.Count == 0)
        {
            return new PublishResult(PublishResultType.SuccessPublishAlready, evtMsgs, document);
        }

        var savingNotification = new ContentSavingNotification(document, evtMsgs);
        if (scope.Notifications.PublishCancelable(savingNotification))
        {
            return new PublishResult(PublishResultType.FailedPublishCancelledByEvent, evtMsgs, document);
        }

        // publish & check if values are valid
        if (!publishCultures(document, culturesToPublish, allLangs))
        {
            // TODO: Based on this callback behavior there is no way to know which properties may have been invalid if this failed, see other results of FailedPublishContentInvalid
            return new PublishResult(PublishResultType.FailedPublishContentInvalid, evtMsgs, document);
        }

        PublishResult result = CommitDocumentChangesInternal(scope, document, evtMsgs, allLangs, savingNotification.State, userId, true, isRoot);
        if (result.Success)
        {
            publishedDocuments.Add(document);
        }

        return result;
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

    #endregion

    #region Publishing Strategies

    /// <summary>
    ///     Ensures that a document can be published
    /// </summary>
    /// <param name="scope"></param>
    /// <param name="content"></param>
    /// <param name="checkPath"></param>
    /// <param name="culturesUnpublishing"></param>
    /// <param name="evtMsgs"></param>
    /// <param name="culturesPublishing"></param>
    /// <param name="allLangs"></param>
    /// <param name="notificationState"></param>
    /// <returns></returns>
    private PublishResult StrategyCanPublish(
        ICoreScope scope,
        IContent content,
        bool checkPath,
        IReadOnlyList<string>? culturesPublishing,
        IReadOnlyCollection<string>? culturesUnpublishing,
        EventMessages evtMsgs,
        IReadOnlyCollection<ILanguage> allLangs,
        IDictionary<string, object?>? notificationState)
    {
        var variesByCulture = content.ContentType.VariesByCulture();

        // If it's null it's invariant
        CultureImpact[] impactsToPublish = culturesPublishing == null
                ? new[] { _cultureImpactFactory.ImpactInvariant() }
            : culturesPublishing.Select(x =>
                _cultureImpactFactory.ImpactExplicit(
                        x,
                        allLangs.Any(lang => lang.IsoCode.InvariantEquals(x) && lang.IsMandatory)))
                    .ToArray();

        // publish the culture(s)
        var publishTime = DateTime.UtcNow;
        if (!impactsToPublish.All(impact => content.PublishCulture(impact, publishTime, _propertyEditorCollection)))
        {
            return new PublishResult(PublishResultType.FailedPublishContentInvalid, evtMsgs, content);
        }

        // Validate the property values
        IProperty[]? invalidProperties = null;
        if (!impactsToPublish.All(x =>
                _propertyValidationService.Value.IsPropertyDataValid(content, out invalidProperties, x)))
        {
            return new PublishResult(PublishResultType.FailedPublishContentInvalid, evtMsgs, content)
            {
                InvalidProperties = invalidProperties,
            };
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

            if (content.Published && culturesPublishing.Count == 0 && culturesUnpublishing?.Count == 0)
            {
                // no published cultures = cannot be published
                // This will occur if for example, a culture that is already unpublished is sent to be unpublished again, or vice versa, in that case
                // there will be nothing to publish/unpublish.
                return new PublishResult(PublishResultType.FailedPublishNothingToPublish, evtMsgs, content);
            }

            // missing mandatory culture = cannot be published
            IEnumerable<string> mandatoryCultures = allLangs.Where(x => x.IsMandatory).Select(x => x.IsoCode);
            var mandatoryMissing = mandatoryCultures.Any(x =>
                !content.PublishedCultures.Contains(x, StringComparer.OrdinalIgnoreCase));
            if (mandatoryMissing)
            {
                return new PublishResult(PublishResultType.FailedPublishMandatoryCultureMissing, evtMsgs, content);
            }

            if (culturesPublishing.Count == 0 && culturesUnpublishing?.Count > 0)
            {
                return new PublishResult(PublishResultType.SuccessUnpublishCulture, evtMsgs, content);
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
            return new PublishResult(PublishResultType.FailedPublishNothingToPublish, evtMsgs, content);
        }

        ContentScheduleCollection contentSchedule = DocumentRepository.GetContentSchedule(content.Id);

        // loop over each culture publishing - or InvariantCulture for invariant
        foreach (var culture in culturesPublishing ?? new[] { Constants.System.InvariantCulture })
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

                    return new PublishResult(
                        !variesByCulture
                            ? PublishResultType.FailedPublishHasExpired : PublishResultType.FailedPublishCultureHasExpired,
                        evtMsgs,
                        content);

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
                            "document has culture awaiting release");
                    }

                    return new PublishResult(
                        !variesByCulture
                            ? PublishResultType.FailedPublishAwaitingRelease
                            : PublishResultType.FailedPublishCultureAwaitingRelease,
                        evtMsgs,
                        content);

                case ContentStatus.Trashed:
                    _logger.LogInformation(
                        "Document {ContentName} (id={ContentId}) cannot be published: {Reason}",
                        content.Name,
                        content.Id,
                        "document is trashed");
                    return new PublishResult(PublishResultType.FailedPublishIsTrashed, evtMsgs, content);
            }
        }

        if (checkPath)
        {
            // check if the content can be path-published
            // root content can be published
            // else check ancestors - we know we are not trashed
            var pathIsOk = content.ParentId == Constants.System.Root || IsPathPublished(_crudService.GetParent(content));
            if (!pathIsOk)
            {
                _logger.LogInformation(
                    "Document {ContentName} (id={ContentId}) cannot be published: {Reason}",
                    content.Name,
                    content.Id,
                    "parent is not published");
                return new PublishResult(PublishResultType.FailedPublishPathNotPublished, evtMsgs, content);
            }
        }

        // If we are both publishing and unpublishing cultures, then return a mixed status
        if (variesByCulture && culturesPublishing?.Count > 0 && culturesUnpublishing?.Count > 0)
        {
            return new PublishResult(PublishResultType.SuccessMixedCulture, evtMsgs, content);
        }

        return new PublishResult(evtMsgs, content);
    }

    /// <summary>
    ///     Publishes a document
    /// </summary>
    /// <param name="content"></param>
    /// <param name="culturesUnpublishing"></param>
    /// <param name="evtMsgs"></param>
    /// <param name="culturesPublishing"></param>
    /// <returns></returns>
    /// <remarks>
    ///     It is assumed that all publishing checks have passed before calling this method like
    ///     <see cref="StrategyCanPublish" />
    /// </remarks>
    private PublishResult StrategyPublish(
        IContent content,
        IReadOnlyCollection<string>? culturesPublishing,
        IReadOnlyCollection<string>? culturesUnpublishing,
        EventMessages evtMsgs)
    {
        // change state to publishing
        content.PublishedState = PublishedState.Publishing;

        // if this is a variant then we need to log which cultures have been published/unpublished and return an appropriate result
        if (content.ContentType.VariesByCulture())
        {
            if (content.Published && culturesUnpublishing?.Count == 0 && culturesPublishing?.Count == 0)
            {
                return new PublishResult(PublishResultType.FailedPublishNothingToPublish, evtMsgs, content);
            }

            if (culturesUnpublishing?.Count > 0)
            {
                _logger.LogInformation(
                    "Document {ContentName} (id={ContentId}) cultures: {Cultures} have been unpublished.",
                    content.Name,
                    content.Id,
                    string.Join(",", culturesUnpublishing));
            }

            if (culturesPublishing?.Count > 0)
            {
                _logger.LogInformation(
                    "Document {ContentName} (id={ContentId}) cultures: {Cultures} have been published.",
                    content.Name,
                    content.Id,
                    string.Join(",", culturesPublishing));
            }

            if (culturesUnpublishing?.Count > 0 && culturesPublishing?.Count > 0)
            {
                return new PublishResult(PublishResultType.SuccessMixedCulture, evtMsgs, content);
            }

            if (culturesUnpublishing?.Count > 0 && culturesPublishing?.Count == 0)
            {
                return new PublishResult(PublishResultType.SuccessUnpublishCulture, evtMsgs, content);
            }

            return new PublishResult(PublishResultType.SuccessPublishCulture, evtMsgs, content);
        }

        _logger.LogInformation("Document {ContentName} (id={ContentId}) has been published.", content.Name, content.Id);
        return new PublishResult(evtMsgs, content);
    }

    /// <summary>
    ///     Ensures that a document can be unpublished
    /// </summary>
    /// <param name="scope"></param>
    /// <param name="content"></param>
    /// <param name="evtMsgs"></param>
    /// <param name="notificationState"></param>
    /// <returns></returns>
    private PublishResult StrategyCanUnpublish(
        ICoreScope scope,
        IContent content,
        EventMessages evtMsgs,
        IDictionary<string, object?>? notificationState)
    {
        // raise Unpublishing notification
        ContentUnpublishingNotification notification = new ContentUnpublishingNotification(content, evtMsgs).WithState(notificationState);
        var notificationResult = scope.Notifications.PublishCancelable(notification);

        if (notificationResult)
        {
            _logger.LogInformation(
                "Document {ContentName} (id={ContentId}) cannot be unpublished: unpublishing was cancelled.", content.Name, content.Id);
            return new PublishResult(PublishResultType.FailedUnpublishCancelledByEvent, evtMsgs, content);
        }

        return new PublishResult(PublishResultType.SuccessUnpublish, evtMsgs, content);
    }

    /// <summary>
    ///     Unpublishes a document
    /// </summary>
    /// <param name="content"></param>
    /// <param name="evtMsgs"></param>
    /// <returns></returns>
    /// <remarks>
    ///     It is assumed that all unpublishing checks have passed before calling this method like
    ///     <see cref="StrategyCanUnpublish" />
    /// </remarks>
    private PublishResult StrategyUnpublish(IContent content, EventMessages evtMsgs)
    {
        var attempt = new PublishResult(PublishResultType.SuccessUnpublish, evtMsgs, content);

        // TODO: What is this check?? we just created this attempt and of course it is Success?!
        if (attempt.Success == false)
        {
            return attempt;
        }

        // if the document has any release dates set to before now,
        // they should be removed so they don't interrupt an unpublish
        // otherwise it would remain released == published
        ContentScheduleCollection contentSchedule = DocumentRepository.GetContentSchedule(content.Id);
        IReadOnlyList<ContentSchedule> pastReleases =
            contentSchedule.GetPending(ContentScheduleAction.Expire, DateTime.UtcNow);
        foreach (ContentSchedule p in pastReleases)
        {
            contentSchedule.Remove(p);
        }

        if (pastReleases.Count > 0)
        {
            _logger.LogInformation(
                "Document {ContentName} (id={ContentId}) had its release date removed, because it was unpublished.", content.Name, content.Id);
        }

        DocumentRepository.PersistContentSchedule(content, contentSchedule);

        // change state to unpublishing
        content.PublishedState = PublishedState.Unpublishing;

        _logger.LogInformation("Document {ContentName} (id={ContentId}) has been unpublished.", content.Name, content.Id);
        return attempt;
    }

    #endregion

    #region Internal Methods

    /// <summary>
    ///     Gets a collection of <see cref="IContent" /> descendants by the first Parent.
    /// </summary>
    /// <param name="content"><see cref="IContent" /> item to retrieve Descendants from</param>
    /// <returns>An Enumerable list of <see cref="IContent" /> objects</returns>
    internal IEnumerable<IContent> GetPublishedDescendants(IContent content)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            scope.ReadLock(Constants.Locks.ContentTree);
            return GetPublishedDescendantsLocked(content).ToArray(); // ToArray important in uow!
        }
    }

    internal IEnumerable<IContent> GetPublishedDescendantsLocked(IContent content)
    {
        var pathMatch = content.Path + ",";
        IQuery<IContent> query = Query<IContent>()
            .Where(x => x.Id != content.Id && x.Path.StartsWith(pathMatch) /*&& culture.Trashed == false*/);
        IEnumerable<IContent> contents = DocumentRepository.Get(query);

        // beware! contents contains all published version below content
        // including those that are not directly published because below an unpublished content
        // these must be filtered out here
        var parents = new List<int> { content.Id };
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

    #endregion

    #region Private Methods

    private static bool HasUnsavedChanges(IContent content) => content.HasIdentity is false || content.IsDirty();

    private string GetLanguageDetailsForAuditEntry(IEnumerable<string> affectedCultures)
        => GetLanguageDetailsForAuditEntry(_languageRepository.GetMany(), affectedCultures);

    private static string GetLanguageDetailsForAuditEntry(IEnumerable<ILanguage> languages, IEnumerable<string> affectedCultures)
    {
        IEnumerable<string> languageIsoCodes = languages
            .Where(x => affectedCultures.InvariantContains(x.IsoCode))
            .Select(x => x.IsoCode);
        return string.Join(", ", languageIsoCodes);
    }

    private static bool IsDefaultCulture(IReadOnlyCollection<ILanguage>? langs, string culture) =>
        langs?.Any(x => x.IsDefault && x.IsoCode.InvariantEquals(culture)) ?? false;

    private bool IsMandatoryCulture(IReadOnlyCollection<ILanguage> langs, string culture) =>
        langs.Any(x => x.IsMandatory && x.IsoCode.InvariantEquals(culture));

    #endregion
}
