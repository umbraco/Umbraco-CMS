using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Extensions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services.Filters;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Services;

/// <summary>
/// Provides services for creating, updating, and managing content (documents).
/// </summary>
internal sealed class ContentEditingService
    : ContentEditingServiceWithSortingBase<IContent, IContentType, IContentService, IContentTypeService>, IContentEditingService
{
    private readonly PropertyEditorCollection _propertyEditorCollection;
    private readonly ITemplateService _templateService;
    private readonly ILogger<ContentEditingService> _logger;
    private readonly IUserService _userService;
    private readonly ILocalizationService _localizationService;
    private readonly ILanguageService _languageService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ContentEditingService"/> class.
    /// </summary>
    /// <param name="contentService">The content service.</param>
    /// <param name="contentTypeService">The content type service.</param>
    /// <param name="propertyEditorCollection">The property editor collection.</param>
    /// <param name="dataTypeService">The data type service.</param>
    /// <param name="templateService">The template service.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="scopeProvider">The scope provider.</param>
    /// <param name="userIdKeyResolver">The user ID key resolver.</param>
    /// <param name="treeEntitySortingService">The tree entity sorting service.</param>
    /// <param name="contentValidationService">The content validation service.</param>
    /// <param name="userService">The user service.</param>
    /// <param name="localizationService">The localization service.</param>
    /// <param name="languageService">The language service.</param>
    /// <param name="optionsMonitor">The content settings options monitor.</param>
    /// <param name="relationService">The relation service.</param>
    /// <param name="contentTypeFilters">The content type filter collection.</param>
    public ContentEditingService(
        IContentService contentService,
        IContentTypeService contentTypeService,
        PropertyEditorCollection propertyEditorCollection,
        IDataTypeService dataTypeService,
        ITemplateService templateService,
        ILogger<ContentEditingService> logger,
        ICoreScopeProvider scopeProvider,
        IUserIdKeyResolver userIdKeyResolver,
        ITreeEntitySortingService treeEntitySortingService,
        IContentValidationService contentValidationService,
        IUserService userService,
        ILocalizationService localizationService,
        ILanguageService languageService,
        IOptionsMonitor<ContentSettings> optionsMonitor,
        IRelationService relationService,
        ContentTypeFilterCollection contentTypeFilters)
        : base(
            contentService,
            contentTypeService,
            propertyEditorCollection,
            dataTypeService,
            logger,
            scopeProvider,
            userIdKeyResolver,
            contentValidationService,
            treeEntitySortingService,
            optionsMonitor,
            relationService,
            contentTypeFilters)
    {
        _propertyEditorCollection = propertyEditorCollection;
        _templateService = templateService;
        _logger = logger;
        _userService = userService;
        _localizationService = localizationService;
        _languageService = languageService;
    }

    /// <inheritdoc/>
    protected override string? RelateParentOnDeleteAlias => Constants.Conventions.RelationTypes.RelateParentDocumentOnDeleteAlias;

    /// <inheritdoc />
    public Task<IContent?> GetAsync(Guid key)
    {
        IContent? content = ContentService.GetById(key);
        return Task.FromResult(content);
    }

    /// <inheritdoc />
    public async Task<Attempt<ContentValidationResult, ContentEditingOperationStatus>> ValidateUpdateAsync(Guid key, ValidateContentUpdateModel updateModel, Guid userKey)
    {
        IContent? content = ContentService.GetById(key);
        return content is not null
            ? await ValidateCulturesAndPropertiesAsync(updateModel, content.ContentType.Key, await GetCulturesToValidate(updateModel.Cultures, userKey))
            : Attempt.FailWithStatus(ContentEditingOperationStatus.NotFound, new ContentValidationResult());
    }

    /// <inheritdoc />
    public async Task<Attempt<ContentValidationResult, ContentEditingOperationStatus>> ValidateCreateAsync(ContentCreateModel createModel, Guid userKey)
    {
        ContentEditingOperationStatus creationAllowedStatus = await ValidateCreationAllowedAsync(createModel);
        if (creationAllowedStatus != ContentEditingOperationStatus.Success)
        {
            return Attempt.FailWithStatus(creationAllowedStatus, new ContentValidationResult());
        }

        return await ValidateCulturesAndPropertiesAsync(createModel, createModel.ContentTypeKey, await GetCulturesToValidate(createModel.Variants.Select(variant => variant.Culture), userKey));
    }

    private async Task<IEnumerable<string?>?> GetCulturesToValidate(IEnumerable<string?>? cultures, Guid userKey)
    {
        // Cultures to validate can be provided by the calling code, but if the editor is restricted to only have
        // access to certain languages, we don't want to validate by any they aren't allowed to edit.

        // TODO: Remove this check once the obsolete overloads to ValidateCreateAsync and ValidateUpdateAsync that don't provide a user key are removed.
        // We only have this to ensure backwards compatibility with the obsolete overloads.
        if (userKey == Guid.Empty)
        {
            return cultures;
        }

        HashSet<string>? allowedCultures = await GetAllowedCulturesForEditingUser(userKey);

        if (cultures == null)
        {
            // If no cultures are provided, we are asking to validate all cultures. But if the user doesn't have access to all, we
            // should only validate the ones they do.
            IEnumerable<string> allCultures = await _languageService.GetAllIsoCodesAsync();
            return allowedCultures.Count == allCultures.Count() ? null : allowedCultures;
        }

        // If explicit cultures are provided, we should only validate the ones the user has access to.
        return cultures.Where(x => !string.IsNullOrEmpty(x) && allowedCultures.Contains(x)).ToList();
    }

    /// <inheritdoc />
    public async Task<Attempt<ContentCreateResult, ContentEditingOperationStatus>> CreateAsync(ContentCreateModel createModel, Guid userKey)
        => ToEditingAttempt(await HandleCreateAsync(createModel, null, userKey));

    /// <inheritdoc />
    public async Task<Attempt<ContentCreateResult, ContentEditingAndPublishingStatus>> CreateAndPublishAsync(ContentCreateModel createModel, ISet<string> culturesToPublish, Guid userKey)
        => await HandleCreateAsync(createModel, culturesToPublish, userKey);

    /// <inheritdoc />
    [Obsolete("Use the overload taking an ISet<string> of cultures to publish, which reports the save and the publish outcome separately. Scheduled for removal in Umbraco 19.")]
    public async Task<Attempt<ContentCreateResult, ContentEditingOperationStatus>> CreateAndPublishAsync(ContentCreateModel createModel, string[] culturesToPublish, Guid userKey)
        => ToEditingAttempt(await HandleCreateAsync(createModel, culturesToPublish.ToHashSet(), userKey));

    private async Task<Attempt<ContentCreateResult, ContentEditingAndPublishingStatus>> HandleCreateAsync(ContentCreateModel createModel, ISet<string>? culturesToPublish, Guid userKey)
    {
        if (await ValidateCulturesAsync(createModel) is false)
        {
            return Attempt.FailWithStatus(EditingStatus(ContentEditingOperationStatus.InvalidCulture), new ContentCreateResult());
        }

        Attempt<ContentCreateResult, ContentEditingOperationStatus> result = await MapCreate<ContentCreateResult>(createModel);
        if (result.Success == false)
        {
            return Attempt.FailWithStatus(EditingStatus(result.Status), result.Result);
        }

        // the create mapping might succeed, but this doesn't mean the model is valid at property level.
        // we'll return the actual property validation status if the entire operation succeeds.
        ContentEditingOperationStatus validationStatus = result.Status;
        ContentValidationResult validationResult = result.Result.ValidationResult;

        IContent content = await EnsureOnlyAllowedFieldsAreUpdated(result.Result.Content!, userKey);
        ContentEditingOperationStatus updateTemplateStatus = await UpdateTemplateAsync(content, createModel.TemplateKey);
        if (updateTemplateStatus != ContentEditingOperationStatus.Success)
        {
            return Attempt.FailWithStatus(
                EditingStatus(updateTemplateStatus),
                new ContentCreateResult { Content = content, ValidationResult = validationResult });
        }

        (ContentEditingAndPublishingStatus saveStatus, IEnumerable<string> invalidPropertyAliases) = culturesToPublish is null
            ? (EditingStatus(await Save(content, userKey)), Enumerable.Empty<string>())
            : await SaveAndPublish(content, culturesToPublish, userKey);
        return IsSuccess(saveStatus)
            ? Attempt.SucceedWithStatus(
                new ContentEditingAndPublishingStatus
                {
                    ContentEditingOperationStatus = validationStatus,
                    ContentPublishingOperationStatus = saveStatus.ContentPublishingOperationStatus,
                },
                new ContentCreateResult { Content = content, ValidationResult = validationResult })
            : Attempt.FailWithStatus(
                saveStatus,
                new ContentCreateResult
                {
                    Content = content,
                    ValidationResult = validationResult,
                    InvalidPropertyAliases = invalidPropertyAliases,
                });
    }

    /// <summary>
    /// A temporary method that ensures the data is sent in is overridden by the original data, in cases where the user do not have permissions to change the data.
    /// </summary>
    private async Task<IContent> EnsureOnlyAllowedFieldsAreUpdated(IContent contentWithPotentialUnallowedChanges, Guid userKey)
    {
        if (contentWithPotentialUnallowedChanges.ContentType.VariesByCulture() is false)
        {
            return contentWithPotentialUnallowedChanges;
        }

        IContent? existingContent = await GetAsync(contentWithPotentialUnallowedChanges.Key);

        HashSet<string>? allowedCultures = await GetAllowedCulturesForEditingUser(userKey);

        ILanguage? defaultLanguage = await _languageService.GetDefaultLanguageAsync();

        var disallowedCultures = (contentWithPotentialUnallowedChanges.EditedCultures ??
                               contentWithPotentialUnallowedChanges.PublishedCultures)
            .Where(culture => allowedCultures.Contains(culture) is false).ToList();

        var allowedToEditDefaultLanguage = allowedCultures.Contains(defaultLanguage?.IsoCode ?? string.Empty);

        var variantProperties = new List<IProperty>();
        var invariantWithVariantSupportProperties = new List<(IProperty Property, IDataEditor DataEditor)>();
        var invariantProperties = new List<IProperty>();

        // group properties in processing groups
        foreach (IProperty property in contentWithPotentialUnallowedChanges.Properties)
        {
            if (property.PropertyType.VariesByCulture())
            {
                variantProperties.Add(property);
            }
            else if (_propertyEditorCollection.TryGet(property.PropertyType.PropertyEditorAlias, out IDataEditor? dataEditor) && dataEditor.CanMergePartialPropertyValues(property.PropertyType))
            {
                invariantWithVariantSupportProperties.Add((property, dataEditor));
            }
            else
            {
                invariantProperties.Add(property);
            }
        }

        // if the property varies by culture, simply overwrite the edited property value with the current property value for every culture
        foreach (IProperty property in variantProperties)
        {
            foreach (var culture in disallowedCultures)
            {
                    var currentValue = existingContent?.Properties.First(x => x.Alias == property.Alias)
                        .GetValue(culture, null, false);
                    property.SetValue(currentValue, culture, null);
            }
        }

        // If property does not support merging, we still need to overwrite if we are not allowed to edit invariant properties.
        if (ContentSettings.AllowEditInvariantFromNonDefault is false && allowedToEditDefaultLanguage is false)
        {
            foreach (IProperty property in invariantProperties)
            {
                var currentValue = existingContent?.Properties.First(x => x.Alias == property.Alias)
                    .GetValue(null, null, false);
                property.SetValue(currentValue, null, null);
            }
        }

        // if the property does not vary by culture and the data editor supports variance within invariant property values,
        // we need perform a merge between the edited property value and the current property value
        foreach ((IProperty Property, IDataEditor DataEditor) propertyWithEditor in invariantWithVariantSupportProperties)
        {
            var currentValue = existingContent?.Properties.First(x => x.Alias == propertyWithEditor.Property.Alias)
                .GetValue(null, null, false);
            var editedValue = contentWithPotentialUnallowedChanges.Properties
                .First(x => x.Alias == propertyWithEditor.Property.Alias).GetValue(null, null, false);

            // update the editedValue with a merged value of invariant data and allowed culture data using the currentValue as a fallback.
            var mergedValue = propertyWithEditor.DataEditor.MergeVariantInvariantPropertyValue(
                currentValue,
                editedValue,
                ContentSettings.AllowEditInvariantFromNonDefault || (defaultLanguage is not null && allowedCultures.Contains(defaultLanguage.IsoCode)),
                allowedCultures);

            propertyWithEditor.Property.SetValue(mergedValue, null, null);
        }

        return contentWithPotentialUnallowedChanges;
    }

    private async Task<HashSet<string>> GetAllowedCulturesForEditingUser(Guid userKey)
    {
        IUser? user = await _userService.GetAsync(userKey)
            ?? throw new InvalidOperationException($"Could not find user by key {userKey} when editing or validating content.");

        var allowedLanguageIds = user.CalculateAllowedLanguageIds(_localizationService)!;

        return (await _languageService.GetIsoCodesByIdsAsync(allowedLanguageIds)).ToHashSet();
    }

    /// <inheritdoc />
    public async Task<Attempt<ContentUpdateResult, ContentEditingOperationStatus>> UpdateAsync(Guid key, ContentUpdateModel updateModel, Guid userKey)
        => ToEditingAttempt(await HandleUpdateAsync(key, updateModel, null, userKey));

    /// <inheritdoc />
    public async Task<Attempt<ContentUpdateResult, ContentEditingAndPublishingStatus>> UpdateAndPublishAsync(Guid key, ContentUpdateModel updateModel, ISet<string> culturesToPublish, Guid userKey)
        => await HandleUpdateAsync(key, updateModel, culturesToPublish, userKey);

    /// <inheritdoc />
    [Obsolete("Use the overload taking an ISet<string> of cultures to publish, which reports the save and the publish outcome separately. Scheduled for removal in Umbraco 19.")]
    public async Task<Attempt<ContentUpdateResult, ContentEditingOperationStatus>> UpdateAndPublishAsync(Guid key, ContentUpdateModel updateModel, string[] culturesToPublish, Guid userKey)
        => ToEditingAttempt(await HandleUpdateAsync(key, updateModel, culturesToPublish.ToHashSet(), userKey));

    private async Task<Attempt<ContentUpdateResult, ContentEditingAndPublishingStatus>> HandleUpdateAsync(Guid key, ContentUpdateModel updateModel, ISet<string>? culturesToPublish, Guid userKey)
    {
        IContent? content = ContentService.GetById(key);
        if (content is null)
        {
            return Attempt.FailWithStatus(EditingStatus(ContentEditingOperationStatus.NotFound), new ContentUpdateResult());
        }

        if (await ValidateCulturesAsync(updateModel) is false)
        {
            return Attempt.FailWithStatus(EditingStatus(ContentEditingOperationStatus.InvalidCulture), new ContentUpdateResult { Content = content });
        }

        Attempt<ContentUpdateResult, ContentEditingOperationStatus> result = await MapUpdate<ContentUpdateResult>(content, updateModel);
        if (result.Success == false)
        {
            return Attempt.FailWithStatus(EditingStatus(result.Status), result.Result);
        }

        // the update mapping might succeed, but this doesn't mean the model is valid at property level.
        // we'll return the actual property validation status if the entire operation succeeds.
        ContentEditingOperationStatus validationStatus = result.Status;
        ContentValidationResult validationResult = result.Result.ValidationResult;

        content = await EnsureOnlyAllowedFieldsAreUpdated(content, userKey);

        ContentEditingOperationStatus updateTemplateStatus = await UpdateTemplateAsync(content, updateModel.TemplateKey);
        if (updateTemplateStatus != ContentEditingOperationStatus.Success)
        {
            return Attempt.FailWithStatus(
                EditingStatus(updateTemplateStatus),
                new ContentUpdateResult { Content = content, ValidationResult = validationResult });
        }

        (ContentEditingAndPublishingStatus saveStatus, IEnumerable<string> invalidPropertyAliases) = culturesToPublish is null
            ? (EditingStatus(await Save(content, userKey)), Enumerable.Empty<string>())
            : await SaveAndPublish(content, culturesToPublish, userKey);
        return IsSuccess(saveStatus)
            ? Attempt.SucceedWithStatus(
                new ContentEditingAndPublishingStatus
                {
                    ContentEditingOperationStatus = validationStatus,
                    ContentPublishingOperationStatus = saveStatus.ContentPublishingOperationStatus,
                },
                new ContentUpdateResult { Content = content, ValidationResult = validationResult })
            : Attempt.FailWithStatus(
                saveStatus,
                new ContentUpdateResult
                {
                    Content = content,
                    ValidationResult = validationResult,
                    InvalidPropertyAliases = invalidPropertyAliases,
                });
    }

    /// <inheritdoc />
    public async Task<Attempt<IContent?, ContentEditingOperationStatus>> MoveToRecycleBinAsync(Guid key, Guid userKey)
        => await HandleMoveToRecycleBinAsync(key, userKey);

    /// <inheritdoc />
    public async Task<Attempt<IContent?, ContentEditingOperationStatus>> DeleteFromRecycleBinAsync(Guid key, Guid userKey)
        => await HandleDeleteAsync(key, userKey,true);

    /// <inheritdoc />
    public async Task<Attempt<IContent?, ContentEditingOperationStatus>> DeleteAsync(Guid key, Guid userKey)
        => await HandleDeleteAsync(key, userKey,false);

    /// <inheritdoc />
    public async Task<Attempt<IContent?, ContentEditingOperationStatus>> MoveAsync(Guid key, Guid? parentKey, Guid userKey)
        => await HandleMoveAsync(key, parentKey, userKey);

    /// <inheritdoc />
    [Obsolete("Use the overload that takes an includeDescendants parameter instead. Scheduled for removal in Umbraco 19.")]
    public async Task<Attempt<IContent?, ContentEditingOperationStatus>> RestoreAsync(Guid key, Guid? parentKey, Guid userKey)
        => await RestoreAsync(key, parentKey, userKey, true);

    /// <inheritdoc />
    public async Task<Attempt<IContent?, ContentEditingOperationStatus>> RestoreAsync(Guid key, Guid? parentKey, Guid userKey, bool includeDescendants)
        => await HandleMoveAsync(key, parentKey, userKey, true, includeDescendants);

    /// <inheritdoc />
    public async Task<Attempt<IContent?, ContentEditingOperationStatus>> CopyAsync(Guid key, Guid? parentKey, bool relateToOriginal, bool includeDescendants, Guid userKey)
        => await HandleCopyAsync(key, parentKey, relateToOriginal, includeDescendants, userKey);

    /// <inheritdoc />
    public async Task<ContentEditingOperationStatus> SortAsync(
        Guid? parentKey,
        IEnumerable<SortingModel> sortingModels,
        Guid userKey)
        => await HandleSortAsync(parentKey, sortingModels, userKey);

    /// <inheritdoc />
    public async Task<ContentEditingOperationStatus> SortByFieldAsync(
        Guid? parentKey,
        ContentSortField field,
        Direction direction,
        string? culture,
        Guid userKey)
        => await HandleSortByFieldAsync(parentKey, field, direction, culture, userKey);

    private async Task<Attempt<ContentValidationResult, ContentEditingOperationStatus>> ValidateCulturesAndPropertiesAsync(
        ContentEditingModelBase contentEditingModelBase,
        Guid contentTypeKey,
        IEnumerable<string?>? culturesToValidate = null)
        => await ValidateCulturesAsync(contentEditingModelBase) is false
            ? Attempt.FailWithStatus(ContentEditingOperationStatus.InvalidCulture, new ContentValidationResult())
            : await ValidatePropertiesAsync(contentEditingModelBase, contentTypeKey, culturesToValidate);

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

    /// <inheritdoc />
    protected override IContent New(string? name, int parentId, IContentType contentType)
        => new Content(name, parentId, contentType);

    /// <inheritdoc />
    protected override OperationResult? Move(IContent content, int newParentId, bool includeDescendants, int userId)
        => ContentService.Move(content, newParentId, includeDescendants, userId);

    /// <inheritdoc />
    protected override IContent? Copy(IContent content, int newParentId, bool relateToOriginal, bool includeDescendants, int userId)
        => ContentService.Copy(content, newParentId, relateToOriginal, includeDescendants, userId);

    /// <inheritdoc />
    protected override OperationResult? MoveToRecycleBin(IContent content, int userId) => ContentService.MoveToRecycleBin(content, userId);

    /// <inheritdoc />
    protected override OperationResult? Delete(IContent content, int userId) => ContentService.Delete(content, userId);

    /// <inheritdoc />
    protected override IEnumerable<IContent> GetPagedChildren(int parentId, int pageIndex, int pageSize, Ordering? ordering, out long total)
        => ContentService.GetPagedChildren(parentId, pageIndex, pageSize, out total, propertyAliases: null, filter: null, ordering: ordering);

    /// <inheritdoc />
    protected override ContentEditingOperationStatus Sort(IEnumerable<IContent> items, int userId)
    {
        OperationResult result = ContentService.Sort(items, userId);
        return OperationResultToOperationStatus(result);
    }

    /// <inheritdoc />
    protected override ContentEditingOperationStatus SortChildrenInBulk(int parentId, IReadOnlyList<int> orderedChildIds, int userId)
    {
        OperationResult result = ContentService.SortChildren(parentId, orderedChildIds, userId);
        return OperationResultToOperationStatus(result);
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
                _ => ContentEditingOperationStatus.Unknown,
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Content save operation failed");
            return ContentEditingOperationStatus.Unknown;
        }
    }

    private async Task<(ContentEditingAndPublishingStatus Status, IEnumerable<string> InvalidPropertyAliases)> SaveAndPublish(IContent content, ISet<string> culturesToPublish, Guid userKey)
    {
        // The cultures to publish must match the content type's variance, or the publish cannot be attempted at all.
        // Checked up-front so the caller gets the reason: the underlying service signals this by throwing, which would
        // otherwise be caught below and reported as an unknown error.
        ContentEditingOperationStatus? invalidCulturesStatus = await ValidateCulturesToPublishAsync(content, culturesToPublish);
        if (invalidCulturesStatus is not null)
        {
            return (EditingStatus(invalidCulturesStatus.Value), Enumerable.Empty<string>());
        }

        try
        {
            var currentUserId = await GetUserIdAsync(userKey);
            PublishResult publishResult = ContentService.SaveAndPublish(content, culturesToPublish.ToArray(), userId: currentUserId);
            if (publishResult.Success)
            {
                return (
                    new ContentEditingAndPublishingStatus
                    {
                        ContentEditingOperationStatus = ContentEditingOperationStatus.Success,
                        ContentPublishingOperationStatus = ContentPublishingOperationStatus.Success,
                    },
                    Enumerable.Empty<string>());
            }

            // Some failures are returned before the document is persisted, so they cannot be reported against the
            // publishing status: doing so would state that the save succeeded when nothing was written at all.
            ContentEditingOperationStatus? nothingPersistedStatus = NothingPersistedStatus(publishResult.Result);
            if (nothingPersistedStatus is not null)
            {
                return (EditingStatus(nothingPersistedStatus.Value), Enumerable.Empty<string>());
            }

            // Any other failure means the publish was rejected after the save took effect, so report the save as
            // successful and let the publishing status carry the reason.
            return (
                new ContentEditingAndPublishingStatus
                {
                    ContentEditingOperationStatus = ContentEditingOperationStatus.Success,
                    ContentPublishingOperationStatus = publishResult.ToContentPublishingOperationStatus(),
                },
                publishResult.InvalidProperties?.Select(property => property.Alias).ToArray() ?? Enumerable.Empty<string>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Content save operation failed");
            return (EditingStatus(ContentEditingOperationStatus.Unknown), Enumerable.Empty<string>());
        }
    }

    /// <summary>
    ///     Validates the cultures requested for publishing against the content type's variance and the configured
    ///     languages, returning <c>null</c> when they are acceptable.
    /// </summary>
    private async Task<ContentEditingOperationStatus?> ValidateCulturesToPublishAsync(IContent content, ISet<string> culturesToPublish)
    {
        if (culturesToPublish.Any(culture => culture.IsNullOrWhiteSpace() || culture == "*"))
        {
            return ContentEditingOperationStatus.InvalidCulture;
        }

        if (content.ContentType.VariesByCulture() is false)
        {
            return culturesToPublish.Count > 0
                ? ContentEditingOperationStatus.ContentTypeCultureVarianceMismatch
                : null;
        }

        // Publishing an unconfigured culture would otherwise silently publish nothing at all.
        IEnumerable<string> configuredCultures = await _languageService.GetAllIsoCodesAsync();
        return culturesToPublish.Except(configuredCultures).Any()
            ? ContentEditingOperationStatus.InvalidCulture
            : null;
    }

    /// <summary>
    ///     Gets the editing status for a publish result that is returned before the document is persisted, or
    ///     <c>null</c> when the result implies the save took effect.
    /// </summary>
    /// <remarks>
    ///     These are the results <see cref="IContentService.SaveAndPublish"/> can return without having written
    ///     anything: a handler cancelling the saving, publishing or unpublishing notification, and a concurrency
    ///     violation. Every other failure is raised after the document has been saved.
    /// </remarks>
    private static ContentEditingOperationStatus? NothingPersistedStatus(PublishResultType resultType)
        => resultType switch
        {
            // The saving and publishing notifications are both raised before persistence, and the two cancel points
            // are indistinguishable in the result.
            PublishResultType.FailedPublishCancelledByEvent or PublishResultType.FailedUnpublishCancelledByEvent
                => ContentEditingOperationStatus.CancelledByNotification,
            PublishResultType.FailedPublishConcurrencyViolation
                => ContentEditingOperationStatus.ConcurrencyViolation,
            _ => null,
        };

    private static ContentEditingAndPublishingStatus EditingStatus(ContentEditingOperationStatus status)
        => new() { ContentEditingOperationStatus = status };

    private static bool IsSuccess(ContentEditingAndPublishingStatus status)
        => status.ContentEditingOperationStatus is ContentEditingOperationStatus.Success
           && status.ContentPublishingOperationStatus is null or ContentPublishingOperationStatus.Success;

    /// <summary>
    ///     Projects a combined status onto the editing status alone, for the save-only operations and for the obsolete
    ///     overloads that predate the combined status.
    /// </summary>
    // TODO (V19): Remove the collapse to "unknown" below when the obsolete CreateAndPublishAsync and
    // UpdateAndPublishAsync overloads taking a string[] of cultures to publish are removed. The remaining callers -
    // CreateAsync and UpdateAsync - do not publish, so their publishing status is always null and this method
    // reduces to projecting the editing status.
    private static Attempt<TResult, ContentEditingOperationStatus> ToEditingAttempt<TResult>(Attempt<TResult, ContentEditingAndPublishingStatus> attempt)
    {
        if (attempt.Success)
        {
            return Attempt.SucceedWithStatus(attempt.Status.ContentEditingOperationStatus, attempt.Result);
        }

        // The editing status cannot express a publish failure, so it collapses to "unknown" - which is precisely why the
        // combined status exists. Retained here so the obsolete overloads keep behaving as they did.
        ContentEditingOperationStatus status =
            attempt.Status.ContentEditingOperationStatus is ContentEditingOperationStatus.Success
            && attempt.Status.ContentPublishingOperationStatus is not null
                ? ContentEditingOperationStatus.Unknown
                : attempt.Status.ContentEditingOperationStatus;

        return Attempt.FailWithStatus(status, attempt.Result);
    }
}
