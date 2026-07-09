namespace Umbraco.Cms.Api.Management.ViewModels.UserGroup.Permissions;

/// <summary>
/// Presentation model that defines permissions related to document property values for a user group.
/// </summary>
public class DocumentPropertyValuePermissionPresentationModel : IPermissionPresentationModel
{
    /// <summary>
    /// Gets or sets a reference to the document type associated with this permission.
    /// </summary>
    public required ReferenceByIdModel DocumentType { get; set; }

    /// <summary>
    /// Gets or sets the property type to which this document property value permission applies.
    /// </summary>
    public required ReferenceByIdModel PropertyType { get; set; }

    /// <summary>
    /// Gets or sets the set of verbs associated with the document property value permission.
    /// </summary>
    public required ISet<string> Verbs { get; set; }
}
