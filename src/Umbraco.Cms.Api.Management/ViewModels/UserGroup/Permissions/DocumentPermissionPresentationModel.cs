namespace Umbraco.Cms.Api.Management.ViewModels.UserGroup.Permissions;

/// <summary>
/// Provides a presentation model that describes the document permissions assigned to a user group.
/// </summary>
public class DocumentPermissionPresentationModel : IPermissionPresentationModel
{
    /// <summary>
    /// Gets or sets a reference to the document for which the permission applies.
    /// </summary>
    public required ReferenceByIdModel Document { get; set; }

    /// <summary>
    /// Gets or sets the set of verbs representing the actions permitted on documents for this user group.
    /// </summary>
    public required ISet<string> Verbs { get; set; }
}
