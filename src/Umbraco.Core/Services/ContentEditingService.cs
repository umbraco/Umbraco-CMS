﻿using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

internal sealed class ContentEditingService
    : ContentEditingServiceBase<IContent, IContentType, IContentService, IContentTypeService>, IContentEditingService
{
    private readonly ITemplateService _templateService;
    private readonly ILogger<ContentEditingService> _logger;
    private readonly ICoreScopeProvider _scopeProvider;

    public ContentEditingService(
        IContentService contentService,
        IContentTypeService contentTypeService,
        PropertyEditorCollection propertyEditorCollection,
        IDataTypeService dataTypeService,
        ITemplateService templateService,
        ILogger<ContentEditingService> logger,
        ICoreScopeProvider scopeProvider)
        : base(contentService, contentTypeService, propertyEditorCollection, dataTypeService, logger)
    {
        _templateService = templateService;
        _logger = logger;
        _scopeProvider = scopeProvider;
    }

    public async Task<IContent?> GetAsync(Guid id)
    {
        IContent? content = ContentService.GetById(id);
        return await Task.FromResult(content);
    }

    public async Task<Attempt<IContent?, ContentEditingOperationStatus>> CreateAsync(ContentCreateModel createModel, int userId = Constants.Security.SuperUserId)
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

        operationStatus = Save(content, userId);
        return operationStatus == ContentEditingOperationStatus.Success
            ? Attempt.SucceedWithStatus<IContent?, ContentEditingOperationStatus>(ContentEditingOperationStatus.Success, content)
            : Attempt.FailWithStatus<IContent?, ContentEditingOperationStatus>(operationStatus, content);
    }

    public async Task<Attempt<IContent, ContentEditingOperationStatus>> UpdateAsync(IContent content, ContentUpdateModel updateModel, int userId = Constants.Security.SuperUserId)
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

        operationStatus = Save(content, userId);
        return operationStatus == ContentEditingOperationStatus.Success
            ? Attempt.SucceedWithStatus(ContentEditingOperationStatus.Success, content)
            : Attempt.FailWithStatus(operationStatus, content);
    }

    public async Task<Attempt<IContent?, ContentEditingOperationStatus>> MoveToRecycleBinAsync(Guid id, int userId = Constants.Security.SuperUserId)
        => await HandleDeletionAsync(id, content => ContentService.MoveToRecycleBin(content, userId));

    public async Task<Attempt<IContent?, ContentEditingOperationStatus>> DeleteAsync(Guid id, int userId = Constants.Security.SuperUserId)
        => await HandleDeletionAsync(id, content => ContentService.Delete(content, userId));

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

    private ContentEditingOperationStatus Save(IContent content, int userId)
    {
        try
        {
            OperationResult saveResult = ContentService.Save(content, userId);
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

    // helper method to perform move-to-recycle-bin and delete for content as they are very much handled in the same way
    private async Task<Attempt<IContent?, ContentEditingOperationStatus>> HandleDeletionAsync(Guid id, Func<IContent, OperationResult> performDelete)
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope(autoComplete:true);
        IContent? content = ContentService.GetById(id);
        if (content == null)
        {
            return await Task.FromResult(Attempt.FailWithStatus(ContentEditingOperationStatus.NotFound, content));
        }

        OperationResult deleteResult = performDelete(content);
        return deleteResult.Result switch
        {
            // these are the only result states currently expected from Delete
            OperationResultType.Success => Attempt.SucceedWithStatus<IContent?, ContentEditingOperationStatus>(ContentEditingOperationStatus.Success, content),
            OperationResultType.FailedCancelledByEvent => Attempt.FailWithStatus<IContent?, ContentEditingOperationStatus>(ContentEditingOperationStatus.CancelledByNotification, content),

            // for any other state we'll return "unknown" so we know that we need to amend this
            _ => Attempt.FailWithStatus<IContent?, ContentEditingOperationStatus>(ContentEditingOperationStatus.Unknown, content)
        };
    }
}
