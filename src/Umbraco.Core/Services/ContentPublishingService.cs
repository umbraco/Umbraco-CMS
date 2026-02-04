using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentPublishing;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Services;

/// <summary>
/// Provides services for publishing and unpublishing content, including scheduled publishing and branch publishing.
/// </summary>
internal sealed class ContentPublishingService : ContentPublishingServiceBase<IContent, IContentService>, IContentPublishingService
{
    private const string PublishBranchOperationType = "ContentPublishBranch";

    private readonly ICoreScopeProvider _coreScopeProvider;
    private readonly IContentService _contentService;
    private readonly IUserIdKeyResolver _userIdKeyResolver;
    private readonly ILogger<ContentPublishingService> _logger;
    private readonly ILongRunningOperationService _longRunningOperationService;
    private readonly IUmbracoContextFactory _umbracoContextFactory;

    protected override int WriteLockId => Constants.Locks.ContentTree;

    /// <summary>
    /// Initializes a new instance of the <see cref="ContentPublishingService"/> class.
    /// </summary>
    /// <param name="coreScopeProvider">The core scope provider.</param>
    /// <param name="contentService">The content service.</param>
    /// <param name="userIdKeyResolver">The user ID key resolver.</param>
    /// <param name="contentValidationService">The content validation service.</param>
    /// <param name="contentTypeService">The content type service.</param>
    /// <param name="languageService">The language service.</param>
    /// <param name="optionsMonitor">The content settings options monitor.</param>
    /// <param name="relationService">The relation service.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="longRunningOperationService">The long running operation service.</param>
    /// <param name="umbracoContextFactory">The Umbraco context factory.</param>
    public ContentPublishingService(
        ICoreScopeProvider coreScopeProvider,
        IContentService contentService,
        IUserIdKeyResolver userIdKeyResolver,
        IContentValidationService contentValidationService,
        IContentTypeService contentTypeService,
        ILanguageService languageService,
        IOptionsMonitor<ContentSettings> optionsMonitor,
        IRelationService relationService,
        ILogger<ContentPublishingService> logger,
        ILongRunningOperationService longRunningOperationService,
        IUmbracoContextFactory umbracoContextFactory)
        : base(
            coreScopeProvider,
            contentService,
            userIdKeyResolver,
            contentValidationService,
            contentTypeService,
            languageService,
            optionsMonitor,
            relationService,
            logger)
    {
        _coreScopeProvider = coreScopeProvider;
        _contentService = contentService;
        _userIdKeyResolver = userIdKeyResolver;
        _logger = logger;
        _longRunningOperationService = longRunningOperationService;
        _umbracoContextFactory = umbracoContextFactory;
    }

    /// <inheritdoc />
    public async Task<Attempt<ContentPublishingBranchResult, ContentPublishingOperationStatus>> PublishBranchAsync(
        Guid key,
        IEnumerable<string> cultures,
        PublishBranchFilter publishBranchFilter,
        Guid userKey,
        bool useBackgroundThread)
    {
        if (useBackgroundThread is false)
        {
            Attempt<ContentPublishingBranchInternalResult, ContentPublishingOperationStatus> minimalAttempt
                = await PerformPublishBranchAsync(key, cultures, publishBranchFilter, userKey, returnContent: true);
            return MapInternalPublishingAttempt(minimalAttempt);
        }

        _logger.LogDebug("Starting long running operation for publishing branch {Key} on background thread.", key);
        Attempt<Guid, LongRunningOperationEnqueueStatus> enqueueAttempt = await _longRunningOperationService.RunAsync(
            PublishBranchOperationType,
            async _ => await PerformPublishBranchAsync(key, cultures, publishBranchFilter, userKey, returnContent: false),
            allowConcurrentExecution: true);
        if (enqueueAttempt.Success)
        {
            return Attempt.SucceedWithStatus(
                ContentPublishingOperationStatus.Accepted,
                new ContentPublishingBranchResult { AcceptedTaskId = enqueueAttempt.Result });
        }

        return Attempt.FailWithStatus(
            ContentPublishingOperationStatus.Unknown,
            new ContentPublishingBranchResult
            {
                FailedItems =
                [
                    new ContentPublishingBranchItemResult
                    {
                        Key = key,
                        OperationStatus = ContentPublishingOperationStatus.Unknown,
                    }
                ],
            });
    }

