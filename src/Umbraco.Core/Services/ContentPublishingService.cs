using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.HostedServices;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.ContentPublishing;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Services;

internal sealed class ContentPublishingService : IContentPublishingService
{
    private const string IsPublishingBranchCacheRuntimeCacheKeyFormat = "temp_publish_branch_op_{0}_{1}";

    private readonly ICoreScopeProvider _coreScopeProvider;
    private readonly IContentService _contentService;
    private readonly IUserIdKeyResolver _userIdKeyResolver;
    private readonly IContentValidationService _contentValidationService;
    private readonly IContentTypeService _contentTypeService;
    private readonly ILanguageService _languageService;
    private ContentSettings _contentSettings;
    private readonly IRelationService _relationService;
    private readonly ILogger<ContentPublishingService> _logger;
    private readonly IBackgroundTaskQueue _backgroundTaskQueue;
    private readonly IAppPolicyCache _runtimeCache;

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
        IBackgroundTaskQueue backgroundTaskQueue,
        IAppPolicyCache runtimeCache)
    {
        _coreScopeProvider = coreScopeProvider;
        _contentService = contentService;
        _userIdKeyResolver = userIdKeyResolver;
        _contentValidationService = contentValidationService;
        _contentTypeService = contentTypeService;
        _languageService = languageService;
        _relationService = relationService;
        _logger = logger;
        _backgroundTaskQueue = backgroundTaskQueue;
        _runtimeCache = runtimeCache;
        _contentSettings = optionsMonitor.CurrentValue;
        optionsMonitor.OnChange((contentSettings) =>
        {
            _contentSettings = contentSettings;
        });
    }

    /// <inheritdoc />
    public async Task<Attempt<ContentPublishingResult, ContentPublishingOperationStatus>> PublishAsync(
        Guid key,
        ICollection<CulturePublishScheduleModel> culturesToPublishOrSchedule,
        Guid userKey)
    {
        var culturesToPublishImmediately =
            culturesToPublishOrSchedule.Where(culture => culture.Schedule is null).Select(c => c.Culture ?? Constants.System.InvariantCulture).ToHashSet();

        ContentScheduleCollection schedules = _contentService.GetContentScheduleByContentId(key);

        foreach (CulturePublishScheduleModel cultureToSchedule in culturesToPublishOrSchedule.Where(c => c.Schedule is not null))
        {
            var culture = cultureToSchedule.Culture ?? Constants.System.InvariantCulture;

            if (cultureToSchedule.Schedule!.PublishDate is null)
            {
                schedules.RemoveIfExists(culture, ContentScheduleAction.Release);
            }
            else
            {
                schedules.AddOrUpdate(culture, cultureToSchedule.Schedule!.PublishDate.Value.UtcDateTime, ContentScheduleAction.Release);
            }

            if (cultureToSchedule.Schedule!.UnpublishDate is null)
            {
                schedules.RemoveIfExists(culture, ContentScheduleAction.Expire);
            }
            else
            {
                schedules.AddOrUpdate(culture, cultureToSchedule.Schedule!.UnpublishDate.Value.UtcDateTime, ContentScheduleAction.Expire);
            }
        }

        var cultureAndSchedule = new CultureAndScheduleModel
        {
            CulturesToPublishImmediately = culturesToPublishImmediately,
            Schedules = schedules,
        };

        return await PublishAsync(key, cultureAndSchedule, userKey);
    }

    /// <inheritdoc />
    [Obsolete("Use non obsoleted version instead. Scheduled for removal in v17")]
    public async Task<Attempt<ContentPublishingResult, ContentPublishingOperationStatus>> PublishAsync(
        Guid key,
        CultureAndScheduleModel cultureAndSchedule,
        Guid userKey)
    {
        using ICoreScope scope = _coreScopeProvider.CreateCoreScope();
        IContent? content = _contentService.GetById(key);
        if (content is null)
        {
            scope.Complete();
            return Attempt.FailWithStatus(ContentPublishingOperationStatus.ContentNotFound, new ContentPublishingResult());
        }

        // If nothing is requested for publish or scheduling, clear all schedules and publish nothing.
        if (cultureAndSchedule.CulturesToPublishImmediately.Count == 0 &&
            cultureAndSchedule.Schedules.FullSchedule.Count == 0)
        {
            _contentService.PersistContentSchedule(content, cultureAndSchedule.Schedules);
            scope.Complete();
            return Attempt.SucceedWithStatus(
                ContentPublishingOperationStatus.Success,
                new ContentPublishingResult { Content = content });
        }

        ISet<string> culturesToPublishImmediately = cultureAndSchedule.CulturesToPublishImmediately;

        var cultures =
            culturesToPublishImmediately.Union(
                cultureAndSchedule.Schedules.FullSchedule.Select(x => x.Culture)).ToArray();

        // If cultures are provided for non variant content, and they include the default culture, consider
        // the request as valid for publishing the content.
        // This is necessary as in a bulk publishing context the cultures are selected and provided from the
        // list of languages.
        bool variesByCulture = content.ContentType.VariesByCulture();
        if (!variesByCulture)
        {
            ILanguage? defaultLanguage = await _languageService.GetDefaultLanguageAsync();
            if (defaultLanguage is not null)
            {
                if (cultures.Contains(defaultLanguage.IsoCode))
                {
                    cultures = ["*"];
                }

                if (culturesToPublishImmediately.Contains(defaultLanguage.IsoCode))
                {
                    culturesToPublishImmediately = new HashSet<string> { "*" };
                }
            }
        }

        if (variesByCulture)
        {
            if (cultures.Any() is false)
            {
                scope.Complete();
                return Attempt.FailWithStatus(ContentPublishingOperationStatus.CultureMissing, new ContentPublishingResult());
            }

            if (cultures.Any(x => x == Constants.System.InvariantCulture))
            {
                scope.Complete();
                return Attempt.FailWithStatus(ContentPublishingOperationStatus.CannotPublishInvariantWhenVariant, new ContentPublishingResult());
            }

            IEnumerable<string> validCultures = (await _languageService.GetAllAsync()).Select(x => x.IsoCode);
            if (validCultures.ContainsAll(cultures) is false)
            {
                scope.Complete();
                return Attempt.FailWithStatus(ContentPublishingOperationStatus.InvalidCulture, new ContentPublishingResult());
            }
        }
        else
        {
            if (cultures.Length != 1 || cultures.Any(x => x != Constants.System.InvariantCulture))
            {
                scope.Complete();
                return Attempt.FailWithStatus(ContentPublishingOperationStatus.InvalidCulture, new ContentPublishingResult());
            }
        }

        ContentValidationResult validationResult = await ValidateCurrentContentAsync(content, cultures);
        if (validationResult.ValidationErrors.Any())
        {
            scope.Complete();
            return Attempt.FailWithStatus(ContentPublishingOperationStatus.ContentInvalid, new ContentPublishingResult
            {
                Content = content,
                InvalidPropertyAliases = validationResult.ValidationErrors.Select(property => property.Alias).ToArray()
            });
        }


        var userId = await _userIdKeyResolver.GetAsync(userKey);

        PublishResult? result = null;
        if (culturesToPublishImmediately.Any())
        {
            result = _contentService.Publish(content, culturesToPublishImmediately.ToArray(), userId);
        }

        if (result?.Success != false && cultureAndSchedule.Schedules.FullSchedule.Any())
        {
            _contentService.PersistContentSchedule(result?.Content ?? content, cultureAndSchedule.Schedules);
            result = new PublishResult(
                PublishResultType.SuccessPublish,
                result?.EventMessages ?? new EventMessages(),
                result?.Content ?? content);
        }

        scope.Complete();

        if (result is null)
        {
            return Attempt.FailWithStatus(ContentPublishingOperationStatus.NothingToPublish, new ContentPublishingResult());
        }

        ContentPublishingOperationStatus contentPublishingOperationStatus = ToContentPublishingOperationStatus(result);
        return contentPublishingOperationStatus is ContentPublishingOperationStatus.Success
            ? Attempt.SucceedWithStatus(
                ToContentPublishingOperationStatus(result),
                new ContentPublishingResult { Content = content })
            : Attempt.FailWithStatus(ToContentPublishingOperationStatus(result), new ContentPublishingResult
            {
                Content = content,
                InvalidPropertyAliases = result.InvalidProperties?.Select(property => property.Alias).ToArray()
                                         ?? Enumerable.Empty<string>()
            });
    }

    private async Task<ContentValidationResult> ValidateCurrentContentAsync(IContent content, string[] cultures)
    {
        // Would be better to be able to use a mapper/factory, but currently all that functionality is very much presentation logic.
        var model = new ContentUpdateModel()
        {
            InvariantName = content.Name,
            // NOTE KJA: this needs redoing; we need to make an informed decision whether to include invariant properties, depending on if editing invariant properties is allowed on all variants, or if the default language is included in cultures
            InvariantProperties = content.Properties.Where(x => x.PropertyType.VariesByCulture() is false).Select(x => new PropertyValueModel()
            {
                Alias = x.Alias,
                Value = x.GetValue()
            }),
            Variants = cultures.Select(culture => new VariantModel()
            {
                Name = content.GetPublishName(culture) ?? string.Empty,
                Culture = culture,
                Segment = null,
                Properties = content.Properties.Where(prop => prop.PropertyType.VariesByCulture()).Select(prop => new PropertyValueModel()
                {
                    Alias = prop.Alias,
                    Value = prop.GetValue(culture: culture, segment: null, published: false)
                })
            })
        };
        IContentType? contentType = _contentTypeService.Get(content.ContentType.Key)!;
        ContentValidationResult validationResult = await _contentValidationService.ValidatePropertiesAsync(model, contentType, cultures);
        return validationResult;
    }

    /// <inheritdoc />
    [Obsolete("This method is not longer used as the 'force' parameter has been extended into options for publishing unpublished and re-publishing changed content. Please use the overload containing the parameter for those options instead. Scheduled for removal in Umbraco 17.")]
    public async Task<Attempt<ContentPublishingBranchResult, ContentPublishingOperationStatus>> PublishBranchAsync(Guid key, IEnumerable<string> cultures, bool force, Guid userKey)
        => await PublishBranchAsync(key, cultures, force ? PublishBranchFilter.IncludeUnpublished : PublishBranchFilter.Default, userKey);

    /// <inheritdoc />
    [Obsolete("Please use the overload containing all parameters. Scheduled for removal in Umbraco 17.")]
    public async Task<Attempt<ContentPublishingBranchResult, ContentPublishingOperationStatus>> PublishBranchAsync(Guid key, IEnumerable<string> cultures, PublishBranchFilter publishBranchFilter, Guid userKey)
        => await PublishBranchAsync(key, cultures, publishBranchFilter, userKey, false);

    /// <inheritdoc />
    public async Task<Attempt<ContentPublishingBranchResult, ContentPublishingOperationStatus>> PublishBranchAsync(Guid key, IEnumerable<string> cultures, PublishBranchFilter publishBranchFilter, Guid userKey, bool useBackgroundThread)
    {
        if (useBackgroundThread)
        {
            _logger.LogInformation("Starting async background thread for rebuilding database cache.");

            _backgroundTaskQueue.QueueBackgroundWorkItem(
                cancellationToken =>
                {
                    using (ExecutionContext.SuppressFlow())
                    {
                        Task.Run(async () => await PerformPublishBranchAsync(key, cultures, publishBranchFilter, userKey));
                        return Task.CompletedTask;
                    }
                });

            return Attempt.SucceedWithStatus(ContentPublishingOperationStatus.Accepted, new ContentPublishingBranchResult());
        }
        else
        {
            return await PerformPublishBranchAsync(key, cultures, publishBranchFilter, userKey);
        }
    }

    private async Task<Attempt<ContentPublishingBranchResult, ContentPublishingOperationStatus>> PerformPublishBranchAsync(Guid key, IEnumerable<string> cultures, PublishBranchFilter publishBranchFilter, Guid userKey)
    {
        try
        {
            SetIsPublishingBranch(key, userKey);

            using ICoreScope scope = _coreScopeProvider.CreateCoreScope();
            IContent? content = _contentService.GetById(key);
            if (content is null)
            {
                return Attempt.FailWithStatus(
                    ContentPublishingOperationStatus.ContentNotFound,
                    new ContentPublishingBranchResult
                    {
                        FailedItems = new[]
                        {
                        new ContentPublishingBranchItemResult
                        {
                            Key = key, OperationStatus = ContentPublishingOperationStatus.ContentNotFound
                        }
                        }
                    });
            }

            var userId = await _userIdKeyResolver.GetAsync(userKey);
            IEnumerable<PublishResult> result = _contentService.PublishBranch(content, publishBranchFilter, cultures.ToArray(), userId);
            scope.Complete();

            var itemResults = result.ToDictionary(r => r.Content.Key, ToContentPublishingOperationStatus);
            var branchResult = new ContentPublishingBranchResult
            {
                Content = content,
                SucceededItems = itemResults
                    .Where(i => i.Value is ContentPublishingOperationStatus.Success)
                    .Select(i => new ContentPublishingBranchItemResult { Key = i.Key, OperationStatus = i.Value })
                    .ToArray(),
                FailedItems = itemResults
                    .Where(i => i.Value is not ContentPublishingOperationStatus.Success)
                    .Select(i => new ContentPublishingBranchItemResult { Key = i.Key, OperationStatus = i.Value })
                    .ToArray()
            };

            return branchResult.FailedItems.Any() is false
                ? Attempt.SucceedWithStatus(ContentPublishingOperationStatus.Success, branchResult)
                : Attempt.FailWithStatus(ContentPublishingOperationStatus.FailedBranch, branchResult);
        }
        finally
        {
            ClearIsPublishingBranch(key, userKey);
        }
    }

    private void SetIsPublishingBranch(Guid key, Guid userKey) => _runtimeCache.Insert(GetIsPublishingBranchCacheKey(key, userKey), () => "tempValue", TimeSpan.FromMinutes(10));

    private void ClearIsPublishingBranch(Guid key, Guid userKey) => _runtimeCache.Clear(GetIsPublishingBranchCacheKey(key, userKey));

    private static string GetIsPublishingBranchCacheKey(Guid key, Guid userKey) => string.Format(IsPublishingBranchCacheRuntimeCacheKeyFormat, key, userKey);

    /// <inheritdoc/>
    public async Task<Attempt<ContentPublishingOperationStatus>> UnpublishAsync(Guid key, ISet<string>? cultures, Guid userKey)
    {
        using ICoreScope scope = _coreScopeProvider.CreateCoreScope();
        IContent? content = _contentService.GetById(key);
        if (content is null)
        {
            scope.Complete();
            return Attempt.Fail(ContentPublishingOperationStatus.ContentNotFound);
        }

        if (_contentSettings.DisableUnpublishWhenReferenced && _relationService.IsRelated(content.Id))
        {
            scope.Complete();
            return Attempt<ContentPublishingOperationStatus>.Fail(ContentPublishingOperationStatus.CannotUnpublishWhenReferenced);
        }

        var userId = await _userIdKeyResolver.GetAsync(userKey);

        // If cultures are provided for non variant content, and they include the default culture, consider
        // the request as valid for unpublishing the content.
        // This is necessary as in a bulk unpublishing context the cultures are selected and provided from the
        // list of languages.
        if (cultures is not null && !content.ContentType.VariesByCulture())
        {
            ILanguage? defaultLanguage = await _languageService.GetDefaultLanguageAsync();
            if (defaultLanguage is not null && cultures.Contains(defaultLanguage.IsoCode))
            {
                cultures = null;
            }
        }

        Attempt<ContentPublishingOperationStatus> attempt;
        if (cultures is null)
        {
            attempt = await UnpublishInvariantAsync(
                content,
                userId);

            scope.Complete();
            return attempt;
        }

        if (cultures.Any() is false)
        {
            scope.Complete();
            return Attempt<ContentPublishingOperationStatus>.Fail(ContentPublishingOperationStatus.CultureMissing);
        }

        if (cultures.Contains("*"))
        {
            attempt = await UnpublishAllCulturesAsync(
                content,
                userId);
        }
        else
        {
            attempt = await UnpublishMultipleCultures(
                content,
                cultures,
                userId);
        }
        scope.Complete();

        return attempt;
    }

    private Task<Attempt<ContentPublishingOperationStatus>> UnpublishAllCulturesAsync(IContent content, int userId)
    {
        if (content.ContentType.VariesByCulture() is false)
        {
            return Task.FromResult(Attempt.Fail(ContentPublishingOperationStatus.CannotPublishVariantWhenNotVariant));
        }

        using ICoreScope scope = _coreScopeProvider.CreateCoreScope();
        PublishResult result = _contentService.Unpublish(content, "*", userId);
        scope.Complete();

        ContentPublishingOperationStatus contentPublishingOperationStatus = ToContentPublishingOperationStatus(result);
        return Task.FromResult(contentPublishingOperationStatus is ContentPublishingOperationStatus.Success
            ? Attempt.Succeed(ToContentPublishingOperationStatus(result))
            : Attempt.Fail(ToContentPublishingOperationStatus(result)));
    }

    private async Task<Attempt<ContentPublishingOperationStatus>> UnpublishMultipleCultures(IContent content, ISet<string> cultures, int userId)
    {
        using ICoreScope scope = _coreScopeProvider.CreateCoreScope();

        if (content.ContentType.VariesByCulture() is false)
        {
            scope.Complete();
            return Attempt.Fail(ContentPublishingOperationStatus.CannotPublishVariantWhenNotVariant);
        }

        var validCultures = (await _languageService.GetAllAsync()).Select(x => x.IsoCode).ToArray();

        foreach (var culture in cultures)
        {
            if (validCultures.Contains(culture) is false)
            {
                scope.Complete();
                return Attempt.Fail(ContentPublishingOperationStatus.InvalidCulture);
            }

            PublishResult result = _contentService.Unpublish(content, culture, userId);

            ContentPublishingOperationStatus contentPublishingOperationStatus = ToContentPublishingOperationStatus(result);

            if (contentPublishingOperationStatus is not ContentPublishingOperationStatus.Success)
            {
                return Attempt.Fail(ToContentPublishingOperationStatus(result));
            }
        }

        scope.Complete();
        return Attempt.Succeed(ContentPublishingOperationStatus.Success);
    }


    private Task<Attempt<ContentPublishingOperationStatus>> UnpublishInvariantAsync(IContent content, int userId)
    {
        using ICoreScope scope = _coreScopeProvider.CreateCoreScope();

        if (content.ContentType.VariesByCulture())
        {
            return Task.FromResult(Attempt.Fail(ContentPublishingOperationStatus.CannotPublishInvariantWhenVariant));
        }

        PublishResult result = _contentService.Unpublish(content, null, userId);
        scope.Complete();

        ContentPublishingOperationStatus contentPublishingOperationStatus = ToContentPublishingOperationStatus(result);
        return Task.FromResult(contentPublishingOperationStatus is ContentPublishingOperationStatus.Success
            ? Attempt.Succeed(ToContentPublishingOperationStatus(result))
            : Attempt.Fail(ToContentPublishingOperationStatus(result)));
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
