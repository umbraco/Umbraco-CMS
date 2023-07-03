namespace Umbraco.Cms.Api.Management.ViewModels.PublicAccess;

public class PublicAccessRequestModel : PublicAccessBaseModel
{
    public Guid ContentId { get; set; }

    public Guid[] MemberIds { get; set; } = Array.Empty<Guid>();

    public Guid[] MemberGroupIds { get; set; } = Array.Empty<Guid>();
}
