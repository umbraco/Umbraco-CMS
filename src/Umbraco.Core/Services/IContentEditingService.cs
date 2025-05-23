using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

public interface IContentEditingService
{
    Task<IContent?> GetAsync(Guid key);

    [Obsolete("Please use the validate create method that is not obsoleted. Scheduled for removal in Umbraco 17.")]
    Task<Attempt<ContentValidationResult, ContentEditingOperationStatus>> ValidateCreateAsync(ContentCreateModel createModel);

    Task<Attempt<ContentValidationResult, ContentEditingOperationStatus>> ValidateCreateAsync(ContentCreateModel createModel, Guid userKey)
#pragma warning disable CS0618 // Type or member is obsolete
        => ValidateCreateAsync(createModel);
#pragma warning restore CS0618 // Type or member is obsolete

    [Obsolete("Please use the validate update method that is not obsoleted. Scheduled for removal in Umbraco 17.")]
    Task<Attempt<ContentValidationResult, ContentEditingOperationStatus>> ValidateUpdateAsync(Guid key, ValidateContentUpdateModel updateModel);

    Task<Attempt<ContentValidationResult, ContentEditingOperationStatus>> ValidateUpdateAsync(Guid key, ValidateContentUpdateModel updateModel, Guid userKey)
#pragma warning disable CS0618 // Type or member is obsolete
        => ValidateUpdateAsync(key, updateModel);
#pragma warning restore CS0618 // Type or member is obsolete

    Task<Attempt<ContentCreateResult, ContentEditingOperationStatus>> CreateAsync(ContentCreateModel createModel, Guid userKey);

    Task<Attempt<ContentUpdateResult, ContentEditingOperationStatus>> UpdateAsync(Guid key, ContentUpdateModel updateModel, Guid userKey);

    Task<Attempt<IContent?, ContentEditingOperationStatus>> MoveToRecycleBinAsync(Guid key, Guid userKey);

    /// <summary>
    /// Deletes a Content Item if it is in the recycle bin.
    /// </summary>
    Task<Attempt<IContent?, ContentEditingOperationStatus>> DeleteFromRecycleBinAsync(Guid key, Guid userKey);

    Task<Attempt<IContent?, ContentEditingOperationStatus>> MoveAsync(Guid key, Guid? parentKey, Guid userKey);

    Task<Attempt<IContent?, ContentEditingOperationStatus>> CopyAsync(Guid key, Guid? parentKey, bool relateToOriginal, bool includeDescendants, Guid userKey);

    Task<ContentEditingOperationStatus> SortAsync(Guid? parentKey, IEnumerable<SortingModel> sortingModels, Guid userKey);

    /// <summary>
    /// Deletes a Content Item whether it is in the recycle bin or not.
    /// </summary>
    Task<Attempt<IContent?, ContentEditingOperationStatus>> DeleteAsync(Guid key, Guid userKey);

    Task<Attempt<IContent?, ContentEditingOperationStatus>> RestoreAsync(Guid key, Guid? parentKey, Guid userKey);
}
