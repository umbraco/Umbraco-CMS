namespace Umbraco.Cms.Api.Management.ViewModels.UserGroups;

public class UserGroupPresentationModel : UserGroupBase, INamedEntityPresentationModel
{
    /// <summary>
    /// The key identifier for the user group.
    /// </summary>
    public required Guid Id { get; init; }

}
