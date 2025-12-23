# ContentService Refactoring Phase 5: Publish Operation Service Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Extract all publishing operations (Publish, Unpublish, scheduled publishing, branch publishing, schedule management) from ContentService into a dedicated IContentPublishOperationService.

**Architecture:** Create a new IContentPublishOperationService interface and ContentPublishOperationService implementation that handles all publish/unpublish operations. The ContentService facade will delegate to this new service. The new service inherits from ContentServiceBase and follows the established pattern from Phases 1-4.

**Tech Stack:** C#, .NET 10.0, Umbraco CMS Core/Infrastructure, NUnit for testing

---

## Phase Overview

This is the most complex phase of the refactoring. The publishing logic includes:
- Complex culture-variant handling (CommitDocumentChangesInternal ~330 lines)
- Scheduled publishing/expiration operations
- Branch publishing with tree traversal
- Strategy pattern methods (StrategyCanPublish, StrategyPublish, StrategyCanUnpublish, StrategyUnpublish)
- Multiple notification types

**Key Decisions:**
1. Keep `MoveToRecycleBin` in facade (orchestrates unpublish + move)
2. The new service name is `IContentPublishOperationService` to avoid collision with existing `IContentPublishingService` (API layer)
3. Complex methods like `CommitDocumentChangesInternal` stay intact - no further splitting
4. **Expose `CommitDocumentChanges` on interface** (Option A from critical review) - allows facade to orchestrate publish/unpublish with other operations (e.g., MoveToRecycleBin)
5. Keep `GetPublishedDescendants` internal - facade uses `CommitDocumentChanges` which handles descendants internally
6. Add optional `notificationState` parameter to `CommitDocumentChanges` for state propagation across orchestrated operations

---

## Methods to Extract

| Method | Lines (approx) | Notes |
|--------|----------------|-------|
| `Publish` | 85 | Core publish operation |
| `Unpublish` | 65 | Core unpublish operation |
| `CommitDocumentChanges` | 20 | Public wrapper |
| `CommitDocumentChangesInternal` | 330 | Core publishing logic |
| `PerformScheduledPublish` | 10 | Entry point for scheduled jobs |
| `PerformScheduledPublishingExpiration` | 70 | Handle expirations |
| `PerformScheduledPublishingRelease` | 115 | Handle releases |
| `PublishBranch` (public) | 50 | Entry point for branch publish |
| `PublishBranch` (internal) | 115 | Core branch logic |
| `PublishBranchItem` | 55 | Individual item in branch |
| `PublishBranch_PublishCultures` | 20 | Utility |
| `PublishBranch_ShouldPublish` | 25 | Utility |
| `EnsureCultures` | 10 | Utility |
| `ProvidedCulturesIndicatePublishAll` | 2 | Utility |
| `SendToPublication` | 55 | Workflow trigger |
| `GetPublishedChildren` | 10 | Read operation |
| `GetPublishedDescendants` | 10 | Internal read |
| `GetPublishedDescendantsLocked` | 20 | Internal read locked |
| `StrategyCanPublish` | 175 | Publishing strategy |
| `StrategyPublish` | 60 | Publishing strategy |
| `StrategyCanUnpublish` | 25 | Unpublishing strategy |
| `StrategyUnpublish` | 40 | Unpublishing strategy |
| `GetContentScheduleByContentId` (int) | 10 | Schedule retrieval |
| `GetContentScheduleByContentId` (Guid) | 10 | Schedule retrieval |
| `PersistContentSchedule` | 10 | Schedule persistence |
| `GetContentSchedulesByIds` | 25 | Bulk schedule retrieval |
| `GetContentForExpiration` | 10 | Schedule query |
| `GetContentForRelease` | 10 | Schedule query |
| `IsPathPublishable` | 15 | Path checks |
| `IsPathPublished` | 10 | Path checks |
| Helper methods | 40 | `HasUnsavedChanges`, `GetLanguageDetailsForAuditEntry`, `IsDefaultCulture`, `IsMandatoryCulture` |

**Estimated Total:** ~1,500 lines (ContentService will reduce from ~3000 to ~1500 lines)

---

## Task 1: Create IContentPublishOperationService Interface

**Files:**
- Create: `src/Umbraco.Core/Services/IContentPublishOperationService.cs`

**Step 1: Write the interface file**

