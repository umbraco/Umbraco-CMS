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

    // NB: Some of the implementation is copied from <see cref="IContentEditingService.CreateAsync()" />
    public async Task<Attempt<ContentCreateResult, ContentEditingOperationStatus>> CreateAsync(ContentBlueprintCreateModel createModel, Guid userKey)
    {
        if (await ValidateCulturesAsync(createModel) is false)
        {
            return Attempt.FailWithStatus(ContentEditingOperationStatus.InvalidCulture, new ContentCreateResult());
        }

        Attempt<ContentCreateResult, ContentEditingOperationStatus> result = await MapCreate<ContentCreateResult>(createModel);
        if (result.Success is false)
        {
            return result;
        }

        IContent blueprint = result.Result.Content!;

        if (ValidateUniqueName(createModel.InvariantName ?? string.Empty, blueprint) is false)
        {
            return Attempt.FailWithStatus(ContentEditingOperationStatus.DuplicateName, new ContentCreateResult());
        }

        // Save blueprint
        await SaveAsync(blueprint, userKey);

        return Attempt.SucceedWithStatus(ContentEditingOperationStatus.Success, new ContentCreateResult { Content = blueprint, ValidationResult = result.Result.ValidationResult });
    }

    // NB: Some of the implementation is copied from <see cref="IContentEditingService.CreateAsync()" />
    public async Task<Attempt<ContentCreateResult, ContentEditingOperationStatus>> CreateFromContentAsync(Guid contentKey, string name, Guid? key, Guid userKey)
    {
        IContent? content = ContentService.GetById(contentKey);
        if (content is null)
        {
            return Attempt.FailWithStatus(ContentEditingOperationStatus.NotFound, new ContentCreateResult());
        }

        if (ValidateUniqueName(name, content) is false)
        {
            return Attempt.FailWithStatus(ContentEditingOperationStatus.DuplicateName, new ContentCreateResult());
        }

        // Create Blueprint
        var currentUserId = await GetUserIdAsync(userKey);
        IContent blueprint = ContentService.CreateContentFromBlueprint(content, name, currentUserId);

        if (key.HasValue)
        {
            blueprint.Key = key.Value;
        }

        // Save blueprint
        await SaveAsync(blueprint, userKey);

        return Attempt.SucceedWithStatus(ContentEditingOperationStatus.Success, new ContentCreateResult { Content = blueprint });
    }

    // NB: Some of the implementation is copied from <see cref="IContentEditingService.UpdateAsync()" />
    public async Task<Attempt<ContentUpdateResult, ContentEditingOperationStatus>> UpdateAsync(Guid key, ContentBlueprintUpdateModel updateModel, Guid userKey)
    {
        IContent? blueprint = await GetAsync(key);
        if (blueprint is null)
        {
            return Attempt.FailWithStatus(ContentEditingOperationStatus.NotFound, new ContentUpdateResult());
        }

        if (ValidateUniqueName(updateModel.InvariantName ?? string.Empty, blueprint) is false)
        {
            return Attempt.FailWithStatus(ContentEditingOperationStatus.DuplicateName, new ContentUpdateResult());
        }

        if (await ValidateCulturesAsync(updateModel) is false)
        {
            return Attempt.FailWithStatus(ContentEditingOperationStatus.InvalidCulture, new ContentUpdateResult { Content = blueprint });
        }

        Attempt<ContentUpdateResult, ContentEditingOperationStatus> result = await MapUpdate<ContentUpdateResult>(blueprint, updateModel);
        if (result.Success is false)
        {
            return Attempt.FailWithStatus(result.Status, result.Result);
        }

        // Save blueprint
        await SaveAsync(blueprint, userKey);

        return Attempt.SucceedWithStatus(ContentEditingOperationStatus.Success, new ContentUpdateResult { Content = blueprint, ValidationResult = result.Result.ValidationResult });
    }

    public async Task<Attempt<IContent?, ContentEditingOperationStatus>> DeleteAsync(Guid key, Guid userKey)
    {
        using ICoreScope scope = CoreScopeProvider.CreateCoreScope();
        IContent? blueprint = await GetAsync(key);
        if (blueprint is null)
        {
            return Attempt.FailWithStatus(ContentEditingOperationStatus.NotFound, blueprint);
        }

        // Delete blueprint
        var performingUserId = await GetUserIdAsync(userKey);
        ContentService.DeleteBlueprint(blueprint, performingUserId);

        scope.Complete();
        return Attempt.SucceedWithStatus<IContent?, ContentEditingOperationStatus>(ContentEditingOperationStatus.Success, blueprint);
    }

    protected override IContent New(string? name, int parentId, IContentType contentType)
        => new Content(name, parentId, contentType);

    /// <summary>
    ///     NB: Some methods from ContentEditingServiceBase are needed, so we need to inherit from it
    ///     but there are others that are not required to be implemented in the case of blueprints, therefore they throw NotImplementedException as default.
    /// </summary>
    protected override OperationResult? Move(IContent content, int newParentId, int userId) => throw new NotImplementedException();

    protected override IContent? Copy(IContent content, int newParentId, bool relateToOriginal, bool includeDescendants, int userId) => throw new NotImplementedException();

    protected override OperationResult? MoveToRecycleBin(IContent content, int userId) => throw new NotImplementedException();

    protected override OperationResult? Delete(IContent content, int userId) => throw new NotImplementedException();

    private async Task SaveAsync(IContent blueprint, Guid userKey)
    {
        var currentUserId = await GetUserIdAsync(userKey);
        ContentService.SaveBlueprint(blueprint, currentUserId);
    }

    private bool ValidateUniqueName(string name, IContent content)
    {
        IEnumerable<IContent> existing = ContentService.GetBlueprintsForContentTypes(content.ContentTypeId);
        return existing.Any(c => c.Name == name && c.Id != content.Id) is false;
    }
}
