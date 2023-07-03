namespace Umbraco.Cms.Api.Management.ViewModels.PublicAccess;

public class PublicAccessRequestModel : PublicAccessBaseModel
{
    public Guid ContentId { get; set; }

    public string[] MemberUserNames { get; set; } = Array.Empty<string>();

    public string[] MemberGroupNames { get; set; } = Array.Empty<string>();
}
