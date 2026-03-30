namespace Umbraco.Cms.Api.Management.ViewModels.Document;

/// <summary>
/// Represents the data model used for assigning domains to a document within the management API.
/// </summary>
public sealed class DomainAssignmentModel
{
    /// <summary>Gets or sets the domain name assigned to the document.</summary>
    
    public required string DomainName { get; set; }

    /// <summary>
    /// Gets or sets a reference to the content item that this domain assignment applies to.
    /// </summary>
    public required ReferenceByIdModel Content { get; set; }
}
