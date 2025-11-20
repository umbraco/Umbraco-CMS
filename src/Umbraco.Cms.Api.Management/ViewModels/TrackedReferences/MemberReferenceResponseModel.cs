namespace Umbraco.Cms.Api.Management.ViewModels.TrackedReferences;

public class MemberReferenceResponseModel : ReferenceResponseModel
{
     public TrackedReferenceMemberType MemberType { get; set; } = new();
}
