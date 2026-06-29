namespace Umbraco.Cms.Api.Management.ViewModels.MemberGroup;

/// <summary>
/// Represents a data transfer object used to return information about a member group in API responses.
/// </summary>
public class MemberGroupResponseModel : MemberGroupPresentationBase
{
    /// <summary>
    /// Gets or sets the unique identifier of the member group.
    /// </summary>
    public Guid Id { get; set; }
}