```csharp
// src/Umbraco.Core/Services/IContentPublishOperationService.cs
using System.ComponentModel;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Querying;

namespace Umbraco.Cms.Core.Services;

/// <summary>
/// Service for content publishing operations (publish, unpublish, scheduled publishing, branch publishing).
/// </summary>
/// <remarks>
/// <para>
/// <strong>Implementation Note:</strong> Do not implement this interface directly.
/// Instead, inherit from <see cref="ContentServiceBase"/> which provides required
/// infrastructure (scoping, repository access, auditing). Direct implementation
/// without this base class will result in missing functionality.
/// </para>
/// <para>
/// This interface is part of the ContentService refactoring initiative (Phase 5).
/// It extracts publishing operations into a focused, testable service.
/// </para>
/// <para>
/// <strong>Note:</strong> This interface is named IContentPublishOperationService to avoid
/// collision with the existing IContentPublishingService which is an API-layer orchestrator.
/// </para>
/// <para>
/// <strong>Versioning Policy:</strong> This interface follows additive-only changes.
/// New methods may be added with default implementations. Existing methods will not
/// be removed or have signatures changed without a 2 major version deprecation period.
/// </para>
/// </remarks>
public interface IContentPublishOperationService : IService
{
    #region Publishing

    /// <summary>
    /// Publishes a document.
    /// </summary>
    /// <param name="content">The document to publish.</param>
    /// <param name="cultures">The cultures to publish. Use "*" for all cultures or specific culture codes.</param>
    /// <param name="userId">The identifier of the user performing the action.</param>
    /// <returns>The publish result indicating success or failure.</returns>
    /// <remarks>
    /// <para>When a culture is being published, it includes all varying values along with all invariant values.</para>
    /// <para>Wildcards (*) can be used as culture identifier to publish all cultures.</para>
    /// <para>An empty array (or a wildcard) can be passed for culture invariant content.</para>
    /// <para>Fires ContentPublishingNotification (cancellable) before publish and ContentPublishedNotification after.</para>
    /// </remarks>
    PublishResult Publish(IContent content, string[] cultures, int userId = Constants.Security.SuperUserId);

    /// <summary>
    /// Publishes a document branch.
    /// </summary>
    /// <param name="content">The root document of the branch.</param>
    /// <param name="publishBranchFilter">Options for force publishing unpublished or re-publishing unchanged content.</param>
    /// <param name="cultures">The cultures to publish.</param>
    /// <param name="userId">The identifier of the user performing the operation.</param>
    /// <returns>Results for each document in the branch.</returns>
    /// <remarks>The root of the branch is always published, regardless of <paramref name="publishBranchFilter"/>.</remarks>
    IEnumerable<PublishResult> PublishBranch(IContent content, PublishBranchFilter publishBranchFilter, string[] cultures, int userId = Constants.Security.SuperUserId);

    #endregion

    #region Unpublishing

    /// <summary>
    /// Unpublishes a document.
    /// </summary>
    /// <param name="content">The document to unpublish.</param>
    /// <param name="culture">The culture to unpublish, or "*" for all cultures.</param>
    /// <param name="userId">The identifier of the user performing the action.</param>
    /// <returns>The publish result indicating success or failure.</returns>
    /// <remarks>
    /// <para>By default, unpublishes the document as a whole, but it is possible to specify a culture.</para>
    /// <para>If the content type is variant, culture can be either '*' or an actual culture.</para>
    /// <para>If the content type is invariant, culture can be either '*' or null or empty.</para>
    /// <para>Fires ContentUnpublishingNotification (cancellable) before and ContentUnpublishedNotification after.</para>
    /// </remarks>
    PublishResult Unpublish(IContent content, string? culture = "*", int userId = Constants.Security.SuperUserId);

    #endregion

    #region Document Changes (Advanced API)

    /// <summary>
    /// Commits pending document publishing/unpublishing changes.
    /// </summary>
    /// <param name="content">The document with pending publish state changes.</param>
    /// <param name="userId">The identifier of the user performing the action.</param>
    /// <param name="notificationState">Optional state dictionary for notification propagation across orchestrated operations.</param>
    /// <returns>The publish result indicating success or failure.</returns>
    /// <remarks>
    /// <para>
    /// <strong>This is an advanced API.</strong> Most consumers should use <see cref="Publish"/> or
    /// <see cref="Unpublish"/> instead.
    /// </para>
    /// <para>
    /// Call this after setting <see cref="IContent.PublishedState"/> to
    /// <see cref="PublishedState.Publishing"/> or <see cref="PublishedState.Unpublishing"/>.
    /// </para>
    /// <para>
    /// This method is exposed for orchestration scenarios where publish/unpublish must be coordinated
    /// with other operations (e.g., MoveToRecycleBin unpublishes before moving).
    /// </para>
    /// </remarks>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    PublishResult CommitDocumentChanges(IContent content, int userId = Constants.Security.SuperUserId, IDictionary<string, object?>? notificationState = null);

    #endregion

    #region Scheduled Publishing

    /// <summary>
    /// Publishes and unpublishes scheduled documents.
    /// </summary>
    /// <param name="date">The date to check schedules against.</param>
    /// <returns>Results for each processed document.</returns>
    IEnumerable<PublishResult> PerformScheduledPublish(DateTime date);

    /// <summary>
    /// Gets documents having an expiration date before (lower than, or equal to) a specified date.
    /// </summary>
    /// <param name="date">The date to check against.</param>
    /// <returns>Documents scheduled for expiration.</returns>
    IEnumerable<IContent> GetContentForExpiration(DateTime date);

    /// <summary>
    /// Gets documents having a release date before (lower than, or equal to) a specified date.
    /// </summary>
    /// <param name="date">The date to check against.</param>
    /// <returns>Documents scheduled for release.</returns>
    IEnumerable<IContent> GetContentForRelease(DateTime date);

    #endregion

    #region Schedule Management

    /// <summary>
    /// Gets publish/unpublish schedule for a content node by integer id.
    /// </summary>
    /// <param name="contentId">Id of the content to load schedule for.</param>
    /// <returns>The content schedule collection.</returns>
    ContentScheduleCollection GetContentScheduleByContentId(int contentId);

    /// <summary>
    /// Gets publish/unpublish schedule for a content node by GUID.
    /// </summary>
    /// <param name="contentId">Key of the content to load schedule for.</param>
    /// <returns>The content schedule collection.</returns>
    ContentScheduleCollection GetContentScheduleByContentId(Guid contentId);

    /// <summary>
    /// Persists publish/unpublish schedule for a content node.
    /// </summary>
    /// <param name="content">The content item.</param>
    /// <param name="contentSchedule">The schedule to persist.</param>
    void PersistContentSchedule(IContent content, ContentScheduleCollection contentSchedule);

    /// <summary>
    /// Gets a dictionary of content Ids and their matching content schedules.
    /// </summary>
    /// <param name="keys">The content keys.</param>
    /// <returns>A dictionary with nodeId and an IEnumerable of matching ContentSchedules.</returns>
    IDictionary<int, IEnumerable<ContentSchedule>> GetContentSchedulesByIds(Guid[] keys);

    #endregion

    #region Path Checks

    /// <summary>
    /// Gets a value indicating whether a document is path-publishable.
    /// </summary>
    /// <param name="content">The content to check.</param>
    /// <returns>True if all ancestors are published.</returns>
    /// <remarks>A document is path-publishable when all its ancestors are published.</remarks>
    bool IsPathPublishable(IContent content);

    /// <summary>
    /// Gets a value indicating whether a document is path-published.
    /// </summary>
    /// <param name="content">The content to check.</param>
    /// <returns>True if all ancestors and the document itself are published.</returns>
    /// <remarks>A document is path-published when all its ancestors, and the document itself, are published.</remarks>
    bool IsPathPublished(IContent? content);

    #endregion

    #region Workflow

    /// <summary>
    /// Saves a document and raises the "sent to publication" events.
    /// </summary>
    /// <param name="content">The content to send to publication.</param>
    /// <param name="userId">The identifier of the user issuing the send to publication.</param>
    /// <returns>True if sending publication was successful otherwise false.</returns>
    /// <remarks>
    /// Fires ContentSendingToPublishNotification (cancellable) before and ContentSentToPublishNotification after.
    /// </remarks>
    bool SendToPublication(IContent? content, int userId = Constants.Security.SuperUserId);

    #endregion

    #region Published Content Queries

    /// <summary>
    /// Gets published children of a parent content item.
    /// </summary>
    /// <param name="id">Id of the parent to retrieve children from.</param>
    /// <returns>Published child content items, ordered by sort order.</returns>
    IEnumerable<IContent> GetPublishedChildren(int id);

    #endregion
}
```

