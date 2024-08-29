using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.ContentPublishing;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Services;

internal sealed class ContentPublishingService : IContentPublishingService
{
    private readonly ICoreScopeProvider _coreScopeProvider;
    private readonly IContentService _contentService;
    private readonly IUserIdKeyResolver _userIdKeyResolver;
    private readonly IContentValidationService _contentValidationService;
    private readonly IContentTypeService _contentTypeService;
    private readonly ILanguageService _languageService;

    public ContentPublishingService(
        ICoreScopeProvider coreScopeProvider,
        IContentService contentService,
        IUserIdKeyResolver userIdKeyResolver,
        IContentValidationService contentValidationService,
        IContentTypeService contentTypeService,
        ILanguageService languageService)
    {
        _coreScopeProvider = coreScopeProvider;
        _contentService = contentService;
        _userIdKeyResolver = userIdKeyResolver;
        _contentValidationService = contentValidationService;
        _contentTypeService = contentTypeService;
        _languageService = languageService;
    }

    /// <inheritdoc />
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


        var cultures =
            cultureAndSchedule.CulturesToPublishImmediately.Union(
                cultureAndSchedule.Schedules.FullSchedule.Select(x => x.Culture)).ToArray();

        if (content.ContentType.VariesByCulture())
        {
            if (cultures.Any() is false)
            {
                scope.Complete();
                return Attempt.FailWithStatus(ContentPublishingOperationStatus.CultureMissing, new ContentPublishingResult());
            }

            var validCultures = (await _languageService.GetAllAsync()).Select(x => x.IsoCode);
            if (cultures.Any(x => x == "*") || cultures.All(x=> validCultures.Contains(x) is false))
            {
                scope.Complete();
                return Attempt.FailWithStatus(ContentPublishingOperationStatus.InvalidCulture, new ContentPublishingResult());
            }
        }
        else
        {
            if (cultures.Length != 1 || cultures.Any(x => x != "*"))
            {
                scope.Complete();
                return Attempt.FailWithStatus(ContentPublishingOperationStatus.InvalidCulture, new ContentPublishingResult());
            }
        }

        ContentValidationResult validationResult = await ValidateCurrentContentAsync(content, cultures);

        var errors = validationResult.ValidationErrors.Where(err =>
            cultures.Contains(err.Culture ?? "*", StringComparer.InvariantCultureIgnoreCase));
        if (errors.Any())
        {
            scope.Complete();
            return Attempt.FailWithStatus(ContentPublishingOperationStatus.ContentInvalid, new ContentPublishingResult
            {
                Content = content,
                InvalidPropertyAliases = errors.Select(property => property.Alias).ToArray()
                                         ?? Enumerable.Empty<string>()
            });
        }


        var userId = await _userIdKeyResolver.GetAsync(userKey);

        PublishResult? result = null;
        if (cultureAndSchedule.CulturesToPublishImmediately.Any())
        {
            result = _contentService.Publish(content, cultureAndSchedule.CulturesToPublishImmediately.ToArray(), userId);
        }
        else if(cultureAndSchedule.Schedules.FullSchedule.Any())
        {
            _contentService.PersistContentSchedule(content, cultureAndSchedule.Schedules);
            result = new PublishResult(PublishResultType.SuccessPublish, new EventMessages(), content);
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
            InvariantProperties = cultures.Contains("*") ? content.Properties.Where(x=>x.PropertyType.VariesByCulture() is false).Select(x=> new PropertyValueModel()
            {
                Alias = x.Alias,
                Value = x.GetValue()
            }) : Array.Empty<PropertyValueModel>(),
            Variants = cultures.Select(culture => new VariantModel()
            {
                Name = content.GetPublishName(culture) ?? string.Empty,
                Culture = culture,
                Segment = null,
                Properties = content.Properties.Where(prop=>prop.PropertyType.VariesByCulture()).Select(prop=> new PropertyValueModel()
                {
                    Alias = prop.Alias,
                    Value = prop.GetValue(culture: culture, segment:null, published:false)
                })
            })
        };
        IContentType? contentType = _contentTypeService.Get(content.ContentType.Key)!;
        ContentValidationResult validationResult = await _contentValidationService.ValidatePropertiesAsync(model, contentType);
        return validationResult;
    }

    /// <inheritdoc />
    public async Task<Attempt<ContentPublishingBranchResult, ContentPublishingOperationStatus>> PublishBranchAsync(Guid key, IEnumerable<string> cultures, bool force, Guid userKey)
    {
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
        IEnumerable<PublishResult> result = _contentService.PublishBranch(content, force, cultures.ToArray(), userId);
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

        var userId = await _userIdKeyResolver.GetAsync(userKey);

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
