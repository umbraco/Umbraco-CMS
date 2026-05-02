using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services.Filters;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

/// <summary>
/// Provides services for creating, updating, and managing content (documents).
/// </summary>
internal sealed class ContentEditingService
    : ContentEditingServiceWithSortingBase<IContent, IContentType, IContentService, IContentTypeService>, IContentEditingService
{
    private readonly ITemplateService _templateService;
    private readonly ILogger<ContentEditingService> _logger;

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
            contentTypeFilters,
            languageService,
            userService)
    {
        _templateService = templateService;
        _logger = logger;
    }

    /// <inheritdoc/>
    protected override string? RelateParentOnDeleteAlias => Constants.Conventions.RelationTypes.RelateParentDocumentOnDeleteAlias;

    /// <inheritdoc />
    public override Task<IContent?> GetAsync(Guid key)
    {
        IContent? content = ContentService.GetById(key);
        return Task.FromResult(content);
    }

    /// <inheritdoc />
    public async Task<Attempt<ContentValidationResult, ContentEditingOperationStatus>> ValidateUpdateAsync(
        Guid key,
        ValidateContentUpdateModel updateModel,
        Guid userKey)
    {
        IContent? content = ContentService.GetById(key);
        return content is not null
            ? await ValidateCulturesAndPropertiesAsync(
                updateModel,
                content.ContentType.Key,
                updateModel.Cultures,
                userKey)
            : Attempt.FailWithStatus(ContentEditingOperationStatus.NotFound, new ContentValidationResult());
    }

    /// <inheritdoc />
    public async Task<Attempt<ContentValidationResult, ContentEditingOperationStatus>> ValidateCreateAsync(
        ContentCreateModel createModel,
        Guid userKey)
        => await ValidateCulturesAndPropertiesAsync(
            createModel,
            createModel.ContentTypeKey,
            createModel.Variants.Select(variant => variant.Culture),
            userKey);

    /// <inheritdoc />
    public async Task<Attempt<ContentCreateResult, ContentEditingOperationStatus>> CreateAsync(ContentCreateModel createModel, Guid userKey)
    {
        if (await ValidateCulturesAsync(createModel) is false)
        {
            return Attempt.FailWithStatus(ContentEditingOperationStatus.InvalidCulture, new ContentCreateResult());
        }

        Attempt<ContentCreateResult, ContentEditingOperationStatus> result = await MapCreate<ContentCreateResult>(createModel);
        if (result.Success == false)
        {
            return result;
        }

        // the create mapping might succeed, but this doesn't mean the model is valid at property level.
        // we'll return the actual property validation status if the entire operation succeeds.
        ContentEditingOperationStatus validationStatus = result.Status;
        ContentValidationResult validationResult = result.Result.ValidationResult;

        IContent content = await EnsureOnlyAllowedFieldsAreUpdated(result.Result.Content!, userKey);
        ContentEditingOperationStatus updateTemplateStatus = await UpdateTemplateAsync(content, createModel.TemplateKey);
        if (updateTemplateStatus != ContentEditingOperationStatus.Success)
        {
            return Attempt.FailWithStatus(updateTemplateStatus, new ContentCreateResult { Content = content });
        }

        ContentEditingOperationStatus saveStatus = await Save(content, userKey);
        return saveStatus == ContentEditingOperationStatus.Success
            ? Attempt.SucceedWithStatus(validationStatus, new ContentCreateResult { Content = content, ValidationResult = validationResult })
            : Attempt.FailWithStatus(saveStatus, new ContentCreateResult { Content = content });
    }

    /// <inheritdoc />
    public async Task<Attempt<ContentUpdateResult, ContentEditingOperationStatus>> UpdateAsync(Guid key, ContentUpdateModel updateModel, Guid userKey)
    {
        IContent? content = ContentService.GetById(key);
        if (content is null)
        {
            return Attempt.FailWithStatus(ContentEditingOperationStatus.NotFound, new ContentUpdateResult());
        }

        if (await ValidateCulturesAsync(updateModel) is false)
        {
            return Attempt.FailWithStatus(ContentEditingOperationStatus.InvalidCulture, new ContentUpdateResult { Content = content });
        }

        Attempt<ContentUpdateResult, ContentEditingOperationStatus> result = await MapUpdate<ContentUpdateResult>(content, updateModel);
        if (result.Success == false)
        {
            return Attempt.FailWithStatus(result.Status, result.Result);
        }

        // the update mapping might succeed, but this doesn't mean the model is valid at property level.
        // we'll return the actual property validation status if the entire operation succeeds.
        ContentEditingOperationStatus validationStatus = result.Status;
        ContentValidationResult validationResult = result.Result.ValidationResult;

        content = await EnsureOnlyAllowedFieldsAreUpdated(content, userKey);

        ContentEditingOperationStatus updateTemplateStatus = await UpdateTemplateAsync(content, updateModel.TemplateKey);
        if (updateTemplateStatus != ContentEditingOperationStatus.Success)
        {
            return Attempt.FailWithStatus(updateTemplateStatus, new ContentUpdateResult { Content = content });
        }

        ContentEditingOperationStatus saveStatus = await Save(content, userKey);
        return saveStatus == ContentEditingOperationStatus.Success
            ? Attempt.SucceedWithStatus(validationStatus, new ContentUpdateResult { Content = content, ValidationResult = validationResult })
            : Attempt.FailWithStatus(saveStatus, new ContentUpdateResult { Content = content });
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
    public async Task<Attempt<IContent?, ContentEditingOperationStatus>> RestoreAsync(Guid key, Guid? parentKey, Guid userKey)
        => await HandleMoveAsync(key, parentKey, userKey, true);

    /// <inheritdoc />
    public async Task<Attempt<IContent?, ContentEditingOperationStatus>> CopyAsync(Guid key, Guid? parentKey, bool relateToOriginal, bool includeDescendants, Guid userKey)
        => await HandleCopyAsync(key, parentKey, relateToOriginal, includeDescendants, userKey);

    /// <inheritdoc />
    public async Task<ContentEditingOperationStatus> SortAsync(
        Guid? parentKey,
        IEnumerable<SortingModel> sortingModels,
        Guid userKey)
        => await HandleSortAsync(parentKey, sortingModels, userKey);

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
    protected override OperationResult? Move(IContent content, int newParentId, int userId)
        => ContentService.Move(content, newParentId, userId);

    /// <inheritdoc />
    protected override async Task<IContent?> CopyAsync(IContent content, int newParentId, bool relateToOriginal, bool includeDescendants, Guid userKey)
    {
        var userId = await GetUserIdAsync(userKey);
        return ContentService.Copy(content, newParentId, relateToOriginal, includeDescendants, userId);
    }

    /// <inheritdoc />
    protected override OperationResult? MoveToRecycleBin(IContent content, int userId) => ContentService.MoveToRecycleBin(content, userId);

    /// <inheritdoc />
    protected override OperationResult? Delete(IContent content, int userId) => ContentService.Delete(content, userId);

    /// <inheritdoc />
    protected override IEnumerable<IContent> GetPagedChildren(int parentId, int pageIndex, int pageSize, out long total)
        => ContentService.GetPagedChildren(parentId, pageIndex, pageSize, out total, propertyAliases: null, filter: null, ordering: null);

    /// <inheritdoc />
    protected override ContentEditingOperationStatus Sort(IEnumerable<IContent> items, int userId)
    {
        OperationResult result = ContentService.Sort(items, userId);
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
                _ => ContentEditingOperationStatus.Unknown
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Content save operation failed");
            return ContentEditingOperationStatus.Unknown;
        }
    }
}
