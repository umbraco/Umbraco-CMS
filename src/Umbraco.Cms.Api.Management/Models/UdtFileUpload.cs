using System.Xml.Linq;

namespace Umbraco.Cms.Api.Management.Models;

/// <summary>
/// Represents the data required to upload a file to a User Defined Table (UDT) in Umbraco CMS.
/// This model is typically used to handle file upload requests via the management API.
/// </summary>
public class UdtFileUpload
{
    /// <summary>
    /// Gets or sets the XML content of the uploaded UDT (User Defined Table) file.
    /// </summary>
    public required XDocument Content { get; set; }

    /// <summary>
    /// Gets or sets the name of the uploaded file.
    /// </summary>
    public required string FileName { get; init; } = string.Empty;
}
