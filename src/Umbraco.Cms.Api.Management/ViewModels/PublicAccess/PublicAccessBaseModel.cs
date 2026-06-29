namespace Umbraco.Cms.Api.Management.ViewModels.PublicAccess;

/// <summary>
/// Represents the base model for configuring public access settings in the API management context.
/// </summary>
public class PublicAccessBaseModel
{
    /// <summary>
    /// Gets or sets the reference to the document used for login when accessing protected content.
    /// </summary>
    public required ReferenceByIdModel LoginDocument { get; set; }

    /// <summary>
    /// Gets or sets the reference to the error document that is displayed when access is denied.
    /// </summary>
    public required ReferenceByIdModel ErrorDocument { get; set; }
}