**Step 2: Verify the file compiles**

Run: `dotnet build src/Umbraco.Core/Umbraco.Core.csproj --no-restore`
Expected: Build succeeded

**Step 3: Commit**

```bash
git add src/Umbraco.Core/Services/IContentPublishOperationService.cs
git commit -m "feat(core): add IContentPublishOperationService interface for Phase 5

Defines interface for content publishing operations:
- Publish/Unpublish operations
- Scheduled publishing (release/expiration)
- Schedule management
- Path checks
- Workflow (SendToPublication)
- Published content queries

Part of ContentService refactoring Phase 5."
```

---

## Task 2: Create ContentPublishOperationService Implementation

**Files:**
- Create: `src/Umbraco.Core/Services/ContentPublishOperationService.cs`

This is the largest task. We'll extract all publishing methods from ContentService.

**Step 1: Create the implementation file with constructor and dependencies**

```csharp
// src/Umbraco.Core/Services/ContentPublishOperationService.cs
using System.Collections.Immutable;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
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

    // Implementation methods will be added in subsequent steps
}
```

**Step 2: Copy and adapt the Publish method**

Add to the class:

```csharp
    #region Publishing

    /// <inheritdoc />
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

        // we need to guard against unsaved changes before proceeding
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
                $"Cannot save-and-publish (un)publishing content, use the dedicated {nameof(CommitDocumentChangesInternal)} method.");
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
```

**Step 3: Add remaining publishing methods**

Continue adding all the methods from ContentService. Due to size, I'll document the key structure:

