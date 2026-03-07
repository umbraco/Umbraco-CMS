namespace Umbraco.Cms.Api.Management.ViewModels.UserGroup.Permissions;

    /// <summary>
    /// Represents the presentation model for a user group permission of an unknown or unsupported type.
    /// </summary>
public class UnknownTypePermissionPresentationModel : IPermissionPresentationModel
{
    /// <summary>
    /// Gets or sets the set of verbs associated with the unknown type permission.
    /// </summary>
    public required ISet<string> Verbs { get; set; }

    /// <summary>
    /// The context in which the unknown type permission applies.
    /// </summary>
    public required string Context { get; set; }
}
