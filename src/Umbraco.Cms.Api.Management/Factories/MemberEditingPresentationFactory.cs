using Umbraco.Cms.Api.Management.ViewModels.Member;
using Umbraco.Cms.Core.Models.ContentEditing;

namespace Umbraco.Cms.Api.Management.Factories;

internal sealed class MemberEditingPresentationFactory : ContentEditingPresentationFactory<MemberValueModel, MemberVariantRequestModel>, IMemberEditingPresentationFactory
{
    public MemberCreateModel MapCreateModel(CreateMemberRequestModel createRequestModel)
    {
        MemberCreateModel model = MapContentEditingModel<MemberCreateModel>(createRequestModel);

        model.Key = createRequestModel.Id;
        model.ContentTypeKey = createRequestModel.MemberType.Id;
        model.IsApproved = createRequestModel.IsApproved;
        model.Email = createRequestModel.Email;
        model.Username = createRequestModel.Username;
        model.Password = createRequestModel.Password;
        model.Roles = createRequestModel.Groups;

        return model;
    }

    public MemberUpdateModel MapUpdateModel(UpdateMemberRequestModel updateRequestModel)
    {
        MemberUpdateModel model = MapContentEditingModel<MemberUpdateModel>(updateRequestModel);

        model.IsApproved = updateRequestModel.IsApproved;
        model.IsLockedOut = updateRequestModel.IsLockedOut;
        model.IsTwoFactorEnabled = updateRequestModel.IsTwoFactorEnabled;
        model.OldPassword = updateRequestModel.OldPassword;
        model.NewPassword = updateRequestModel.NewPassword;
        model.Email = updateRequestModel.Email;
        model.Username = updateRequestModel.Username;
        model.Roles = updateRequestModel.Groups;

        return model;
    }
}
