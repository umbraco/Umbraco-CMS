namespace Umbraco.Cms.Api.Management.ViewModels.UserGroup;

/// <summary>
/// Request model for creating a user group.
/// </summary>
public class CreateUserGroupRequestModel : UserGroupBase
{
    /// <summary>
    /// Gets or sets the unique identifier of the user group.
    /// </summary>
    public Guid? Id { get; set; }
}
