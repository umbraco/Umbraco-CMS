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

    public ContentEditingService(
        IContentService contentService,
        IContentTypeService contentTypeService,
        PropertyEditorCollection propertyEditorCollection,
        IDataTypeService dataTypeService,
        ITemplateService templateService,
        ILogger<ContentEditingService> logger,
        ICoreScopeProvider scopeProvider,
        IUserIdKeyResolver userIdKeyResolver,
        ITreeEntitySortingService treeEntitySortingService)
        : base(contentService, contentTypeService, propertyEditorCollection, dataTypeService, logger, scopeProvider, userIdKeyResolver, treeEntitySortingService)
    {
        _templateService = templateService;
        _logger = logger;
    }

    public async Task<IContent?> GetAsync(Guid key)
    {
        IContent? content = ContentService.GetById(key);
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

    public async Task<Attempt<IContent?, ContentEditingOperationStatus>> MoveToRecycleBinAsync(Guid key, Guid userKey)
        => await HandleMoveToRecycleBinAsync(key, userKey);

    public async Task<Attempt<IContent?, ContentEditingOperationStatus>> DeleteAsync(Guid key, Guid userKey)
        => await HandleDeleteAsync(key, userKey);

    public async Task<Attempt<IContent?, ContentEditingOperationStatus>> MoveAsync(Guid key, Guid? parentKey, Guid userKey)
        => await HandleMoveAsync(key, parentKey, userKey);

    public async Task<Attempt<IContent?, ContentEditingOperationStatus>> CopyAsync(Guid key, Guid? parentKey, bool relateToOriginal, bool includeDescendants, Guid userKey)
        => await HandleCopyAsync(key, parentKey, relateToOriginal, includeDescendants, userKey);

    public async Task<ContentEditingOperationStatus> SortAsync(Guid? parentKey, IEnumerable<SortingModel> sortingModels, Guid userKey)
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

    protected override IContent New(string? name, int parentId, IContentType contentType)
        => new Content(name, parentId, contentType);

    protected override OperationResult? Move(IContent content, int newParentId, int userId)
        => ContentService.Move(content, newParentId, userId);

    protected override IContent? Copy(IContent content, int newParentId, bool relateToOriginal, bool includeDescendants, int userId)
        => ContentService.Copy(content, newParentId, relateToOriginal, includeDescendants, userId);

    protected override OperationResult? MoveToRecycleBin(IContent content, int userId)
        => ContentService.MoveToRecycleBin(content, userId);

    protected override OperationResult? Delete(IContent content, int userId)
        => ContentService.Delete(content, userId);

    protected override IEnumerable<IContent> GetPagedChildren(int parentId, int pageIndex, int pageSize, out long total)
        => ContentService.GetPagedChildren(parentId, pageIndex, pageSize, out total);

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
