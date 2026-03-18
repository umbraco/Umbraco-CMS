namespace Umbraco.Cms.Api.Management.ViewModels.MemberGroup;

/// <summary>
/// Represents the request model for creating a member group.
/// </summary>
public class CreateMemberGroupRequestModel : MemberGroupPresentationBase
{
    /// <summary>
    /// Gets or sets the unique identifier of the member group.
    /// </summary>
    public Guid? Id { get; set; }
}
