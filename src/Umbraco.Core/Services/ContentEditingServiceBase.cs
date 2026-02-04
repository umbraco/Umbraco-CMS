using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Extensions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.Editors;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services.Filters;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Services;

/// <summary>
/// Base class for content editing services that provides common functionality for creating, updating,
/// and managing content entities (documents, media, members).
/// </summary>
/// <typeparam name="TContent">The type of content entity.</typeparam>
/// <typeparam name="TContentType">The type of content type.</typeparam>
/// <typeparam name="TContentService">The type of content service.</typeparam>
/// <typeparam name="TContentTypeService">The type of content type service.</typeparam>
internal abstract class ContentEditingServiceBase<TContent, TContentType, TContentService, TContentTypeService>
    where TContent : class, IContentBase
    where TContentType : class, IContentTypeComposition
    where TContentService : IContentServiceBase<TContent>
    where TContentTypeService : IContentTypeBaseService<TContentType>
{
    private readonly PropertyEditorCollection _propertyEditorCollection;
    private readonly IDataTypeService _dataTypeService;
    private readonly ILogger<ContentEditingServiceBase<TContent, TContentType, TContentService, TContentTypeService>> _logger;
    private readonly IUserIdKeyResolver _userIdKeyResolver;
    private readonly IContentValidationServiceBase<TContentType> _validationService;
    private readonly IRelationService _relationService;
    private readonly ContentTypeFilterCollection _contentTypeFilters;
    private readonly ILanguageService _languageService;
    private readonly IUserService _userService;
    private readonly ILocalizationService _localizationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ContentEditingServiceBase{TContent, TContentType, TContentService, TContentTypeService}"/> class.
    /// </summary>
    /// <param name="contentService">The content service.</param>
    /// <param name="contentTypeService">The content type service.</param>
    /// <param name="propertyEditorCollection">The property editor collection.</param>
    /// <param name="dataTypeService">The data type service.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="scopeProvider">The scope provider.</param>
    /// <param name="userIdKeyResolver">The user ID key resolver.</param>
    /// <param name="validationService">The validation service.</param>
    /// <param name="optionsMonitor">The content settings options monitor.</param>
    /// <param name="relationService">The relation service.</param>
    /// <param name="contentTypeFilters">The content type filter collection.</param>
    protected ContentEditingServiceBase(
        TContentService contentService,
        TContentTypeService contentTypeService,
        PropertyEditorCollection propertyEditorCollection,
        IDataTypeService dataTypeService,
        ILogger<ContentEditingServiceBase<TContent, TContentType, TContentService, TContentTypeService>> logger,
        ICoreScopeProvider scopeProvider,
        IUserIdKeyResolver userIdKeyResolver,
        IContentValidationServiceBase<TContentType> validationService,
        IOptionsMonitor<ContentSettings> optionsMonitor,
        IRelationService relationService,
        ContentTypeFilterCollection contentTypeFilters,
        ILanguageService languageService,
        IUserService userService,
        ILocalizationService localizationService)
    {
        _propertyEditorCollection = propertyEditorCollection;
        _dataTypeService = dataTypeService;
        _logger = logger;
        _userIdKeyResolver = userIdKeyResolver;
        _validationService = validationService;
        ContentSettings = optionsMonitor.CurrentValue;
        optionsMonitor.OnChange((contentSettings) =>
        {
            ContentSettings = contentSettings;
        });

        _relationService = relationService;
        CoreScopeProvider = scopeProvider;
        ContentService = contentService;
        ContentTypeService = contentTypeService;
        _contentTypeFilters = contentTypeFilters;
        _languageService = languageService;
        _userService = userService;
        _localizationService = localizationService;
    }

    /// <summary>
    /// Creates a new content entity.
    /// </summary>
    /// <param name="name">The name of the content.</param>
    /// <param name="parentId">The parent identifier.</param>
    /// <param name="contentType">The content type.</param>
    /// <returns>A new content entity.</returns>
    protected abstract TContent New(string? name, int parentId, TContentType contentType);

    /// <summary>
    /// Moves content to a new parent.
    /// </summary>
    /// <param name="content">The content to move.</param>
    /// <param name="newParentId">The new parent identifier.</param>
    /// <param name="userId">The user performing the operation.</param>
    /// <returns>The operation result.</returns>
    protected abstract OperationResult? Move(TContent content, int newParentId, int userId);

    /// <summary>
    /// Copies content to a new parent.
    /// </summary>
    /// <param name="content">The content to copy.</param>
    /// <param name="newParentId">The new parent identifier.</param>
    /// <param name="relateToOriginal">Whether to create a relation to the original.</param>
    /// <param name="includeDescendants">Whether to include descendants in the copy.</param>
    /// <param name="userId">The user performing the operation.</param>
    /// <returns>The copied content, or null if the operation failed.</returns>
    protected abstract TContent? Copy(TContent content, int newParentId, bool relateToOriginal, bool includeDescendants, int userId);

    /// <summary>
    /// Moves content to the recycle bin.
    /// </summary>
    /// <param name="content">The content to move to recycle bin.</param>
    /// <param name="userId">The user performing the operation.</param>
    /// <returns>The operation result.</returns>
    protected abstract OperationResult? MoveToRecycleBin(TContent content, int userId);

    /// <summary>
    /// Deletes content.
    /// </summary>
    /// <param name="content">The content to delete.</param>
    /// <param name="userId">The user performing the operation.</param>
    /// <returns>The operation result.</returns>
    protected abstract OperationResult? Delete(TContent content, int userId);

    /// <summary>
    /// Gets the current content settings.
    /// </summary>
    protected ContentSettings ContentSettings { get; private set; }

    /// <summary>
    /// Gets the core scope provider.
    /// </summary>
    protected ICoreScopeProvider CoreScopeProvider { get; }

    /// <summary>
    /// Gets the content service.
    /// </summary>
    protected TContentService ContentService { get; }

    /// <summary>
    /// Gets the content type service.
    /// </summary>
    protected TContentTypeService ContentTypeService { get; }

    /// <summary>
    /// Gets the alias used to relate the parent entity when handling content (document or media) delete operations.
    /// </summary>
    protected virtual string? RelateParentOnDeleteAlias => null;

    /// <summary>
    /// Maps a content creation model to a new content entity.
    /// </summary>
    /// <typeparam name="TContentCreateResult">The type of the creation result.</typeparam>
    /// <param name="contentCreationModelBase">The content creation model.</param>
    /// <returns>An attempt containing the creation result and operation status.</returns>
    protected async Task<Attempt<TContentCreateResult, ContentEditingOperationStatus>> MapCreate<TContentCreateResult>(ContentCreationModelBase contentCreationModelBase)
        where TContentCreateResult : ContentCreateResultBase<TContent>, new()
    {
        TContentType? contentType = TryGetAndValidateContentType(contentCreationModelBase.ContentTypeKey, contentCreationModelBase, out ContentEditingOperationStatus validationOperationStatus);
        if (contentType == null)
        {
            return Attempt.FailWithStatus(validationOperationStatus, new TContentCreateResult());
        }

        (int? ParentId, ContentEditingOperationStatus OperationStatus) parent = await TryGetAndValidateParentIdAsync(contentCreationModelBase.ParentKey, contentType);
        if (parent.OperationStatus != ContentEditingOperationStatus.Success)
        {
            return Attempt.FailWithStatus(parent.OperationStatus, new TContentCreateResult());
        }

        // NOTE: property level validation errors must NOT fail the update - it must be possible to save invalid properties.
        //       instead, the error state and validation errors will be communicated in the return value.
        Attempt<ContentValidationResult, ContentEditingOperationStatus> validationResult = await ValidatePropertiesAsync(contentCreationModelBase, contentType);

        TContent content = New(null, parent.ParentId ?? Constants.System.Root, contentType);
        if (contentCreationModelBase.Key.HasValue)
        {
            content.Key = contentCreationModelBase.Key.Value;
        }

        UpdateNames(contentCreationModelBase, content, contentType);
        await UpdateExistingProperties(contentCreationModelBase, content, contentType);

        return Attempt.SucceedWithStatus(validationResult.Status, new TContentCreateResult { Content = content, ValidationResult = validationResult.Result });
    }

    /// <summary>
    /// Maps a content editing model to an existing content entity for update.
    /// </summary>
    /// <typeparam name="TContentUpdateResult">The type of the update result.</typeparam>
    /// <param name="content">The existing content entity to update.</param>
    /// <param name="contentEditingModelBase">The content editing model.</param>
    /// <returns>An attempt containing the update result and operation status.</returns>
    protected async Task<Attempt<TContentUpdateResult, ContentEditingOperationStatus>> MapUpdate<TContentUpdateResult>(TContent content, ContentEditingModelBase contentEditingModelBase)
        where TContentUpdateResult : ContentUpdateResultBase<TContent>, new()
    {
        TContentType? contentType = TryGetAndValidateContentType(content.ContentType.Key, contentEditingModelBase, out ContentEditingOperationStatus operationStatus);
        if (contentType == null)
        {
            return Attempt.FailWithStatus(operationStatus, new TContentUpdateResult { Content = content });
        }

        // NOTE: property level validation errors must NOT fail the update - it must be possible to save invalid properties.
        //       instead, the error state and validation errors will be communicated in the return value.
        Attempt<ContentValidationResult, ContentEditingOperationStatus> validationResult = await ValidatePropertiesAsync(contentEditingModelBase, contentType);

        UpdateNames(contentEditingModelBase, content, contentType);
        await UpdateExistingProperties(contentEditingModelBase, content, contentType);
        RemoveMissingProperties(contentEditingModelBase, content, contentType);

        return Attempt.SucceedWithStatus(validationResult.Status, new TContentUpdateResult { Content = content, ValidationResult = validationResult.Result });
    }

    /// <summary>
    /// Validates the cultures in the content editing model.
    /// </summary>
    /// <param name="contentEditingModelBase">The content editing model to validate.</param>
    /// <returns><c>true</c> if all cultures are valid; otherwise, <c>false</c>.</returns>
    protected async Task<bool> ValidateCulturesAsync(ContentEditingModelBase contentEditingModelBase)
        => await _validationService.ValidateCulturesAsync(contentEditingModelBase);

    protected async Task<Attempt<ContentValidationResult, ContentEditingOperationStatus>> ValidateCulturesAndPropertiesAsync(
        ContentEditingModelBase contentEditingModelBase,
        Guid contentTypeKey,
        IEnumerable<string?>? cultures,
        Guid userKey)
    {
        if (await ValidateCulturesAsync(contentEditingModelBase) is false)
        {
            return Attempt.FailWithStatus(ContentEditingOperationStatus.InvalidCulture, new ContentValidationResult());
        }

        IEnumerable<string?>? culturesToValidate = await GetCulturesToValidate(cultures, userKey);
        return await ValidatePropertiesAsync(contentEditingModelBase, contentTypeKey, culturesToValidate);
    }

    /// <summary>
    /// Validates the properties in the content editing model against the content type.
    /// </summary>
    /// <param name="contentEditingModelBase">The content editing model to validate.</param>
    /// <param name="contentTypeKey">The content type key.</param>
    /// <param name="culturesToValidate">Optional cultures to restrict validation to.</param>
    /// <returns>An attempt containing the validation result and operation status.</returns>
    protected async Task<Attempt<ContentValidationResult, ContentEditingOperationStatus>> ValidatePropertiesAsync(
        ContentEditingModelBase contentEditingModelBase,
        Guid contentTypeKey,
        IEnumerable<string?>? culturesToValidate = null)
    {
        TContentType? contentType = await ContentTypeService.GetAsync(contentTypeKey);
        if (contentType is null)
        {
            return Attempt.FailWithStatus(ContentEditingOperationStatus.ContentTypeNotFound, new ContentValidationResult());
        }

        return await ValidatePropertiesAsync(contentEditingModelBase, contentType, culturesToValidate);
    }

    private async Task<Attempt<ContentValidationResult, ContentEditingOperationStatus>> ValidatePropertiesAsync(
        ContentEditingModelBase contentEditingModelBase,
        TContentType contentType,
        IEnumerable<string?>? culturesToValidate = null)
    {
        ContentValidationResult result = await _validationService.ValidatePropertiesAsync(contentEditingModelBase, contentType, culturesToValidate);
        return result.ValidationErrors.Any() is false
            ? Attempt.SucceedWithStatus(ContentEditingOperationStatus.Success, result)
            : Attempt.FailWithStatus(ContentEditingOperationStatus.PropertyValidationError, result);
    }

    protected async Task<IEnumerable<string?>?> GetCulturesToValidate(IEnumerable<string?>? cultures, Guid userKey)
    {
        // Cultures to validate can be provided by the calling code, but if the editor is restricted to only have
        // access to certain languages, we don't want to validate by any they aren't allowed to edit.
        HashSet<string> allowedCultures = await GetAllowedCulturesForEditingUser(userKey);

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

    /// <summary>
    /// Handles moving content to the recycle bin.
    /// </summary>
    /// <param name="key">The content key.</param>
    /// <param name="userKey">The user key performing the operation.</param>
    /// <returns>An attempt containing the content and operation status.</returns>
    protected async Task<Attempt<TContent?, ContentEditingOperationStatus>> HandleMoveToRecycleBinAsync(Guid key, Guid userKey)
        => await HandleDeletionAsync(
                key,
                userKey,
                ContentTrashStatusRequirement.MustNotBeTrashed,
                MoveToRecycleBin,
                ContentSettings.DisableUnpublishWhenReferenced,
                ContentEditingOperationStatus.CannotMoveToRecycleBinWhenReferenced);

    /// <summary>
    /// Handles deleting content.
    /// </summary>
    /// <param name="key">The content key.</param>
    /// <param name="userKey">The user key performing the operation.</param>
    /// <param name="mustBeTrashed">Whether the content must be in the recycle bin to be deleted.</param>
    /// <returns>An attempt containing the content and operation status.</returns>
    protected async Task<Attempt<TContent?, ContentEditingOperationStatus>> HandleDeleteAsync(Guid key, Guid userKey, bool mustBeTrashed = true)
        => await HandleDeletionAsync(
                key,
                userKey,
                mustBeTrashed
                    ? ContentTrashStatusRequirement.MustBeTrashed
                    : ContentTrashStatusRequirement.Irrelevant,
                Delete,
                ContentSettings.DisableDeleteWhenReferenced,
                ContentEditingOperationStatus.CannotDeleteWhenReferenced);

    // helper method to perform move-to-recycle-bin, delete-from-recycle-bin and delete for content as they are very much handled in the same way
    // IContentEditingService methods hitting this (ContentTrashStatusRequirement, calledFunction):
    // DeleteAsync (irrelevant, Delete)
    // MoveToRecycleBinAsync (MustNotBeTrashed, MoveToRecycleBin)
    // DeleteFromRecycleBinAsync (MustBeTrashed, Delete)
    private async Task<Attempt<TContent?, ContentEditingOperationStatus>> HandleDeletionAsync(
        Guid key,
        Guid userKey,
        ContentTrashStatusRequirement trashStatusRequirement,
        Func<TContent, int, OperationResult?> performDelete,
        bool disabledWhenReferenced,
        ContentEditingOperationStatus referenceFailStatus)
    {
        using ICoreScope scope = CoreScopeProvider.CreateCoreScope();
        TContent? content = ContentService.GetById(key);
        if (content == null)
        {
            return Attempt.FailWithStatus(ContentEditingOperationStatus.NotFound, content);
        }

        // checking the trash status is not done when it is irrelevant
        if ((trashStatusRequirement is ContentTrashStatusRequirement.MustBeTrashed && content.Trashed is false)
            || (trashStatusRequirement is ContentTrashStatusRequirement.MustNotBeTrashed && content.Trashed is true))
        {
            ContentEditingOperationStatus status = trashStatusRequirement is ContentTrashStatusRequirement.MustBeTrashed
                ? ContentEditingOperationStatus.NotInTrash
                : ContentEditingOperationStatus.InTrash;
            return Attempt.FailWithStatus<TContent?, ContentEditingOperationStatus>(status, content);
        }

        if (disabledWhenReferenced)
        {
            // When checking if an item is related, we may need to exclude the "relate parent on delete" relation type, as this prevents
            // deleting from the recycle bin.
            int[]? excludeRelationTypeIds = null;
            if (string.IsNullOrWhiteSpace(RelateParentOnDeleteAlias) is false)
            {
                IRelationType? relateParentOnDeleteRelationType = _relationService.GetRelationTypeByAlias(RelateParentOnDeleteAlias);
                if (relateParentOnDeleteRelationType is not null)
                {
                    excludeRelationTypeIds = [relateParentOnDeleteRelationType.Id];
                }
            }

            if (_relationService.IsRelated(
                content.Id,
                RelationDirectionFilter.Child,
                excludeRelationTypeIds: excludeRelationTypeIds))
            {
                return Attempt.FailWithStatus<TContent?, ContentEditingOperationStatus>(referenceFailStatus, content);
            }
        }

        var userId = await GetUserIdAsync(userKey);
        OperationResult? deleteResult = performDelete(content, userId);

        scope.Complete();

        return OperationResultToAttempt(content, deleteResult);
    }

    /// <summary>
    /// Handles moving content to a new parent.
    /// </summary>
    /// <param name="key">The content key.</param>
    /// <param name="parentKey">The new parent key, or null for root.</param>
    /// <param name="userKey">The user key performing the operation.</param>
    /// <param name="mustBeInRecycleBin">Whether the content must be in the recycle bin (for restore operations).</param>
    /// <returns>An attempt containing the content and operation status.</returns>
    protected async Task<Attempt<TContent?, ContentEditingOperationStatus>> HandleMoveAsync(Guid key, Guid? parentKey, Guid userKey, bool mustBeInRecycleBin = false)
    {
        using ICoreScope scope = CoreScopeProvider.CreateCoreScope();
        TContent? content = ContentService.GetById(key);
        if (content is null)
        {
            return Attempt.FailWithStatus(ContentEditingOperationStatus.NotFound, content);
        }

        if (mustBeInRecycleBin && content.Trashed is false)
        {
            return Attempt.FailWithStatus<TContent?, ContentEditingOperationStatus>(ContentEditingOperationStatus.NotInTrash, content);
        }

        TContentType contentType = ContentTypeService.Get(content.ContentType.Key)!;

        (int? ParentId, ContentEditingOperationStatus OperationStatus) parent = await TryGetAndValidateParentIdAsync(parentKey, contentType);
        if (parent.OperationStatus != ContentEditingOperationStatus.Success)
        {
            return Attempt.FailWithStatus<TContent?, ContentEditingOperationStatus>(parent.OperationStatus, content);
        }

        // special case for move: short circuit the operation if the content is already under the correct parent.
        if ((parent.ParentId == null && content.ParentId == Constants.System.Root) || (parent.ParentId != null && parent.ParentId == content.ParentId))
        {
            return Attempt.SucceedWithStatus<TContent?, ContentEditingOperationStatus>(ContentEditingOperationStatus.Success, content);
        }

        // special case for move: do not allow moving an item beneath itself.
        if (parentKey.HasValue)
        {
            // at this point the parent MUST exist - unless someone starts using this move method
            // e.g. for blueprints (which should be handled elsewhere).
            TContent parentContent = ContentService.GetById(parentKey.Value) ?? throw new InvalidOperationException("The content parent ID was validated, but the parent was not found");
            if (parentContent.Path.Split(Constants.CharArrays.Comma).Select(int.Parse).Contains(content.Id) is true)
            {
                return Attempt.FailWithStatus<TContent?, ContentEditingOperationStatus>(ContentEditingOperationStatus.ParentInvalid, content);
            }
        }

        var userId = await GetUserIdAsync(userKey);
        OperationResult? moveResult = Move(content, parent.ParentId ?? Constants.System.Root, userId);

        scope.Complete();

        return OperationResultToAttempt(content, moveResult);
    }

    /// <summary>
    /// Handles copying content to a new parent.
    /// </summary>
    /// <param name="key">The content key to copy.</param>
    /// <param name="parentKey">The new parent key, or null for root.</param>
    /// <param name="relateToOriginal">Whether to create a relation to the original.</param>
    /// <param name="includeDescendants">Whether to include descendants in the copy.</param>
    /// <param name="userKey">The user key performing the operation.</param>
    /// <returns>An attempt containing the copied content and operation status.</returns>
    protected async Task<Attempt<TContent?, ContentEditingOperationStatus>> HandleCopyAsync(Guid key, Guid? parentKey, bool relateToOriginal, bool includeDescendants, Guid userKey)
    {
        using ICoreScope scope = CoreScopeProvider.CreateCoreScope();
        TContent? content = ContentService.GetById(key);
        if (content is null)
        {
            return Attempt.FailWithStatus(ContentEditingOperationStatus.NotFound, content);
        }

        TContentType contentType = ContentTypeService.Get(content.ContentType.Key)!;

        (int? ParentId, ContentEditingOperationStatus OperationStatus) parent = await TryGetAndValidateParentIdAsync(parentKey, contentType);
        if (parent.OperationStatus != ContentEditingOperationStatus.Success)
        {
            return Attempt.FailWithStatus<TContent?, ContentEditingOperationStatus>(parent.OperationStatus, content);
        }

        var userId = await GetUserIdAsync(userKey);
        TContent? copy = Copy(content, parent.ParentId ?? Constants.System.Root, relateToOriginal, includeDescendants, userId);
        scope.Complete();

        // we'll assume that we have performed all validations for unsuccessful scenarios above, so a null result here
        // means the copy operation was cancelled by a notification handler
        return copy != null
            ? Attempt.SucceedWithStatus<TContent?, ContentEditingOperationStatus>(ContentEditingOperationStatus.Success, copy)
            : Attempt.FailWithStatus<TContent?, ContentEditingOperationStatus>(ContentEditingOperationStatus.CancelledByNotification, null);
    }

    private Attempt<TContent?, ContentEditingOperationStatus> OperationResultToAttempt(TContent? content, OperationResult? operationResult)
    {
        ContentEditingOperationStatus operationStatus = OperationResultToOperationStatus(operationResult);
        return operationStatus == ContentEditingOperationStatus.Success
            ? Attempt.SucceedWithStatus(operationStatus, content)
            : Attempt.FailWithStatus(operationStatus, content);
    }

    /// <summary>
    /// Converts an operation result to a content editing operation status.
    /// </summary>
    /// <param name="operationResult">The operation result to convert.</param>
    /// <returns>The corresponding content editing operation status.</returns>
    protected ContentEditingOperationStatus OperationResultToOperationStatus(OperationResult? operationResult)
        => operationResult?.Result switch
        {
            // these are the only result states currently expected from the invoked IContentService operations
            OperationResultType.Success => ContentEditingOperationStatus.Success,
            OperationResultType.FailedCancelledByEvent => ContentEditingOperationStatus.CancelledByNotification,
            OperationResultType.FailedCannot => ContentEditingOperationStatus.CannotDeleteWhenReferenced,

            // for any other state we'll return "unknown" so we know that we need to amend this switch statement
            _ => ContentEditingOperationStatus.Unknown
        };

    /// <summary>
    /// Gets the user ID from the user key.
    /// </summary>
    /// <param name="userKey">The user key.</param>
    /// <returns>The user ID.</returns>
    protected async Task<int> GetUserIdAsync(Guid userKey) => await _userIdKeyResolver.GetAsync(userKey);

    private TContentType? TryGetAndValidateContentType(Guid contentTypeKey, ContentEditingModelBase contentEditingModelBase, out ContentEditingOperationStatus operationStatus)
    {
        TContentType? contentType = ContentTypeService.Get(contentTypeKey);
        if (contentType == null)
        {
            operationStatus = ContentEditingOperationStatus.ContentTypeNotFound;
            return null;
        }

        if (contentType.VariesByNothing() && contentEditingModelBase.Variants.Any(v => v.Culture is null && v.Segment is null) is false)
        {
            // does not vary by anything and is missing the invariant name = invalid
            operationStatus = ContentEditingOperationStatus.ContentTypeCultureVarianceMismatch;
            return null;
        }

        if (contentType.VariesByCulture() && contentEditingModelBase.Variants.Any(v => v.Culture is null))
        {
            // varies by culture with one or more variants not bound to a culture = invalid
            operationStatus = ContentEditingOperationStatus.ContentTypeCultureVarianceMismatch;
            return null;
        }

        if (contentType.VariesBySegment() && contentEditingModelBase.Variants.Any(v => v.Segment is null) is false)
        {
            // varies by segment with no default segment variants = invalid
            operationStatus = ContentEditingOperationStatus.ContentTypeSegmentVarianceMismatch;
            return null;
        }

        var propertyTypesByAlias = contentType.CompositionPropertyTypes.ToDictionary(pt => pt.Alias);
        var propertyValuesAndVariance = contentEditingModelBase
            .Properties
            .Select(pv => new
            {
                VariesByCulture = pv.Culture is not null,
                VariesBySegment = pv.Segment is not null,
                PropertyValue = pv
            })
            .ToArray();

        // verify that all property values are defined as property types
        if (propertyValuesAndVariance.Any(pv => propertyTypesByAlias.ContainsKey(pv.PropertyValue.Alias) == false))
        {
            operationStatus = ContentEditingOperationStatus.PropertyTypeNotFound;
            return null;
        }

        // verify that all properties match their respective property type culture and segment variance - i.e. no culture invariant properties that should have been culture variant
        if (propertyValuesAndVariance.Any(pv =>
            {
                IPropertyType propertyType = propertyTypesByAlias[pv.PropertyValue.Alias];
                return (propertyType.VariesByCulture() != pv.VariesByCulture)
                       || (propertyType.VariesBySegment() is false && pv.VariesBySegment);
            }))
        {
            operationStatus = ContentEditingOperationStatus.PropertyTypeNotFound;
            return null;
        }

        operationStatus = ContentEditingOperationStatus.Success;
        return contentType;
    }

    /// <summary>
    /// Attempts to get and validate the parent ID for content creation or move operations.
    /// </summary>
    /// <param name="parentKey">The parent key, or null for root.</param>
    /// <param name="contentType">The content type being created or moved.</param>
    /// <returns>A tuple containing the parent ID (if valid) and the operation status.</returns>
    protected virtual async Task<(int? ParentId, ContentEditingOperationStatus OperationStatus)> TryGetAndValidateParentIdAsync(Guid? parentKey, TContentType contentType)
    {
        TContent? parent = parentKey.HasValue
            ? ContentService.GetById(parentKey.Value)
            : null;

        if (parentKey.HasValue && parent == null)
        {
            return (null, ContentEditingOperationStatus.ParentNotFound);
        }

        if (parent == null &&
            (contentType.AllowedAsRoot == false ||

            // We could have a content type filter registered that prevents the content from being created at the root level,
            // even if it's allowed in the content type definition.
            await IsAllowedAtRootByContentTypeFilters(contentType) == false))
        {
            return (null, ContentEditingOperationStatus.NotAllowed);
        }

        if (parent != null)
        {
            if (parent.Trashed)
            {
                return (null, ContentEditingOperationStatus.InTrash);
            }

            TContentType? parentContentType = ContentTypeService.Get(parent.ContentType.Key);
            Guid[] allowedContentTypeKeys = parentContentType?.AllowedContentTypes?.Select(c => c.Key).ToArray()
                                            ?? Array.Empty<Guid>();

            if (allowedContentTypeKeys.Contains(contentType.Key) == false ||

                // We could have a content type filter registered that prevents the content from being created as a child,
                // even if it's allowed in the content type definition.
                await IsAllowedAsChildByContentTypeFilters(contentType, parentContentType!.Key, parent.Key) == false)
            {
                return (null, ContentEditingOperationStatus.NotAllowed);
            }
        }

        return (parent?.Id ?? Constants.System.Root, ContentEditingOperationStatus.Success);
    }

    private async Task<bool> IsAllowedAtRootByContentTypeFilters(TContentType contentType)
    {
        IEnumerable<TContentType> filteredContentTypes = [contentType];
        foreach (IContentTypeFilter filter in _contentTypeFilters)
        {
            filteredContentTypes = await filter.FilterAllowedAtRootAsync(filteredContentTypes);
        }

        return filteredContentTypes.Any();
    }

    private async Task<bool> IsAllowedAsChildByContentTypeFilters(TContentType contentType, Guid parentContentTypeKey, Guid parentKey)
    {
        IEnumerable<ContentTypeSort> filteredContentTypes = [new ContentTypeSort(contentType.Key, contentType.SortOrder, contentType.Alias)];
        foreach (IContentTypeFilter filter in _contentTypeFilters)
        {
            filteredContentTypes = await filter.FilterAllowedChildrenAsync(filteredContentTypes, parentContentTypeKey, parentKey);
        }

        return filteredContentTypes.Any();
    }

    private void UpdateNames(ContentEditingModelBase contentEditingModelBase, TContent content, TContentType contentType)
    {
        if (contentType.VariesByCulture())
        {
            // get the content names for each culture, keeping in mind that there may be multiple per culture
            // as each culture can have several segments. we'll prioritize the segment-less names
            var variantNamesByCulture = contentEditingModelBase.Variants
                .Where(v => v.Culture.IsNullOrWhiteSpace() == false)
                .OrderBy(v => v.Segment.IsNullOrWhiteSpace() ? 0 : 1)
                .GroupBy(v => v.Culture!)
                .ToDictionary(g => g.Key, g => g.First().Name);

            // update the content names for all cultures
            foreach (var (culture, name) in variantNamesByCulture)
            {
                content.SetCultureName(name, culture);
            }
        }
        else if (contentType.VariesBySegment())
        {
            // this should be validated already so it's OK to throw an exception here
            content.Name = contentEditingModelBase.Variants.FirstOrDefault(v => v.Segment is null)?.Name
                           ?? throw new ArgumentException("Could not find the default segment variant", nameof(contentEditingModelBase));
        }
        else
        {
            // this should be validated already so it's OK to throw an exception here
            content.Name = contentEditingModelBase.Variants.FirstOrDefault(v => v.Culture is null && v.Segment is null)?.Name
                           ?? throw new ArgumentException("Could not find a culture invariant variant", nameof(contentEditingModelBase));
        }
    }

    private async Task UpdateExistingProperties(ContentEditingModelBase contentEditingModelBase, TContent content, TContentType contentType)
    {
        // create a mapping dictionary for all content type property types by their property aliases
        Dictionary<string, IPropertyType> propertyTypesByAlias = GetPropertyTypesByAlias(contentType);

        // remove any properties that do not exist on the content type
        PropertyValueModel[] propertyValues = contentEditingModelBase
            .Properties
            .Where(propertyValue => propertyTypesByAlias.ContainsKey(propertyValue.Alias))
            .ToArray();

        // update all properties on the content item
        foreach (PropertyValueModel propertyValue in propertyValues)
        {
            // the following checks should already have been validated by now, so it's OK to throw exceptions here
            if(propertyTypesByAlias.TryGetValue(propertyValue.Alias, out IPropertyType? propertyType) == false
               || (propertyType.VariesByCulture() && propertyValue.Culture.IsNullOrWhiteSpace())
               || (propertyType.VariesBySegment() is false && propertyValue.Segment.IsNullOrWhiteSpace() is false))
            {
                throw new ArgumentException($"Culture or segment variance mismatch for property: {propertyValue.Alias}", nameof(contentEditingModelBase));
            }

            // pass the value through the data editor to construct the value to store in the content
            var dataEditorValue = await GetDataEditorValue(propertyValue.Value, propertyValue.Culture, propertyValue.Segment, propertyType, content);
            content.SetValue(propertyValue.Alias, dataEditorValue, propertyValue.Culture, propertyValue.Segment);
        }
    }

    private void RemoveMissingProperties(ContentEditingModelBase contentEditingModelBase, TContent content, TContentType contentType)
    {
        // create a mapping dictionary for all content type property types by their property aliases
        Dictionary<string, IPropertyType> propertyTypesByAlias = GetPropertyTypesByAlias(contentType);
        var knownPropertyAliases = contentEditingModelBase
            .Properties
            .Select(pv => pv.Alias)
            .Distinct()
            .ToArray();

        var missingPropertyAliases = propertyTypesByAlias.Keys.Except(knownPropertyAliases).ToArray();
        foreach (var propertyAlias in missingPropertyAliases)
        {
            content.RemoveValue(propertyAlias);
        }
    }

    private async Task<object?> GetDataEditorValue(object? value, string? culture, string? segment, IPropertyType propertyType, TContent content)
    {
        // this should already have been validated by now, so it's OK to throw exceptions here
        if (_propertyEditorCollection.TryGet(propertyType.PropertyEditorAlias, out IDataEditor? dataEditor) == false)
        {
            _logger.LogWarning(
                "Unable to find property editor {PropertyEditorAlias}, for property {PropertyAlias}. Leaving property value unchanged.",
                propertyType.PropertyEditorAlias,
                propertyType.Alias);

            return content.GetValue(propertyType.Alias, culture, segment);
        }

        IDataValueEditor dataValueEditor = dataEditor.GetValueEditor();
        if (dataValueEditor.IsReadOnly)
        {
            // read-only property editor - get and return the current value
            return content.GetValue(propertyType.Alias, culture, segment);
        }

        IDataType? dataType = await _dataTypeService.GetAsync(propertyType.DataTypeKey);
        var data = new ContentPropertyData(value, dataType?.ConfigurationObject)
        {
            ContentKey = content.Key,
            PropertyTypeKey = propertyType.Key
        };

        var currentValue = content.GetValue(propertyType.Alias, culture, segment);
        return dataValueEditor.FromEditor(data, currentValue);
    }

    private static Dictionary<string, IPropertyType> GetPropertyTypesByAlias(TContentType contentType)
        => contentType.CompositionPropertyTypes.ToDictionary(pt => pt.Alias);

    protected async Task<HashSet<string>> GetAllowedCulturesForEditingUser(Guid userKey)
    {
        IUser user = await _userService.GetAsync(userKey)
                      ?? throw new InvalidOperationException($"Could not find user by key {userKey} when editing or validating content.");

        var allowedLanguageIds = user.CalculateAllowedLanguageIds(_localizationService)!;

        return (await _languageService.GetIsoCodesByIdsAsync(allowedLanguageIds)).ToHashSet();
    }

    /// <summary>
    /// Should never be made public, serves the purpose of a nullable bool but more readable.
    /// </summary>
    protected internal enum ContentTrashStatusRequirement
    {
        Irrelevant,
        MustBeTrashed,
        MustNotBeTrashed
    }
}
