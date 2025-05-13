namespace Umbraco.Cms.Api.Management.ViewModels.TrackedReferences;

public class MemberTypePropertyTypeReferenceResponseModel : ContentTypePropertyTypeReferenceResponseModel
{
    public TrackedReferenceMemberType MemberType { get; set; } = new();
}