```csharp
    #region Unpublishing

    /// <inheritdoc />
    public PublishResult Unpublish(IContent content, string? culture = "*", int userId = Constants.Security.SuperUserId)
    {
        // Copy exact implementation from ContentService lines 918-1010
    }

    #endregion

    #region Commit Document Changes (Advanced API)

    /// <inheritdoc />
    /// <remarks>
    /// This is the public wrapper for CommitDocumentChangesInternal.
    /// Exposed on interface for orchestration scenarios (e.g., MoveToRecycleBin).
    /// </remarks>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public PublishResult CommitDocumentChanges(
        IContent content,
        int userId = Constants.Security.SuperUserId,
        IDictionary<string, object?>? notificationState = null)
    {
        if (content == null)
        {
            throw new ArgumentNullException(nameof(content));
        }

        EventMessages evtMsgs = EventMessagesFactory.Get();

        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            scope.WriteLock(Constants.Locks.ContentTree);
            var allLangs = _languageRepository.GetMany().ToList();

            // Use provided notification state or create new one
            var state = notificationState ?? new Dictionary<string, object?>();

            PublishResult result = CommitDocumentChangesInternal(
                scope, content, evtMsgs, allLangs, state, userId);
            scope.Complete();
            return result;
        }
    }

    /// <summary>
    /// Handles a lot of business logic cases for how the document should be persisted.
    /// </summary>
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
        // Copy exact implementation from ContentService lines 1080-1406
        // This is ~330 lines - the core publishing logic
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

        // Critical Review Q4: Add explicit logging for failures
        var failures = results.Where(r => !r.Success).ToList();
        if (failures.Count > 0)
        {
            foreach (var failure in failures)
            {
                _logger.LogWarning(
                    "Scheduled publish failed for content {ContentId} ({ContentName}): {ResultType}",
                    failure.Content?.Id,
                    failure.Content?.Name,
                    failure.Result);
            }

            _logger.LogWarning(
                "Scheduled publish completed with {FailureCount} failures out of {TotalCount} items",
                failures.Count,
                results.Count);
        }

        return results;
    }

    private void PerformScheduledPublishingExpiration(DateTime date, List<PublishResult> results, EventMessages evtMsgs, Lazy<List<ILanguage>> allLangs)
    {
        // Copy from ContentService lines 1421-1491
    }

    private void PerformScheduledPublishingRelease(DateTime date, List<PublishResult> results, EventMessages evtMsgs, Lazy<List<ILanguage>> allLangs)
    {
        // Copy from ContentService lines 1493-1610
    }

    /// <inheritdoc />
    public IEnumerable<IContent> GetContentForExpiration(DateTime date)
    {
        // Copy from ContentService lines 708-715
    }

    /// <inheritdoc />
    public IEnumerable<IContent> GetContentForRelease(DateTime date)
    {
        // Copy from ContentService lines 718-725
    }

    #endregion

    #region Branch Publishing

    private bool PublishBranch_PublishCultures(IContent content, HashSet<string> culturesToPublish, IReadOnlyCollection<ILanguage> allLangs)
    {
        // Copy from ContentService lines 1613-1631
    }

    private static HashSet<string>? PublishBranch_ShouldPublish(ref HashSet<string>? cultures, string c, bool published, bool edited, bool isRoot, PublishBranchFilter publishBranchFilter)
    {
        // Copy from ContentService lines 1634-1659
    }

    /// <inheritdoc />
    public IEnumerable<PublishResult> PublishBranch(IContent content, PublishBranchFilter publishBranchFilter, string[] cultures, int userId = Constants.Security.SuperUserId)
    {
        // Copy from ContentService lines 1662-1713
    }

    private static string[] EnsureCultures(IContent content, string[] cultures)
    {
        // Copy from ContentService lines 1715-1724
    }

    private static bool ProvidedCulturesIndicatePublishAll(string[] cultures) => cultures.Length == 0 || (cultures.Length == 1 && cultures[0] == "invariant");

    internal IEnumerable<PublishResult> PublishBranch(
        IContent document,
        Func<IContent, HashSet<string>?> shouldPublish,
        Func<IContent, HashSet<string>, IReadOnlyCollection<ILanguage>, bool> publishCultures,
        int userId = Constants.Security.SuperUserId)
    {
        // Copy from ContentService lines 1728-1842
    }

    private PublishResult? PublishBranchItem(
        ICoreScope scope,
        IContent document,
        HashSet<string>? culturesToPublish,
        Func<IContent, HashSet<string>, IReadOnlyCollection<ILanguage>, bool> publishCultures,
        bool isRoot,
        ICollection<IContent> publishedDocuments,
        EventMessages evtMsgs,
        int userId,
        IReadOnlyCollection<ILanguage> allLangs,
        out IDictionary<string, object?>? initialNotificationState)
    {
        // Copy from ContentService lines 1847-1900
    }

    #endregion

    #region Schedule Management

    /// <inheritdoc />
    public ContentScheduleCollection GetContentScheduleByContentId(int contentId)
    {
        // Copy from ContentService lines 497-504
    }

    /// <inheritdoc />
    public ContentScheduleCollection GetContentScheduleByContentId(Guid contentId)
    {
        // Copy from ContentService lines 506-515
    }

    /// <inheritdoc />
    public void PersistContentSchedule(IContent content, ContentScheduleCollection contentSchedule)
    {
        // Copy from ContentService lines 518-526
    }

    /// <inheritdoc />
    public IDictionary<int, IEnumerable<ContentSchedule>> GetContentSchedulesByIds(Guid[] keys)
    {
        // Critical Review fix 2.4: Add null/empty check
        ArgumentNullException.ThrowIfNull(keys);
        if (keys.Length == 0)
        {
            return new Dictionary<int, IEnumerable<ContentSchedule>>();
        }

        // Copy from ContentService lines 759-783
    }

    #endregion

    #region Path Checks

    /// <inheritdoc />
    public bool IsPathPublishable(IContent content)
    {
        // Fast path for root content
        if (content.ParentId == Constants.System.Root)
        {
            return true; // root content is always publishable
        }

        if (content.Trashed)
        {
            return false; // trashed content is never publishable
        }

        // Critical Review fix 2.2: Use _crudService to avoid circular dependency
        // Not trashed and has a parent: publishable if the parent is path-published
        IContent? parent = GetParent(content);
        return parent == null || IsPathPublished(parent);
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
    public bool SendToPublication(IContent? content, int userId = Constants.Security.SuperUserId)
    {
        // Copy from ContentService lines 2155-2207
        // Note: Replace Save call with _crudService.Save
    }

    #endregion

    #region Published Content Queries

    /// <inheritdoc />
    public IEnumerable<IContent> GetPublishedChildren(int id)
    {
        // Copy from ContentService lines 622-630
    }

    /// <summary>
    /// Gets a collection of published descendants.
    /// </summary>
    internal IEnumerable<IContent> GetPublishedDescendants(IContent content)
    {
        // Copy from ContentService lines 2270-2277
    }

    internal IEnumerable<IContent> GetPublishedDescendantsLocked(IContent content)
    {
        // Copy from ContentService lines 2279-2301
    }

    #endregion

    #region Publishing Strategies

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
        // Copy from ContentService lines 2356-2527
    }

    private PublishResult StrategyPublish(
        IContent content,
        IReadOnlyCollection<string>? culturesPublishing,
        IReadOnlyCollection<string>? culturesUnpublishing,
        EventMessages evtMsgs)
    {
        // Copy from ContentService lines 2541-2591
    }

    private PublishResult StrategyCanUnpublish(
        ICoreScope scope,
        IContent content,
        EventMessages evtMsgs,
        IDictionary<string, object?>? notificationState)
    {
        // Copy from ContentService lines 2601-2618
    }

    private PublishResult StrategyUnpublish(IContent content, EventMessages evtMsgs)
    {
        // Copy from ContentService lines 2631-2665
    }

    #endregion

    #region Helper Methods

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

    /// <summary>
    /// Gets paged descendants - required for branch operations.
    /// </summary>
    private IEnumerable<IContent> GetPagedDescendants(int id, long pageIndex, int pageSize, out long totalRecords, Ordering? ordering = null)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
        scope.ReadLock(Constants.Locks.ContentTree);

        IQuery<IContent>? query = Query<IContent>();
        IContent? content = DocumentRepository.Get(id);
        if (content != null && !content.Path.IsNullOrWhiteSpace())
        {
            var path = content.Path + ",";
            query?.Where(x => x.Path.SqlStartsWith(path, TextColumnType.NVarchar));
        }

        return DocumentRepository.GetPage(query, pageIndex, pageSize, out totalRecords, null, ordering ?? Ordering.By("Path"));
    }

    /// <summary>
    /// Checks if content has children - required for publishing operations.
    /// </summary>
    private bool HasChildren(int id)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
        scope.ReadLock(Constants.Locks.ContentTree);
        IQuery<IContent> query = Query<IContent>().Where(x => x.ParentId == id);
        var count = DocumentRepository.Count(query);
        return count > 0;
    }

    /// <summary>
    /// Gets parent content - required for IsPathPublishable.
    /// </summary>
    private IContent? GetParent(IContent? content)
    {
        if (content?.ParentId == null || content.ParentId == Constants.System.Root)
        {
            return null;
        }

        using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
        scope.ReadLock(Constants.Locks.ContentTree);
        return DocumentRepository.Get(content.ParentId);
    }

    #endregion
```

**Step 4: Verify the implementation compiles**

Run: `dotnet build src/Umbraco.Core/Umbraco.Core.csproj --no-restore`
Expected: Build succeeded

**Step 5: Commit**

```bash
git add src/Umbraco.Core/Services/ContentPublishOperationService.cs
git commit -m "feat(core): implement ContentPublishOperationService for Phase 5

Implements all publishing operations:
- Publish/Unpublish with culture support
- CommitDocumentChangesInternal (core publishing logic)
- PerformScheduledPublish for scheduled jobs
- PublishBranch for tree publishing
- Schedule management operations
- Path checks (IsPathPublishable, IsPathPublished)
- SendToPublication workflow
- Publishing strategies (StrategyCanPublish, etc.)

Part of ContentService refactoring Phase 5."
```

---

## Task 3: Register ContentPublishOperationService in DI

