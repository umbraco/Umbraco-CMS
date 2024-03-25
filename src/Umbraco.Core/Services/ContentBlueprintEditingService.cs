using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

// not implementing ContentEditingServiceWithSortingBase - it might be for later as it has Move, Copy, etc.
// FIXME: Refactor IContentEditingService and IContentBlueprintEditingService - they share logic
internal sealed class ContentBlueprintEditingService
    : ContentEditingServiceBase<IContent, IContentType, IContentService, IContentTypeService>, IContentBlueprintEditingService
{
    public ContentBlueprintEditingService(
        IContentService contentService,
        IContentTypeService contentTypeService,
        PropertyEditorCollection propertyEditorCollection,
        IDataTypeService dataTypeService,
        ILogger<ContentEditingServiceBase<IContent, IContentType, IContentService, IContentTypeService>> logger,
        ICoreScopeProvider scopeProvider,
        IUserIdKeyResolver userIdKeyResolver,
        IContentValidationService validationService)
        : base(contentService, contentTypeService, propertyEditorCollection, dataTypeService, logger, scopeProvider, userIdKeyResolver, validationService)
    {
    }

    public async Task<IContent?> GetAsync(Guid key)
    {
        IContent? blueprint = ContentService.GetBlueprintById(key);
        return await Task.FromResult(blueprint);
    }

    // NB: Some of the implementation is copied from <see cref="IContentEditingService.UpdateAsync()" />
    public async Task<Attempt<ContentUpdateResult, ContentEditingOperationStatus>> UpdateAsync(Guid key, ContentBlueprintUpdateModel updateModel, Guid userKey)
    {
        IContent? blueprint = await GetAsync(key);
        if (blueprint == null)
        {
            return Attempt.FailWithStatus(ContentEditingOperationStatus.NotFound, new ContentUpdateResult());
        }

        if (await ValidateCulturesAsync(updateModel) is false)
        {
            return Attempt.FailWithStatus(ContentEditingOperationStatus.InvalidCulture, new ContentUpdateResult { Content = blueprint });
        }

        Attempt<ContentUpdateResult, ContentEditingOperationStatus> result = await MapUpdate<ContentUpdateResult>(blueprint, updateModel);
        if (result.Success == false)
        {
            return Attempt.FailWithStatus(result.Status, result.Result);
        }

        var currentUserId = await GetUserIdAsync(userKey);
        ContentService.SaveBlueprint(blueprint, currentUserId);

        return Attempt.SucceedWithStatus(ContentEditingOperationStatus.Success, new ContentUpdateResult { Content = blueprint, ValidationResult = result.Result.ValidationResult });
    }

    public async Task<Attempt<IContent?, ContentEditingOperationStatus>> DeleteAsync(Guid key, Guid userKey)
    {
        using ICoreScope scope = CoreScopeProvider.CreateCoreScope();
        IContent? blueprint = await GetAsync(key);
        if (blueprint == null)
        {
            return Attempt.FailWithStatus(ContentEditingOperationStatus.NotFound, blueprint);
        }

        var performingUserId = await GetUserIdAsync(userKey);

        ContentService.DeleteBlueprint(blueprint, performingUserId);

        scope.Complete();
        return Attempt.SucceedWithStatus<IContent?, ContentEditingOperationStatus>(ContentEditingOperationStatus.Success, blueprint);
    }

    // NB: Some methods from ContentEditingServiceBase are needed so we need to inherit from it and have some of these as unimplemented as they don't apply in the case of blueprints
    protected override IContent New(string? name, int parentId, IContentType contentType)
        => new Content(name, parentId, contentType);

    protected override OperationResult? Move(IContent content, int newParentId, int userId) => throw new NotImplementedException();

    protected override IContent? Copy(IContent content, int newParentId, bool relateToOriginal, bool includeDescendants, int userId) => throw new NotImplementedException();

    protected override OperationResult? MoveToRecycleBin(IContent content, int userId) => throw new NotImplementedException();

    protected override OperationResult? Delete(IContent content, int userId) => throw new NotImplementedException();
}
