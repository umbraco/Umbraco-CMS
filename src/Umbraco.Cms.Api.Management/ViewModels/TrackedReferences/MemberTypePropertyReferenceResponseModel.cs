namespace Umbraco.Cms.Api.Management.ViewModels.TrackedReferences;

public class MemberTypePropertyReferenceResponseModel : ContentTypePropertyReferenceResponseModel
{
    public TrackedReferenceMemberType MemberType { get; set; } = new();
}
