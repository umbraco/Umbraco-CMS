namespace Umbraco.Cms.Api.Management.ViewModels.UserGroup.Permissions;

/// <summary>
/// Provides a presentation model that describes the element container permissions assigned to a user group.
/// </summary>
public class ElementContainerPermissionPresentationModel : IPermissionPresentationModel
{
    /// <summary>
    /// Gets or sets a reference to the element container for which the permission applies.
    /// </summary>
    public required ReferenceByIdModel ElementContainer { get; set; }

    /// <summary>
    /// Gets or sets the set of verbs representing the actions permitted on the element container and its descendant elements for this user group.
    /// </summary>
    public required ISet<string> Verbs { get; set; }
}
