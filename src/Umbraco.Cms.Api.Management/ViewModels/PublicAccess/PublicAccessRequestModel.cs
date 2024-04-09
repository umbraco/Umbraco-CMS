namespace Umbraco.Cms.Api.Management.ViewModels.PublicAccess;

public class PublicAccessRequestModel : PublicAccessBaseModel
{
    public string[] MemberUserNames { get; set; } = Array.Empty<string>();

    public string[] MemberGroupNames { get; set; } = Array.Empty<string>();
}
