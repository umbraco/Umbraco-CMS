namespace Umbraco.Cms.Api.Management.ViewModels.PublicAccess;

/// <summary>
/// Represents the data required to configure or update public access settings for a resource.
/// </summary>
public class PublicAccessRequestModel : PublicAccessBaseModel
{
    /// <summary>
    /// Gets or sets the member usernames associated with the public access request.
    /// </summary>
    public string[] MemberUserNames { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Gets or sets the member group names associated with the public access request.
    /// </summary>
    public string[] MemberGroupNames { get; set; } = Array.Empty<string>();
}