**Files:**
- Modify: `src/Umbraco.Core/DependencyInjection/UmbracoBuilder.cs`

**Step 1: Add the service registration**

Add after line 304 (after IContentMoveOperationService):

```csharp
Services.AddUnique<IContentPublishOperationService, ContentPublishOperationService>();
```

**Step 2: Update ContentService factory to inject the new service**

Modify the ContentService factory registration (lines 305-327) to include the new parameter:

```csharp
Services.AddUnique<IContentService>(sp =>
    new ContentService(
        sp.GetRequiredService<ICoreScopeProvider>(),
        sp.GetRequiredService<ILoggerFactory>(),
        sp.GetRequiredService<IEventMessagesFactory>(),
        sp.GetRequiredService<IDocumentRepository>(),
        sp.GetRequiredService<IEntityRepository>(),
        sp.GetRequiredService<IAuditService>(),
        sp.GetRequiredService<IContentTypeRepository>(),
        sp.GetRequiredService<IDocumentBlueprintRepository>(),
        sp.GetRequiredService<ILanguageRepository>(),
        sp.GetRequiredService<Lazy<IPropertyValidationService>>(),
        sp.GetRequiredService<IShortStringHelper>(),
        sp.GetRequiredService<ICultureImpactFactory>(),
        sp.GetRequiredService<IUserIdKeyResolver>(),
        sp.GetRequiredService<PropertyEditorCollection>(),
        sp.GetRequiredService<IIdKeyMap>(),
        sp.GetRequiredService<IOptionsMonitor<ContentSettings>>(),
        sp.GetRequiredService<IRelationService>(),
        sp.GetRequiredService<IContentCrudService>(),
        sp.GetRequiredService<IContentQueryOperationService>(),
        sp.GetRequiredService<IContentVersionOperationService>(),
        sp.GetRequiredService<IContentMoveOperationService>(),
        sp.GetRequiredService<IContentPublishOperationService>()));  // NEW
```

**Step 3: Verify compilation**

Run: `dotnet build src/Umbraco.Core/Umbraco.Core.csproj --no-restore`
Expected: Build succeeded

**Step 4: Commit**

```bash
git add src/Umbraco.Core/DependencyInjection/UmbracoBuilder.cs
git commit -m "feat(di): register IContentPublishOperationService

Adds DI registration for the new publish operation service
and updates ContentService factory to inject it.

Part of ContentService refactoring Phase 5."
```

---

## Task 4: Update ContentService to Accept IContentPublishOperationService

**Files:**
- Modify: `src/Umbraco.Core/Services/ContentService.cs`

**Step 1: Add field and property for the publish operation service**

Add after line 64 (after move operation service fields):

```csharp
    // Publish operation service fields (for Phase 5 extracted publish operations)
    private readonly IContentPublishOperationService? _publishOperationService;
    private readonly Lazy<IContentPublishOperationService>? _publishOperationServiceLazy;

    /// <summary>
    /// Gets the publish operation service.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the service was not properly initialized.</exception>
    private IContentPublishOperationService PublishOperationService =>
        _publishOperationService ?? _publishOperationServiceLazy?.Value
        ?? throw new InvalidOperationException("PublishOperationService not initialized. Ensure the service is properly injected via constructor.");
```

**Step 2: Update the primary constructor to accept IContentPublishOperationService**

Modify the primary constructor (starting at line 92) to add the new parameter:

```csharp
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
        IContentQueryOperationService queryOperationService,
        IContentVersionOperationService versionOperationService,
        IContentMoveOperationService moveOperationService,
        IContentPublishOperationService publishOperationService)  // NEW PARAMETER
        : base(provider, loggerFactory, eventMessagesFactory)
    {
        // ... existing field assignments ...

        // Phase 5: Publish operation service (direct injection)
        ArgumentNullException.ThrowIfNull(publishOperationService);
        _publishOperationService = publishOperationService;
        _publishOperationServiceLazy = null;  // Not needed when directly injected
    }
```

**Step 3: Update obsolete constructors to lazy-resolve the new service**

Add to both obsolete constructors (around lines 156 and 222):

```csharp
        // Phase 5: Lazy resolution of IContentPublishOperationService
        _publishOperationServiceLazy = new Lazy<IContentPublishOperationService>(() =>
            StaticServiceProvider.Instance.GetRequiredService<IContentPublishOperationService>(),
            LazyThreadSafetyMode.ExecutionAndPublication);
```

**Step 4: Verify compilation**

Run: `dotnet build src/Umbraco.Core/Umbraco.Core.csproj --no-restore`
Expected: Build succeeded

**Step 5: Commit**

```bash
git add src/Umbraco.Core/Services/ContentService.cs
git commit -m "refactor(core): add IContentPublishOperationService injection to ContentService

Adds field, property, and constructor parameter for the new
publish operation service. Obsolete constructors use lazy
resolution for backward compatibility.

Part of ContentService refactoring Phase 5."
```

---

## Task 5: Delegate Publishing Methods from ContentService

**Files:**
- Modify: `src/Umbraco.Core/Services/ContentService.cs`

This is a large task - we need to replace method implementations with delegations.

**Step 1: Replace Publish method**

Find the `Publish` method (around line 830) and replace with:

```csharp
    /// <inheritdoc/>
    public PublishResult Publish(IContent content, string[] cultures, int userId = Constants.Security.SuperUserId)
        => PublishOperationService.Publish(content, cultures, userId);
```

**Step 2: Replace Unpublish method**

Find the `Unpublish` method (around line 918) and replace with:

```csharp
    /// <inheritdoc />
    public PublishResult Unpublish(IContent content, string? culture = "*", int userId = Constants.Security.SuperUserId)
        => PublishOperationService.Unpublish(content, culture, userId);
```

**Step 3: Replace schedule management methods**

Replace `GetContentScheduleByContentId` (int overload):

```csharp
    /// <inheritdoc />
    public ContentScheduleCollection GetContentScheduleByContentId(int contentId)
        => PublishOperationService.GetContentScheduleByContentId(contentId);
```

Replace `GetContentScheduleByContentId` (Guid overload):

```csharp
    public ContentScheduleCollection GetContentScheduleByContentId(Guid contentId)
        => PublishOperationService.GetContentScheduleByContentId(contentId);
```

