namespace Umbraco.Cms.Api.Management.ViewModels.UserGroups;

public class UserGroupViewModel : UserGroupBase
{
    /// <summary>
    /// The key identifier for the user group.
    /// </summary>
    public required Guid Key { get; init; }

    /// <summary>
    /// The type of entity this model represents.
    /// </summary>
    public string Type => "user-group";
}
