using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.BackgroundJobs.Jobs.DistributedJobs;

/// <summary>
/// Background job that automatically promotes named draft branches once their
/// <see cref="IContentDraftBranch.ScheduledPublishDate"/> has passed, then hands the actual publishing off to
/// the ordinary <see cref="ScheduledPublishingJob"/>. Each due branch is first promoted exactly as a manual
/// <c>PromoteAsync</c> call would (its content copied forward onto the node's default draft via a plain
/// save, no publish, no branch-specific publish notification involved). The default draft is then given a
/// scheduled release entry (via <see cref="IPublishableContentService{TContent}.PersistContentSchedule"/>)
/// dated to when the branch itself was due, so the next run of the ordinary <see cref="ScheduledPublishingJob"/>
/// picks it up and publishes it through the same pipeline - and fires the same
/// <c>ContentPublishingNotification</c>/<c>ContentPublishedNotification</c> - as any other scheduled or
/// editorial publish would, with no awareness that a branch was ever involved.
/// </summary>
/// <remarks>
/// Scheduled draft publishing is culture-aware: the cultures to promote/queue are exactly the ones chosen (and
/// permission-checked) at the time <c>IContentDraftBranchService.Schedule</c>/<c>IElementDraftBranchService.Schedule</c>
/// was called, persisted on the branch as <see cref="IContentDraftBranch.ScheduledCultures"/> - this job has no
/// user context of its own to check permissions against, so it simply threads that already-validated set
/// through to both the promote step and the queued release, rather than re-deriving or widening it.
/// </remarks>
internal class ContentDraftBranchScheduledPublishingJob : IDistributedBackgroundJob
{
    private readonly ILogger<ContentDraftBranchScheduledPublishingJob> _logger;
    private readonly IOptionsMonitor<ScheduledPublishingSettings> _scheduledPublishingSettings;
    private readonly IContentDraftBranchRepository _contentDraftBranchRepository;
    private readonly IContentDraftBranchService _contentDraftBranchService;
    private readonly IElementDraftBranchService _elementDraftBranchService;
    private readonly IContentService _contentService;
    private readonly IElementService _elementService;
    private readonly IUserIdKeyResolver _userIdKeyResolver;
    private readonly ICoreScopeProvider _coreScopeProvider;
    private readonly IUmbracoContextFactory _umbracoContextFactory;
    private readonly IServerMessenger _serverMessenger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ContentDraftBranchScheduledPublishingJob"/> class.
    /// </summary>
    public ContentDraftBranchScheduledPublishingJob(
        ILogger<ContentDraftBranchScheduledPublishingJob> logger,
        IOptionsMonitor<ScheduledPublishingSettings> scheduledPublishingSettings,
        IContentDraftBranchRepository contentDraftBranchRepository,
        IContentDraftBranchService contentDraftBranchService,
        IElementDraftBranchService elementDraftBranchService,
        IContentService contentService,
        IElementService elementService,
        IUserIdKeyResolver userIdKeyResolver,
        ICoreScopeProvider coreScopeProvider,
        IUmbracoContextFactory umbracoContextFactory,
        IServerMessenger serverMessenger)
    {
        _logger = logger;
        _scheduledPublishingSettings = scheduledPublishingSettings;
        _contentDraftBranchRepository = contentDraftBranchRepository;
        _contentDraftBranchService = contentDraftBranchService;
        _elementDraftBranchService = elementDraftBranchService;
        _contentService = contentService;
        _elementService = elementService;
        _userIdKeyResolver = userIdKeyResolver;
        _coreScopeProvider = coreScopeProvider;
        _umbracoContextFactory = umbracoContextFactory;
        _serverMessenger = serverMessenger;
    }

    /// <inheritdoc />
    public string Name => "ContentDraftBranchScheduledPublishingJob";

    /// <inheritdoc />
    public TimeSpan Period => _scheduledPublishingSettings.CurrentValue.Period;

    /// <inheritdoc />
    public bool AlignToClock => _scheduledPublishingSettings.CurrentValue.AlignToClock;

    /// <inheritdoc />
    public async Task ExecuteAsync()
    {
        if (Suspendable.ScheduledPublishing.CanRun == false)
        {
            return;
        }

        List<IContentDraftBranch> dueBranches;
        using (ICoreScope scope = _coreScopeProvider.CreateCoreScope())
        {
            scope.ReadLock(Constants.Locks.ContentDraftBranches);
            dueBranches = _contentDraftBranchRepository.GetScheduledForPublish(DateTime.UtcNow).ToList();
            scope.Complete();
        }

        if (dueBranches.Count == 0)
        {
            return;
        }

        // ensure an UmbracoContext, since this runs as a background task outside any HTTP request, and flush
        // the distributed cache messenger afterwards - mirrors ScheduledPublishingJob, which this job otherwise
        // shares no code path with (each branch's promote+publish manages its own scopes independently).
        using UmbracoContextReference contextReference = _umbracoContextFactory.EnsureUmbracoContext();
        try
        {
            foreach (IContentDraftBranch branch in dueBranches)
            {
                await PromoteAndQueuePublishForDueBranchAsync(branch);
            }
        }
        finally
        {
            if (contextReference.IsRoot)
            {
                _serverMessenger.SendMessages();
            }
        }
    }