Replace `PersistContentSchedule`:

```csharp
    /// <inheritdoc />
    public void PersistContentSchedule(IContent content, ContentScheduleCollection contentSchedule)
        => PublishOperationService.PersistContentSchedule(content, contentSchedule);
```

Replace `GetContentSchedulesByIds`:

```csharp
    /// <inheritdoc/>
    public IDictionary<int, IEnumerable<ContentSchedule>> GetContentSchedulesByIds(Guid[] keys)
        => PublishOperationService.GetContentSchedulesByIds(keys);
```

**Step 4: Replace scheduled publishing methods**

Replace `PerformScheduledPublish`:

```csharp
    /// <inheritdoc />
    public IEnumerable<PublishResult> PerformScheduledPublish(DateTime date)
        => PublishOperationService.PerformScheduledPublish(date);
```

Replace `GetContentForExpiration`:

```csharp
    /// <inheritdoc />
    public IEnumerable<IContent> GetContentForExpiration(DateTime date)
        => PublishOperationService.GetContentForExpiration(date);
```

Replace `GetContentForRelease`:

```csharp
    /// <inheritdoc />
    public IEnumerable<IContent> GetContentForRelease(DateTime date)
        => PublishOperationService.GetContentForRelease(date);
```

**Step 5: Replace path check methods**

Replace `IsPathPublishable`:

```csharp
    /// <inheritdoc />
    public bool IsPathPublishable(IContent content)
        => PublishOperationService.IsPathPublishable(content);
```

Replace `IsPathPublished`:

```csharp
    public bool IsPathPublished(IContent? content)
        => PublishOperationService.IsPathPublished(content);
```

**Step 6: Replace branch publishing**

Replace `PublishBranch`:

```csharp
    /// <inheritdoc />
    public IEnumerable<PublishResult> PublishBranch(IContent content, PublishBranchFilter publishBranchFilter, string[] cultures, int userId = Constants.Security.SuperUserId)
        => PublishOperationService.PublishBranch(content, publishBranchFilter, cultures, userId);
```

**Step 7: Replace workflow methods**

Replace `SendToPublication`:

```csharp
    /// <inheritdoc />
    public bool SendToPublication(IContent? content, int userId = Constants.Security.SuperUserId)
        => PublishOperationService.SendToPublication(content, userId);
```

**Step 8: Replace GetPublishedChildren**

```csharp
    public IEnumerable<IContent> GetPublishedChildren(int id)
        => PublishOperationService.GetPublishedChildren(id);
```

**Step 9: Remove the now-unused private methods**

Delete the following methods from ContentService (they now live in ContentPublishOperationService):

**Definitely delete (moved to ContentPublishOperationService):**
- `CommitDocumentChanges` (internal) - now on interface
- `CommitDocumentChangesInternal` (private)
- `PerformScheduledPublishingExpiration` (private)
- `PerformScheduledPublishingRelease` (private)
- `PublishBranch_PublishCultures` (private)
- `PublishBranch_ShouldPublish` (private)
- `EnsureCultures` (private)
- `ProvidedCulturesIndicatePublishAll` (private)
- `PublishBranch` internal overload
- `PublishBranchItem` (private)
- `StrategyCanPublish` (private)
- `StrategyPublish` (private)
- `StrategyCanUnpublish` (private)
- `StrategyUnpublish` (private)
- `IsDefaultCulture` (private)
- `IsMandatoryCulture` (private)

**Delete (no longer needed - facade uses CommitDocumentChanges which handles internally):**
- `GetPublishedDescendants` (internal) - Key Decision #5: CommitDocumentChanges handles descendants internally
- `GetPublishedDescendantsLocked` (internal) - Called by CommitDocumentChangesInternal, not needed in facade

