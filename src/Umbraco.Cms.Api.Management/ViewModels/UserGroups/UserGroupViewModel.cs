namespace Umbraco.Cms.Api.Management.ViewModels.UserGroups;

public class UserGroupViewModel : UserGroupBase, INamedEntityViewModel
{
    /// <summary>
    /// The key identifier for the user group.
    /// </summary>
    public required Guid Key { get; init; }

}
