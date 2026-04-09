using System.ComponentModel.DataAnnotations;

namespace Umbraco.Cms.Api.Management.ViewModels.DocumentType;

/// <summary>
/// Represents the data required to create a template for a document type in the Umbraco CMS.
/// </summary>
public class CreateDocumentTypeTemplateRequestModel
{
    /// <summary>
    /// Gets or sets the unique alias identifier for the document type template.
    /// </summary>
    [Required]
    public required string Alias { get; set; }

    /// <summary>
    /// Gets or sets the name of the document type template.
    /// </summary>
    [Required]
    public required string Name { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this template is the default template.
    /// </summary>
    public bool IsDefault { get; set; }
}
