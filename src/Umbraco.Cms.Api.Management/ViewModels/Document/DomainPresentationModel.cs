namespace Umbraco.Cms.Api.Management.ViewModels.Document;

/// <summary>
/// Represents a domain presentation model in the Umbraco CMS Management API.
/// </summary>
public class DomainPresentationModel
{
    /// <summary>
    /// Gets or sets the assigned domain name for the document.
    /// </summary>
    public required string DomainName { get; set; }

    /// <summary>
    /// Gets or sets the ISO code representing the domain.
    /// </summary>
    public required string IsoCode { get; set; }
}
