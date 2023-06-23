using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

// FIXME: add granular permissions check (for inspiration, check how the old ContentController utilizes IAuthorizationService)
internal sealed class ContentEditingService
    : ContentEditingServiceBase<IContent, IContentType, IContentService, IContentTypeService>, IContentEditingService
{
    private readonly ITemplateService _templateService;
    private readonly ILogger<ContentEditingService> _logger;
    private readonly IUserIdKeyResolver _userIdKeyResolver;

    public ContentEditingService(
        IContentService contentService,
        IContentTypeService contentTypeService,
        PropertyEditorCollection propertyEditorCollection,
        IDataTypeService dataTypeService,
        ITemplateService templateService,
        ILogger<ContentEditingService> logger,
        ICoreScopeProvider scopeProvider,
        IUserIdKeyResolver userIdKeyResolver)
        : base(contentService, contentTypeService, propertyEditorCollection, dataTypeService, logger, scopeProvider)
    {
        _templateService = templateService;
        _logger = logger;
        _userIdKeyResolver = userIdKeyResolver;
    }

    public async Task<IContent?> GetAsync(Guid id)
    {
        IContent? content = ContentService.GetById(id);
        return await Task.FromResult(content);
    }

    public async Task<Attempt<IContent?, ContentEditingOperationStatus>> CreateAsync(ContentCreateModel createModel, Guid userKey)
    {
        Attempt<IContent?, ContentEditingOperationStatus> result = await MapCreate(createModel);
        if (result.Success == false)
        {
            return result;
        }

        IContent content = result.Result!;
        ContentEditingOperationStatus operationStatus = await UpdateTemplateAsync(content, createModel.TemplateKey);
        if (operationStatus != ContentEditingOperationStatus.Success)
        {
            return Attempt.FailWithStatus<IContent?, ContentEditingOperationStatus>(operationStatus, content);
        }

        operationStatus = await Save(content, userKey);
        return operationStatus == ContentEditingOperationStatus.Success
            ? Attempt.SucceedWithStatus<IContent?, ContentEditingOperationStatus>(ContentEditingOperationStatus.Success, content)
            : Attempt.FailWithStatus<IContent?, ContentEditingOperationStatus>(operationStatus, content);
    }

    public async Task<Attempt<IContent, ContentEditingOperationStatus>> UpdateAsync(IContent content, ContentUpdateModel updateModel, Guid userKey)
    {
        Attempt<ContentEditingOperationStatus> result = await MapUpdate(content, updateModel);
        if (result.Success == false)
        {
            return Attempt.FailWithStatus(result.Result, content);
        }

        ContentEditingOperationStatus operationStatus = await UpdateTemplateAsync(content, updateModel.TemplateKey);
        if (operationStatus != ContentEditingOperationStatus.Success)
        {
            return Attempt.FailWithStatus(operationStatus, content);
        }

        operationStatus = await Save(content, userKey);
        return operationStatus == ContentEditingOperationStatus.Success
            ? Attempt.SucceedWithStatus(ContentEditingOperationStatus.Success, content)
            : Attempt.FailWithStatus(operationStatus, content);
    }

    public async Task<Attempt<IContent?, ContentEditingOperationStatus>> MoveToRecycleBinAsync(Guid id, Guid userKey)
    {
        var currentUserId = await GetUserIdAsync(userKey);
        return await HandleDeletionAsync(id, content => ContentService.MoveToRecycleBin(content, currentUserId), false);
    }

    public async Task<Attempt<IContent?, ContentEditingOperationStatus>> DeleteAsync(Guid id, Guid userKey)
    {
        var currentUserId = await GetUserIdAsync(userKey);
        return await HandleDeletionAsync(id, content => ContentService.Delete(content, currentUserId), false);
    }

    public async Task<Attempt<IContent?, ContentEditingOperationStatus>> MoveAsync(Guid id, Guid? parentId, Guid userKey)
    {
        var currentUserId = await GetUserIdAsync(userKey);
        return await HandleMoveAsync(id, parentId, (content, newParentId) => ContentService.Move(content, newParentId, currentUserId));
    }

    public async Task<Attempt<IContent?, ContentEditingOperationStatus>> CopyAsync(Guid id, Guid? parentId, bool relateToOriginal, bool includeDescendants, Guid userKey)
    {
        var currentUserId = await GetUserIdAsync(userKey);
        return await HandleCopyAsync(id, parentId, (content, newParentId) => ContentService.Copy(content, newParentId, relateToOriginal, includeDescendants, currentUserId));
    }

    protected override IContent Create(string? name, int parentId, IContentType contentType) => new Content(name, parentId, contentType);

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

    private async Task<int> GetUserIdAsync(Guid userKey) => await _userIdKeyResolver.GetAsync(userKey);
}