**Conditional delete (verify MoveToRecycleBin doesn't use directly):**
- `HasUnsavedChanges` (private) - If MoveToRecycleBin doesn't check this, delete
- `GetLanguageDetailsForAuditEntry` (private) - If no remaining callers, delete

**Step 10: Verify compilation**

Run: `dotnet build src/Umbraco.Core/Umbraco.Core.csproj --no-restore`
Expected: Build succeeded

**Step 11: Run tests to verify behavior unchanged**

Run: `dotnet test tests/Umbraco.Tests.Integration --filter "FullyQualifiedName~ContentService" --no-restore`
Expected: All tests pass

**Step 12: Commit**

```bash
git add src/Umbraco.Core/Services/ContentService.cs
git commit -m "refactor(core): delegate publish operations to ContentPublishOperationService

Replaces publishing method implementations with delegations:
- Publish/Unpublish
- PerformScheduledPublish
- PublishBranch
- GetContentScheduleByContentId
- PersistContentSchedule
- GetContentSchedulesByIds
- GetContentForExpiration/Release
- IsPathPublishable/IsPathPublished
- SendToPublication
- GetPublishedChildren

Removes ~1500 lines of implementation that now lives in
ContentPublishOperationService.

Part of ContentService refactoring Phase 5."
```

---

## Task 6: Add Interface Contract Tests

**Files:**
- Create: `tests/Umbraco.Tests.UnitTests/Umbraco.Core/Services/ContentPublishOperationServiceContractTests.cs`

**Step 1: Create interface contract test file**

```csharp
using System.ComponentModel;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Services;

/// <summary>
/// Contract tests for IContentPublishOperationService interface.
/// These tests verify interface design and expected behaviors.
/// </summary>
[TestFixture]
public class ContentPublishOperationServiceContractTests
{
    [Test]
    public void IContentPublishOperationService_Inherits_From_IService()
    {
        // Assert
        Assert.That(typeof(IService).IsAssignableFrom(typeof(IContentPublishOperationService)), Is.True);
    }

    [Test]
    public void Publish_Method_Exists_With_Expected_Signature()
    {
        // Arrange
        var methodInfo = typeof(IContentPublishOperationService).GetMethod(
            nameof(IContentPublishOperationService.Publish),
            new[] { typeof(IContent), typeof(string[]), typeof(int) });

        // Assert
        Assert.That(methodInfo, Is.Not.Null);
        Assert.That(methodInfo!.ReturnType, Is.EqualTo(typeof(PublishResult)));
    }

    [Test]
    public void Unpublish_Method_Exists_With_Expected_Signature()
    {
        // Arrange
        var methodInfo = typeof(IContentPublishOperationService).GetMethod(
            nameof(IContentPublishOperationService.Unpublish),
            new[] { typeof(IContent), typeof(string), typeof(int) });

        // Assert
        Assert.That(methodInfo, Is.Not.Null);
        Assert.That(methodInfo!.ReturnType, Is.EqualTo(typeof(PublishResult)));
    }

    [Test]
    public void PublishBranch_Method_Exists_With_Expected_Signature()
    {
        // Arrange
        var methodInfo = typeof(IContentPublishOperationService).GetMethod(
            nameof(IContentPublishOperationService.PublishBranch),
            new[] { typeof(IContent), typeof(PublishBranchFilter), typeof(string[]), typeof(int) });

        // Assert
        Assert.That(methodInfo, Is.Not.Null);
        Assert.That(methodInfo!.ReturnType, Is.EqualTo(typeof(IEnumerable<PublishResult>)));
    }

    [Test]
    public void PerformScheduledPublish_Method_Exists_With_Expected_Signature()
    {
        // Arrange
        var methodInfo = typeof(IContentPublishOperationService).GetMethod(
            nameof(IContentPublishOperationService.PerformScheduledPublish),
            new[] { typeof(DateTime) });

        // Assert
        Assert.That(methodInfo, Is.Not.Null);
        Assert.That(methodInfo!.ReturnType, Is.EqualTo(typeof(IEnumerable<PublishResult>)));
    }

    [Test]
    public void GetContentScheduleByContentId_IntOverload_Exists()
    {
        // Arrange
        var methodInfo = typeof(IContentPublishOperationService).GetMethod(
            nameof(IContentPublishOperationService.GetContentScheduleByContentId),
            new[] { typeof(int) });

        // Assert
        Assert.That(methodInfo, Is.Not.Null);
        Assert.That(methodInfo!.ReturnType, Is.EqualTo(typeof(ContentScheduleCollection)));
    }

    [Test]
    public void GetContentScheduleByContentId_GuidOverload_Exists()
    {
        // Arrange
        var methodInfo = typeof(IContentPublishOperationService).GetMethod(
            nameof(IContentPublishOperationService.GetContentScheduleByContentId),
            new[] { typeof(Guid) });

        // Assert
        Assert.That(methodInfo, Is.Not.Null);
        Assert.That(methodInfo!.ReturnType, Is.EqualTo(typeof(ContentScheduleCollection)));
    }

    [Test]
    public void IsPathPublishable_Method_Exists()
    {
        // Arrange
        var methodInfo = typeof(IContentPublishOperationService).GetMethod(
            nameof(IContentPublishOperationService.IsPathPublishable),
            new[] { typeof(IContent) });

        // Assert
        Assert.That(methodInfo, Is.Not.Null);
        Assert.That(methodInfo!.ReturnType, Is.EqualTo(typeof(bool)));
    }

    [Test]
    public void IsPathPublished_Method_Exists()
    {
        // Arrange
        var methodInfo = typeof(IContentPublishOperationService).GetMethod(
            nameof(IContentPublishOperationService.IsPathPublished),
            new[] { typeof(IContent) });

        // Assert
        Assert.That(methodInfo, Is.Not.Null);
        Assert.That(methodInfo!.ReturnType, Is.EqualTo(typeof(bool)));
    }

    [Test]
    public void SendToPublication_Method_Exists()
    {
        // Arrange
        var methodInfo = typeof(IContentPublishOperationService).GetMethod(
            nameof(IContentPublishOperationService.SendToPublication),
            new[] { typeof(IContent), typeof(int) });

        // Assert
        Assert.That(methodInfo, Is.Not.Null);
        Assert.That(methodInfo!.ReturnType, Is.EqualTo(typeof(bool)));
    }

    [Test]
    public void CommitDocumentChanges_Method_Exists_With_NotificationState_Parameter()
    {
        // Arrange - Critical Review Option A: Exposed for orchestration
        var methodInfo = typeof(IContentPublishOperationService).GetMethod(
            nameof(IContentPublishOperationService.CommitDocumentChanges),
            new[] { typeof(IContent), typeof(int), typeof(IDictionary<string, object?>) });

        // Assert
        Assert.That(methodInfo, Is.Not.Null);
        Assert.That(methodInfo!.ReturnType, Is.EqualTo(typeof(PublishResult)));
    }

    [Test]
    public void CommitDocumentChanges_Has_EditorBrowsable_Advanced_Attribute()
    {
        // Arrange - Should be hidden from IntelliSense by default
        var methodInfo = typeof(IContentPublishOperationService).GetMethod(
            nameof(IContentPublishOperationService.CommitDocumentChanges),
            new[] { typeof(IContent), typeof(int), typeof(IDictionary<string, object?>) });

        // Act
        var attribute = methodInfo?.GetCustomAttributes(typeof(EditorBrowsableAttribute), false)
            .Cast<EditorBrowsableAttribute>()
            .FirstOrDefault();

        // Assert
        Assert.That(attribute, Is.Not.Null);
        Assert.That(attribute!.State, Is.EqualTo(EditorBrowsableState.Advanced));
    }
}
```

**Step 2: Run the contract tests**

Run: `dotnet test tests/Umbraco.Tests.UnitTests --filter "FullyQualifiedName~ContentPublishOperationServiceContractTests" --no-restore`
Expected: All tests pass

**Step 3: Commit**

```bash
git add tests/Umbraco.Tests.UnitTests/Umbraco.Core/Services/ContentPublishOperationServiceContractTests.cs
git commit -m "test(unit): add ContentPublishOperationService interface contract tests

Verifies interface design and method signatures for Phase 5.

Part of ContentService refactoring Phase 5."
```

---

## Task 7: Add Integration Tests

**Files:**
- Modify: `tests/Umbraco.Tests.Integration/Umbraco.Infrastructure/Services/ContentServiceRefactoringTests.cs`

**Step 1: Add integration test for IContentPublishOperationService DI registration**

Add to the existing test file:

```csharp
    [Test]
    public void ContentPublishOperationService_Can_Be_Resolved_From_DI()
    {
        // Act
        var publishOperationService = GetRequiredService<IContentPublishOperationService>();

        // Assert
        Assert.That(publishOperationService, Is.Not.Null);
        Assert.That(publishOperationService, Is.InstanceOf<ContentPublishOperationService>());
    }

    [Test]
    public async Task Publish_Through_ContentService_Uses_PublishOperationService()
    {
        // Arrange
        var contentService = GetRequiredService<IContentService>();
        var contentTypeService = GetRequiredService<IContentTypeService>();

        var contentType = new ContentTypeBuilder()
            .WithAlias("testPublishPage")
            .Build();
        await contentTypeService.SaveAsync(contentType, Constants.Security.SuperUserId);

        var content = contentService.Create("Test Publish Page", Constants.System.Root, contentType.Alias);
        contentService.Save(content);

        // Act
        var result = contentService.Publish(content, new[] { "*" });

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(content.Published, Is.True);
    }

    [Test]
    public async Task Unpublish_Through_ContentService_Uses_PublishOperationService()
    {
        // Arrange
        var contentService = GetRequiredService<IContentService>();
        var contentTypeService = GetRequiredService<IContentTypeService>();

        var contentType = new ContentTypeBuilder()
            .WithAlias("testUnpublishPage")
            .Build();
        await contentTypeService.SaveAsync(contentType, Constants.Security.SuperUserId);

        var content = contentService.Create("Test Unpublish Page", Constants.System.Root, contentType.Alias);
        contentService.Save(content);
        contentService.Publish(content, new[] { "*" });

        // Act
        var result = contentService.Unpublish(content);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(content.Published, Is.False);
    }

    [Test]
    public async Task IsPathPublishable_RootContent_ReturnsTrue()
    {
        // Arrange
        var contentService = GetRequiredService<IContentService>();
        var contentTypeService = GetRequiredService<IContentTypeService>();

        var contentType = new ContentTypeBuilder()
            .WithAlias("testPathPage")
            .Build();
        await contentTypeService.SaveAsync(contentType, Constants.Security.SuperUserId);

        var content = contentService.Create("Test Path Page", Constants.System.Root, contentType.Alias);
        contentService.Save(content);

        // Act
        var result = contentService.IsPathPublishable(content);

        // Assert
        Assert.That(result, Is.True);
    }
```

**Step 2: Run integration tests**

Run: `dotnet test tests/Umbraco.Tests.Integration --filter "FullyQualifiedName~ContentServiceRefactoringTests" --no-restore`
Expected: All tests pass

**Step 3: Commit**

```bash
git add tests/Umbraco.Tests.Integration/Umbraco.Infrastructure/Services/ContentServiceRefactoringTests.cs
git commit -m "test(integration): add ContentPublishOperationService integration tests

Tests DI resolution and basic publish/unpublish operations
delegated through ContentService to the new service.

Part of ContentService refactoring Phase 5."
```

---

## Task 8: Run Full Test Suite and Verify

**Step 1: Run all ContentService tests**

Run: `dotnet test tests/Umbraco.Tests.Integration --filter "FullyQualifiedName~ContentService" --no-restore`
Expected: All tests pass

**Step 2: Run refactoring-specific tests**

Run: `dotnet test tests/Umbraco.Tests.Integration --filter "FullyQualifiedName~ContentServiceRefactoringTests" --no-restore`
Expected: All tests pass

**Step 3: Run notification ordering tests (critical for publishing)**

Run: `dotnet test tests/Umbraco.Tests.Integration --filter "FullyQualifiedName~ContentNotification" --no-restore`
Expected: All tests pass

**Step 4: Commit final verification**

```bash
git tag phase-5-publish-extraction
git commit --allow-empty -m "chore: Phase 5 complete - ContentPublishOperationService extracted

Phase 5 Summary:
- Created IContentPublishOperationService interface (30+ methods)
- Created ContentPublishOperationService implementation (~1500 lines)
- Updated DI registration
- Updated ContentService to inject and delegate to new service
- All tests passing

ContentService reduced from ~3000 to ~1500 lines."
```

---

## Task 9: Update Design Document

**Files:**
- Modify: `docs/plans/2025-12-19-contentservice-refactor-design.md`

**Step 1: Update the phase status**

Change Phase 5 status from "Pending" to "Complete":

```markdown
| 5 | Publish Operation Service | All ContentService*Tests + Notification ordering tests | All pass | ✅ Complete |
```

Add to revision history:

```markdown
| 1.9 | Phase 5 complete - ContentPublishOperationService extracted |
```

**Step 2: Commit**

```bash
git add docs/plans/2025-12-19-contentservice-refactor-design.md
git commit -m "docs: mark Phase 5 complete in design document

Updates revision history and phase status."
```

---

## Summary

**Phase 5 extracts ~1,500 lines of publishing logic into a dedicated service:**

| Artifact | Description |
|----------|-------------|
| `IContentPublishOperationService` | Interface with 16 public methods (including CommitDocumentChanges) |
| `ContentPublishOperationService` | Implementation with ~1,500 lines |
| Contract Tests | 12 unit tests for interface |
| Integration Tests | 4 tests for DI and basic operations |

**After Phase 5, ContentService will be ~1,500 lines (down from ~3,000).**

**Remaining Phases:**
- Phase 6: Permission Manager (~50 lines)
- Phase 7: Blueprint Manager (~200 lines)
- Phase 8: Final Facade cleanup

---

## Revision History

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2025-12-23 | Initial plan |
| 1.1 | 2025-12-23 | Applied critical review recommendations: |
| | | - Added `CommitDocumentChanges` to interface (Option A) for facade orchestration |
| | | - Added optional `notificationState` parameter for state propagation |
| | | - Added thread-safe `ContentSettings` accessor with lock |
| | | - Added null/empty check to `GetContentSchedulesByIds` |
| | | - Added explicit failure logging in `PerformScheduledPublish` |
| | | - Clarified GetPublishedDescendants stays internal (Key Decision #5) |
| | | - Fixed IsPathPublishable to use DocumentRepository (avoid circular dependency) |
| | | - Added contract tests for CommitDocumentChanges and EditorBrowsable attribute |
| | | - Fixed test framework documentation (NUnit, not xUnit) |

---

**Plan ready for execution via `superpowers:executing-plans`.**