    private static bool QueueDefaultDraftForPublish<TContent>(IPublishableContentService<TContent> service, TContent defaultDraft, DateTime releaseDate, ISet<string> cultures)
        where TContent : class, IPublishableContentBase
    {
        ContentScheduleCollection schedule = service.GetContentScheduleByContentId(defaultDraft.Id);

        if (cultures.Count == 0)
        {
            // TODO (KJA): Fix includeInvariantForVariant here
            schedule.AddOrUpdate(Constants.System.InvariantCulture, releaseDate, ContentScheduleAction.Release, true);
        }
        else
        {
            foreach (string culture in cultures)
            {
                // TODO (KJA): Fix includeInvariantForVariant here
                schedule.AddOrUpdate(culture, releaseDate, ContentScheduleAction.Release, true);
            }
        }

        service.PersistContentSchedule(defaultDraft, schedule);
        return true;
    }

    private async Task PromoteAndQueuePublishForDueBranchAsync(IContentDraftBranch branch)
    {
        try
        {
            if (branch.CreatedByUserId is null)
            {
                _logger.LogWarning(
                    "Scheduled publish skipped for draft branch {BranchKey}: it has no recorded creating user.",
                    branch.Key);
                return;
            }

            Guid userKey = await _userIdKeyResolver.GetAsync(branch.CreatedByUserId.Value);

            // Step 1: promote the branch - copies its content onto the node's default draft via a plain save.
            // This is a draft-branch-service operation and never publishes anything on its own.
            Attempt<ContentDraftBranchOperationStatus> promoteResult;
            bool isDocument;
            if (_contentDraftBranchService.GetContent(branch.ContentKey, branch.Key) is not null)
            {
                isDocument = true;
                promoteResult = await _contentDraftBranchService.PromoteAsync(branch.ContentKey, branch.Key, branch.ScheduledCultures, userKey);
            }
            else if (_elementDraftBranchService.GetContent(branch.ContentKey, branch.Key) is not null)
            {
                isDocument = false;
                promoteResult = await _elementDraftBranchService.PromoteAsync(branch.ContentKey, branch.Key, branch.ScheduledCultures, userKey);
            }
            else
            {
                _logger.LogWarning(
                    "Scheduled publish skipped for draft branch {BranchKey}: its content could not be resolved.",
                    branch.Key);
                return;
            }

            if (!promoteResult.Success)
            {
                _logger.LogWarning(
                    "Scheduled publish failed for draft branch {BranchKey}: promotion failed with status {Status}.",
                    branch.Key,
                    promoteResult.Result);
                return;
            }

            // Step 2: queue a scheduled release on the (now-updated) default draft, dated to when the branch
            // itself was due - not a direct publish call, and not through the draft-branch service. The
            // ordinary ScheduledPublishingJob will pick this up on its own next run and publish it through the
            // same pipeline any other scheduled or editorial publish would use.
            DateTime releaseDate = branch.ScheduledPublishDate ?? DateTime.UtcNow;
            bool queued;
            if (isDocument)
            {
                IContent? defaultDraft = _contentService.GetById(branch.ContentKey);
                queued = defaultDraft is not null && QueueDefaultDraftForPublish(_contentService, defaultDraft, releaseDate, branch.ScheduledCultures);
            }
            else
            {
                IElement? defaultDraft = _elementService.GetById(branch.ContentKey);
                queued = defaultDraft is not null && QueueDefaultDraftForPublish(_elementService, defaultDraft, releaseDate, branch.ScheduledCultures);
            }

            if (queued)
            {
                _logger.LogInformation(
                    "Scheduled publish queued for draft branch {BranchKey}: promoted and queued for the next scheduled-publishing run.",
                    branch.Key);
            }
            else
            {
                _logger.LogWarning("Scheduled publish failed for draft branch {BranchKey}: the underlying content could not be resolved to queue publishing.", branch.Key);
            }
        }
        catch (Exception ex)
        {
            // important to catch *everything* per branch so one failure doesn't stop the rest, or the job itself
            _logger.LogError(ex, "Scheduled publish threw for draft branch {BranchKey}.", branch.Key);
        }
    }
}