    private async Task<Attempt<ContentPublishingBranchInternalResult, ContentPublishingOperationStatus>> PerformPublishBranchAsync(
        Guid key,
        IEnumerable<string> cultures,
        PublishBranchFilter publishBranchFilter,
        Guid userKey,
        bool returnContent)
    {
        // Ensure we have an UmbracoContext in case running on a background thread so operations that run in the published notification handlers
        // have access to this (e.g. webhooks).
        using UmbracoContextReference umbracoContextReference = _umbracoContextFactory.EnsureUmbracoContext();

        using ICoreScope scope = _coreScopeProvider.CreateCoreScope();
        IContent? content = _contentService.GetById(key);
        if (content is null)
        {
            return Attempt.FailWithStatus(
                ContentPublishingOperationStatus.ContentNotFound,
                new ContentPublishingBranchInternalResult
                {
                    FailedItems =
                    [
                        new ContentPublishingBranchItemResult
                        {
                            Key = key,
                            OperationStatus = ContentPublishingOperationStatus.ContentNotFound,
                        }
                    ],
                });
        }

        var userId = await _userIdKeyResolver.GetAsync(userKey);
        IEnumerable<PublishResult> result = _contentService.PublishBranch(content, publishBranchFilter, cultures.ToArray(), userId);
        scope.Complete();

        var itemResults = result.ToDictionary(r => r.Content.Key, ToContentPublishingOperationStatus);
        var branchResult = new ContentPublishingBranchInternalResult
        {
            ContentKey = content.Key,
            Content = returnContent ? content : null,
            SucceededItems = itemResults
                .Where(i => i.Value is ContentPublishingOperationStatus.Success)
                .Select(i => new ContentPublishingBranchItemResult { Key = i.Key, OperationStatus = i.Value })
                .ToArray(),
            FailedItems = itemResults
                .Where(i => i.Value is not ContentPublishingOperationStatus.Success)
                .Select(i => new ContentPublishingBranchItemResult { Key = i.Key, OperationStatus = i.Value })
                .ToArray(),
        };

        Attempt<ContentPublishingBranchInternalResult, ContentPublishingOperationStatus> attempt = branchResult.FailedItems.Any() is false
            ? Attempt.SucceedWithStatus(ContentPublishingOperationStatus.Success, branchResult)
            : Attempt.FailWithStatus(ContentPublishingOperationStatus.FailedBranch, branchResult);

        return attempt;
    }

    /// <inheritdoc/>
    public async Task<bool> IsPublishingBranchAsync(Guid taskId)
        => await _longRunningOperationService.GetStatusAsync(taskId) is LongRunningOperationStatus.Enqueued or LongRunningOperationStatus.Running;

    /// <inheritdoc/>
    public async Task<Attempt<ContentPublishingBranchResult, ContentPublishingOperationStatus>> GetPublishBranchResultAsync(Guid taskId)
    {
        Attempt<Attempt<ContentPublishingBranchInternalResult, ContentPublishingOperationStatus>, LongRunningOperationResultStatus> result =
            await _longRunningOperationService
                .GetResultAsync<Attempt<ContentPublishingBranchInternalResult, ContentPublishingOperationStatus>>(taskId);

        if (result.Success is false)
        {
            return Attempt.FailWithStatus(
                result.Status switch
                {
                    LongRunningOperationResultStatus.OperationNotFound => ContentPublishingOperationStatus.TaskResultNotFound,
                    LongRunningOperationResultStatus.OperationFailed => ContentPublishingOperationStatus.Failed,
                    _ => ContentPublishingOperationStatus.Unknown,
                },
                new ContentPublishingBranchResult());
        }

        return MapInternalPublishingAttempt(result.Result);
    }

    private Attempt<ContentPublishingBranchResult, ContentPublishingOperationStatus> MapInternalPublishingAttempt(
        Attempt<ContentPublishingBranchInternalResult, ContentPublishingOperationStatus> minimalAttempt) =>
        minimalAttempt.Success
            ? Attempt.SucceedWithStatus(minimalAttempt.Status, MapMinimalPublishingBranchResult(minimalAttempt.Result))
            : Attempt.FailWithStatus(minimalAttempt.Status, MapMinimalPublishingBranchResult(minimalAttempt.Result));

    private ContentPublishingBranchResult MapMinimalPublishingBranchResult(ContentPublishingBranchInternalResult internalResult) =>
        new()
        {
            Content = internalResult.Content
                      ?? (internalResult.ContentKey is null ? null : _contentService.GetById(internalResult.ContentKey.Value)),
            SucceededItems = internalResult.SucceededItems,
            FailedItems = internalResult.FailedItems,
        };
}
