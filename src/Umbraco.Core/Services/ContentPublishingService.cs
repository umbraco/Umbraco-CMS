using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

public class ContentPublishingService : IContentPublishingService
{
    private readonly ICoreScopeProvider _coreScopeProvider;
    private readonly IContentService _contentService;
    private readonly IUserIdKeyResolver _userIdKeyResolver;

    public ContentPublishingService(ICoreScopeProvider coreScopeProvider, IContentService contentService, IUserIdKeyResolver userIdKeyResolver)
    {
        _coreScopeProvider = coreScopeProvider;
        _contentService = contentService;
        _userIdKeyResolver = userIdKeyResolver;
    }

    /// <inheritdoc />
    public async Task<Attempt<ContentPublishingOperationStatus>> PublishAsync(Guid key, IEnumerable<string> cultures, Guid userKey)
    {
        using ICoreScope scope = _coreScopeProvider.CreateCoreScope();
        IContent? content = _contentService.GetById(key);
        if (content is null)
        {
            return Attempt.Fail(ContentPublishingOperationStatus.ContentNotFound);
        }

        var userId = await _userIdKeyResolver.GetAsync(userKey);
        PublishResult result = _contentService.Publish(content, cultures.ToArray(), userId);
        scope.Complete();

        ContentPublishingOperationStatus contentPublishingOperationStatus = ToContentPublishingOperationStatus(result);
        return contentPublishingOperationStatus is ContentPublishingOperationStatus.Success
            ? Attempt.Succeed(ToContentPublishingOperationStatus(result))
            : Attempt.Fail(ToContentPublishingOperationStatus(result));
    }

    /// <inheritdoc />
    public async Task<Attempt<IDictionary<Guid, ContentPublishingOperationStatus>>> PublishBranchAsync(Guid key, IEnumerable<string> cultures, bool force, Guid userKey)
    {
        using ICoreScope scope = _coreScopeProvider.CreateCoreScope();
        IContent? content = _contentService.GetById(key);
        if (content is null)
        {
            var payload = new Dictionary<Guid, ContentPublishingOperationStatus>
            {
                { key, ContentPublishingOperationStatus.ContentNotFound },
            };

            return Attempt<IDictionary<Guid, ContentPublishingOperationStatus>>.Fail(payload);
        }

        var userId = await _userIdKeyResolver.GetAsync(userKey);
        IEnumerable<PublishResult> result = _contentService.PublishBranch(content, force, cultures.ToArray(), userId);
        scope.Complete();

        var payloads = result.ToDictionary(r => r.Content.Key, ToContentPublishingOperationStatus);
        return payloads.All(p => p.Value is ContentPublishingOperationStatus.Success)
            ? Attempt<IDictionary<Guid, ContentPublishingOperationStatus>>.Succeed(payloads)
            : Attempt<IDictionary<Guid, ContentPublishingOperationStatus>>.Fail(payloads);
    }

    /// <inheritdoc />
    public async Task<Attempt<ContentPublishingOperationStatus>> UnpublishAsync(Guid key, string? culture, Guid userKey)
    {
        using ICoreScope scope = _coreScopeProvider.CreateCoreScope();
        IContent? content = _contentService.GetById(key);
        if (content is null)
        {
            return Attempt.Fail(ContentPublishingOperationStatus.ContentNotFound);
        }

        var userId = await _userIdKeyResolver.GetAsync(userKey);
        PublishResult result = _contentService.Unpublish(content, culture ?? "*", userId);
        scope.Complete();

        ContentPublishingOperationStatus contentPublishingOperationStatus = ToContentPublishingOperationStatus(result);
        return contentPublishingOperationStatus is ContentPublishingOperationStatus.Success
            ? Attempt.Succeed(ToContentPublishingOperationStatus(result))
            : Attempt.Fail(ToContentPublishingOperationStatus(result));
    }

    private static ContentPublishingOperationStatus ToContentPublishingOperationStatus(PublishResult publishResult)
        => publishResult.Result switch
        {
            PublishResultType.SuccessPublish => ContentPublishingOperationStatus.Success,
            PublishResultType.SuccessPublishCulture => ContentPublishingOperationStatus.Success,
            PublishResultType.SuccessPublishAlready => ContentPublishingOperationStatus.Success,
            PublishResultType.SuccessUnpublish => ContentPublishingOperationStatus.Success,
            PublishResultType.SuccessUnpublishAlready => ContentPublishingOperationStatus.Success,
            PublishResultType.SuccessUnpublishCulture => ContentPublishingOperationStatus.Success,
            PublishResultType.SuccessUnpublishMandatoryCulture => ContentPublishingOperationStatus.Success,
            PublishResultType.SuccessUnpublishLastCulture => ContentPublishingOperationStatus.Success,
            PublishResultType.SuccessMixedCulture => ContentPublishingOperationStatus.Success,
            // PublishResultType.FailedPublish => expr, <-- never used directly in a PublishResult
            PublishResultType.FailedPublishPathNotPublished => ContentPublishingOperationStatus.PathNotPublished,
            PublishResultType.FailedPublishHasExpired => ContentPublishingOperationStatus.HasExpired,
            PublishResultType.FailedPublishAwaitingRelease => ContentPublishingOperationStatus.AwaitingRelease,
            PublishResultType.FailedPublishCultureHasExpired => ContentPublishingOperationStatus.CultureHasExpired,
            PublishResultType.FailedPublishCultureAwaitingRelease => ContentPublishingOperationStatus.CultureAwaitingRelease,
            PublishResultType.FailedPublishIsTrashed => ContentPublishingOperationStatus.InTrash,
            PublishResultType.FailedPublishCancelledByEvent => ContentPublishingOperationStatus.CancelledByEvent,
            PublishResultType.FailedPublishContentInvalid => ContentPublishingOperationStatus.ContentInvalid,
            PublishResultType.FailedPublishNothingToPublish => ContentPublishingOperationStatus.NothingToPublish,
            PublishResultType.FailedPublishMandatoryCultureMissing => ContentPublishingOperationStatus.MandatoryCultureMissing,
            PublishResultType.FailedPublishConcurrencyViolation => ContentPublishingOperationStatus.ConcurrencyViolation,
            PublishResultType.FailedPublishUnsavedChanges => ContentPublishingOperationStatus.UnsavedChanges,
            PublishResultType.FailedUnpublish => ContentPublishingOperationStatus.Failed,
            PublishResultType.FailedUnpublishCancelledByEvent => ContentPublishingOperationStatus.CancelledByEvent,
            _ => throw new ArgumentOutOfRangeException()
        };
}
