namespace Umbraco.Cms.Api.Management.ViewModels.UserGroup.Permissions;

/// <summary>
/// Provides a presentation model that describes the element permissions assigned to a user group.
/// </summary>
public class ElementPermissionPresentationModel : IPermissionPresentationModel
{
    /// <summary>
    /// Gets or sets a reference to the element for which the permission applies.
    /// </summary>
    public required ReferenceByIdModel Element { get; set; }

    /// <summary>
    /// Gets or sets the set of verbs representing the actions permitted on elements for this user group.
    /// </summary>
    public required ISet<string> Verbs { get; set; }
}