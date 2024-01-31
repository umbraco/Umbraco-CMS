using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

internal sealed class MemberContentEditingService
    : ContentEditingServiceBase<IMember, IMemberType, IMemberService, IMemberTypeService>, IMemberContentEditingService
{
    private readonly ILogger<ContentEditingServiceBase<IMember, IMemberType, IMemberService, IMemberTypeService>> _logger;

    public MemberContentEditingService(
        IMemberService contentService,
        IMemberTypeService contentTypeService,
        PropertyEditorCollection propertyEditorCollection,
        IDataTypeService dataTypeService,
        ILogger<ContentEditingServiceBase<IMember, IMemberType, IMemberService, IMemberTypeService>> logger,
        ICoreScopeProvider scopeProvider,
        IUserIdKeyResolver userIdKeyResolver,
        IMemberValidationService memberValidationService)
        : base(contentService, contentTypeService, propertyEditorCollection, dataTypeService, logger, scopeProvider, userIdKeyResolver, memberValidationService)
        => _logger = logger;

    public async Task<Attempt<MemberUpdateResult, ContentEditingOperationStatus>> UpdateAsync(IMember member, MemberEditingModelBase updateModel, Guid userKey)
    {
        // FIXME: handle sensitive property data
        Attempt<MemberUpdateResult, ContentEditingOperationStatus> result = await MapUpdate<MemberUpdateResult>(member, updateModel);
        if (result.Success == false)
        {
            return Attempt.FailWithStatus(result.Status, result.Result);
        }

        // the create mapping might succeed, but this doesn't mean the model is valid at property level.
        // we'll return the actual property validation status if the entire operation succeeds.
        ContentEditingOperationStatus validationStatus = result.Status;
        ContentValidationResult validationResult = result.Result.ValidationResult;

        var currentUserId = await GetUserIdAsync(userKey);
        ContentEditingOperationStatus operationStatus = Save(member, currentUserId);
        return operationStatus == ContentEditingOperationStatus.Success
            ? Attempt.SucceedWithStatus(validationStatus, new MemberUpdateResult { Content = member, ValidationResult = validationResult })
            : Attempt.FailWithStatus(operationStatus, new MemberUpdateResult { Content = member });
    }

    public async Task<Attempt<IMember?, ContentEditingOperationStatus>> DeleteAsync(Guid key, Guid userKey)
        => await HandleDeleteAsync(key, userKey, false);


    protected override IMember New(string? name, int parentId, IMemberType memberType)
        => throw new NotSupportedException("Member creation is not supported by this service. This should never be called.");

    protected override OperationResult? Move(IMember member, int newParentId, int userId)
        => throw new InvalidOperationException("Move is not supported for members");

    protected override IMember? Copy(IMember member, int newParentId, bool relateToOriginal, bool includeDescendants, int userId)
        => throw new NotSupportedException("Copy is not supported for Member");

    protected override OperationResult? MoveToRecycleBin(IMember member, int userId)
        => throw new InvalidOperationException("Recycle bin is not supported for members");

    protected override OperationResult? Delete(IMember member, int userId)
        => ContentService.Delete(member, userId).Result;

    private ContentEditingOperationStatus Save(IMember member, int userId)
    {
        try
        {
            Attempt<OperationResult?> saveResult = ContentService.Save(member, userId);
            return saveResult.Result?.Result switch
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
            _logger.LogError(ex, "Member save operation failed");
            return ContentEditingOperationStatus.Unknown;
        }
    }
}
