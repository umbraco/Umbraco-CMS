using System.ComponentModel.DataAnnotations;

namespace Umbraco.Cms.Api.Management.ViewModels.Manifest;

/// <summary>
/// Represents the response model containing manifest information returned by the management API.
/// Typically includes metadata or configuration details for the Umbraco CMS.
/// </summary>
public class ManifestResponseModel
{
    /// <summary>
    /// Gets or sets the name of the manifest.
    /// </summary>
    [Required]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the identifier of the manifest.
    /// </summary>
    public string? Id { get; set; }

    /// <summary>
    /// Gets or sets the version of the manifest.
    /// </summary>
    public string? Version { get; set; }

    /// <summary>
    /// Gets or sets the extensions associated with this manifest response.
    /// </summary>
    public object[] Extensions { get; set; } = Array.Empty<object>();
}
