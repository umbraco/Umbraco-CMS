using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services.Filters;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

/// <summary>
/// Provides services for creating, updating, and managing content blueprints (templates for content).
/// </summary>
internal sealed class ContentBlueprintEditingService
    : ContentEditingServiceBase<IContent, IContentType, IContentService, IContentTypeService>, IContentBlueprintEditingService
{
    private readonly IContentBlueprintContainerService _containerService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ContentBlueprintEditingService"/> class.
    /// </summary>
    /// <param name="contentService">The content service.</param>
    /// <param name="contentTypeService">The content type service.</param>
    /// <param name="propertyEditorCollection">The collection of property editors.</param>
    /// <param name="dataTypeService">The data type service.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="scopeProvider">The scope provider.</param>
    /// <param name="userIdKeyResolver">The user ID key resolver.</param>
    /// <param name="validationService">The content validation service.</param>
    /// <param name="containerService">The content blueprint container service.</param>
    /// <param name="optionsMonitor">The content settings options monitor.</param>
    /// <param name="relationService">The relation service.</param>
    /// <param name="contentTypeFilters">The content type filter collection.</param>
    public ContentBlueprintEditingService(
        IContentService contentService,
        IContentTypeService contentTypeService,
        PropertyEditorCollection propertyEditorCollection,
        IDataTypeService dataTypeService,
        ILogger<ContentEditingServiceBase<IContent, IContentType, IContentService, IContentTypeService>> logger,
        ICoreScopeProvider scopeProvider,
        IUserIdKeyResolver userIdKeyResolver,
        IContentValidationService validationService,
        IContentBlueprintContainerService containerService,
        IOptionsMonitor<ContentSettings> optionsMonitor,
        IRelationService relationService,
        ContentTypeFilterCollection contentTypeFilters,
        ILanguageService languageService,
        IUserService userService,
        ILocalizationService localizationService)
        : base(contentService, contentTypeService, propertyEditorCollection, dataTypeService, logger, scopeProvider, userIdKeyResolver, validationService, optionsMonitor, relationService, contentTypeFilters, languageService, userService, localizationService)
        => _containerService = containerService;

    /// <inheritdoc />
    public Task<IContent?> GetAsync(Guid key)
    {
        IContent? blueprint = ContentService.GetBlueprintById(key);
        return Task.FromResult(blueprint);
    }

    /// <inheritdoc />
    public Task<IContent?> GetScaffoldedAsync(Guid key)
    {
        IContent? blueprint = ContentService.GetBlueprintById(key);
        if (blueprint is null)
        {
            return Task.FromResult<IContent?>(null);
        }

        IContent scaffold = blueprint.DeepCloneWithResetIdentities();

        using ICoreScope scope = CoreScopeProvider.CreateCoreScope();
        scope.Notifications.Publish(new ContentScaffoldedNotification(blueprint, scaffold, Constants.System.Root, new EventMessages()));
        scope.Complete();

        return Task.FromResult<IContent?>(scaffold);
    }

    /// <inheritdoc />
    public async Task<Attempt<PagedModel<IContent>?, ContentEditingOperationStatus>> GetPagedByContentTypeAsync(Guid contentTypeKey, int skip, int take)
    {
        IContentType? contentType = await ContentTypeService.GetAsync(contentTypeKey);
        if (contentType is null)
        {
            return Attempt.FailWithStatus<PagedModel<IContent>?, ContentEditingOperationStatus>(ContentEditingOperationStatus.ContentTypeNotFound, null);
        }

        IContent[] blueprints = ContentService.GetBlueprintsForContentTypes([contentType.Id]).ToArray();

        var result = new PagedModel<IContent>
        {
            Items = blueprints.Skip(skip).Take(take),
            Total = blueprints.Length,
        };

        return Attempt.SucceedWithStatus<PagedModel<IContent>?, ContentEditingOperationStatus>(ContentEditingOperationStatus.Success, result);
    }

    /// <inheritdoc />
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

        if (ValidateUniqueNames(createModel.Variants, blueprint) is false)
        {
            return Attempt.FailWithStatus(ContentEditingOperationStatus.DuplicateName, new ContentCreateResult());
        }

        // Save blueprint
        await SaveAsync(blueprint, userKey);

        return Attempt.SucceedWithStatus(ContentEditingOperationStatus.Success, new ContentCreateResult { Content = blueprint, ValidationResult = result.Result.ValidationResult });
    }

    /// <inheritdoc />
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
        IContent blueprint = ContentService.CreateBlueprintFromContent(content, name, currentUserId);

        if (key.HasValue)
        {
            blueprint.Key = key.Value;
        }

        // Save blueprint
        await SaveAsync(blueprint, userKey, content);

        return Attempt.SucceedWithStatus(ContentEditingOperationStatus.Success, new ContentCreateResult { Content = blueprint });
    }

    /// <inheritdoc />
    public async Task<Attempt<ContentUpdateResult, ContentEditingOperationStatus>> UpdateAsync(Guid key, ContentBlueprintUpdateModel updateModel, Guid userKey)
    {
        IContent? blueprint = await GetAsync(key);
        if (blueprint is null)
        {
            return Attempt.FailWithStatus(ContentEditingOperationStatus.NotFound, new ContentUpdateResult());
        }

        if (ValidateUniqueNames(updateModel.Variants, blueprint) is false)
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

    /// <inheritdoc />
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

    /// <inheritdoc />
    public async Task<Attempt<ContentEditingOperationStatus>> MoveAsync(Guid key, Guid? containerKey, Guid userKey)
    {
        using ICoreScope scope = CoreScopeProvider.CreateCoreScope();
        IContent? toMove = await GetAsync(key);
        if (toMove is null)
        {
            return Attempt.Fail(ContentEditingOperationStatus.NotFound);
        }

        var parentId = Constants.System.Root;
        if (containerKey.HasValue && containerKey.Value != Guid.Empty)
        {
            EntityContainer? container = await _containerService.GetAsync(containerKey.Value);
            if (container is null)
            {
                return Attempt.Fail(ContentEditingOperationStatus.ParentNotFound);
            }

            parentId = container.Id;
        }

        if (toMove.ParentId == parentId)
        {
            return Attempt.Succeed(ContentEditingOperationStatus.Success);
        }

        // NOTE: as long as the parent ID is correct the document repo takes care of updating the rest of the
        //       structural node data like path, level, sort orders etc.
        toMove.ParentId = parentId;

        // Save blueprint
        await SaveAsync(toMove, userKey);

        scope.Complete();

        return Attempt.Succeed(ContentEditingOperationStatus.Success);
    }

    /// <inheritdoc />
    protected override IContent New(string? name, int parentId, IContentType contentType)
        => new Content(name, parentId, contentType);

    /// <inheritdoc />
    protected override async Task<(int? ParentId, ContentEditingOperationStatus OperationStatus)> TryGetAndValidateParentIdAsync(Guid? parentKey, IContentType contentType)
    {
        if (parentKey.HasValue is false)
        {
            return (Constants.System.Root, ContentEditingOperationStatus.Success);
        }

        EntityContainer? container = await _containerService.GetAsync(parentKey.Value);
        return container is not null
            ? (container.Id, ContentEditingOperationStatus.Success)
            : (null, ContentEditingOperationStatus.ParentNotFound);
    }

    /// <summary>
    /// Moves the specified content to a new parent. Not supported for blueprints.
    /// </summary>
    /// <param name="content">The content to move.</param>
    /// <param name="newParentId">The ID of the new parent.</param>
    /// <param name="userId">The ID of the user performing the operation.</param>
    /// <returns>Not supported for blueprints.</returns>
    /// <exception cref="NotImplementedException">Always thrown as this operation is not supported for blueprints.</exception>
    /// <remarks>
    /// Some methods from ContentEditingServiceBase are needed, so we need to inherit from it
    /// but there are others that are not required to be implemented in the case of blueprints.
    /// </remarks>
    protected override OperationResult? Move(IContent content, int newParentId, int userId) => throw new NotImplementedException();

    /// <summary>
    /// Copies the specified content to a new parent. Not supported for blueprints.
    /// </summary>
    /// <param name="content">The content to copy.</param>
    /// <param name="newParentId">The ID of the new parent.</param>
    /// <param name="relateToOriginal">Whether to relate the copy to the original.</param>
    /// <param name="includeDescendants">Whether to include descendants in the copy.</param>
    /// <param name="userId">The ID of the user performing the operation.</param>
    /// <returns>Not supported for blueprints.</returns>
    /// <exception cref="NotImplementedException">Always thrown as this operation is not supported for blueprints.</exception>
    protected override IContent? Copy(IContent content, int newParentId, bool relateToOriginal, bool includeDescendants, int userId) => throw new NotImplementedException();

    /// <summary>
    /// Moves the specified content to the recycle bin. Not supported for blueprints.
    /// </summary>
    /// <param name="content">The content to move to recycle bin.</param>
    /// <param name="userId">The ID of the user performing the operation.</param>
    /// <returns>Not supported for blueprints.</returns>
    /// <exception cref="NotImplementedException">Always thrown as this operation is not supported for blueprints.</exception>
    protected override OperationResult? MoveToRecycleBin(IContent content, int userId) => throw new NotImplementedException();

    /// <summary>
    /// Deletes the specified content. Not supported for blueprints.
    /// </summary>
    /// <param name="content">The content to delete.</param>
    /// <param name="userId">The ID of the user performing the operation.</param>
    /// <returns>Not supported for blueprints.</returns>
    /// <exception cref="NotImplementedException">Always thrown as this operation is not supported for blueprints.</exception>
    protected override OperationResult? Delete(IContent content, int userId) => throw new NotImplementedException();

    /// <summary>
    /// Saves a blueprint with the specified user key.
    /// </summary>
    /// <param name="blueprint">The blueprint to save.</param>
    /// <param name="userKey">The unique identifier of the user performing the save.</param>
    /// <param name="createdFromContent">The optional content item the blueprint was created from.</param>
    private async Task SaveAsync(IContent blueprint, Guid userKey, IContent? createdFromContent = null)
    {
        var currentUserId = await GetUserIdAsync(userKey);
        ContentService.SaveBlueprint(blueprint, createdFromContent, currentUserId);
    }

    /// <summary>
    /// Validates that the specified name is unique among blueprints of the same content type.
    /// </summary>
    /// <param name="name">The name to validate.</param>
    /// <param name="content">The content item to check against.</param>
    /// <returns><c>true</c> if the name is unique; otherwise, <c>false</c>.</returns>
    private bool ValidateUniqueName(string name, IContent content)
    {
        IEnumerable<IContent> existing = ContentService.GetBlueprintsForContentTypes(content.ContentTypeId);
        return existing.Any(c => c.Name == name && c.Id != content.Id) is false;
    }

    /// <summary>
    /// Validates that all variant names are unique among blueprints of the same content type.
    /// </summary>
    /// <param name="variants">The variants containing names to validate.</param>
    /// <param name="content">The content item to check against.</param>
    /// <returns><c>true</c> if all names are unique; otherwise, <c>false</c>.</returns>
    private bool ValidateUniqueNames(IEnumerable<VariantModel> variants, IContent content)
    {
        IContent[] existing = ContentService.GetBlueprintsForContentTypes(content.ContentTypeId).ToArray();
        foreach (VariantModel variant in variants)
        {
            if (existing.Any(c => c.GetCultureName(variant.Culture) == variant.Name && c.Id != content.Id))
            {
                return false;
            }
        }

        return true;
    }
}
