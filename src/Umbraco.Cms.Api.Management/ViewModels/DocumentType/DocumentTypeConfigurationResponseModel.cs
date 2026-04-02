using Umbraco.Cms.Core.Configuration.Models;

namespace Umbraco.Cms.Api.Management.ViewModels.DocumentType;

/// <summary>
/// Represents a response model containing configuration details for a document type in the Umbraco CMS Management API.
/// </summary>
public class DocumentTypeConfigurationResponseModel
{
    /// <summary>
    /// Gets or sets a value indicating the allowed mode for changing data types in the document type configuration.
    /// </summary>
    public required DataTypeChangeMode DataTypesCanBeChanged { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether templates are disabled for the document type.
    /// </summary>
    public required bool DisableTemplates { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether segments are used for this document type, enabling content segmentation or variations.
    /// </summary>
    public required bool UseSegments { get; set; }

    /// <summary>
    /// Gets or sets the set of reserved field names that cannot be used as property names in document types.
    /// These names are typically reserved by the system to avoid conflicts with built-in functionality.
    /// </summary>
    public required ISet<string> ReservedFieldNames { get; set; }
}
