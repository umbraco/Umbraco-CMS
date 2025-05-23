using System.ComponentModel.DataAnnotations;

namespace Umbraco.Cms.Api.Management.ViewModels.Manifest;

public class ManifestResponseModel
{
    [Required]
    public string Name { get; set; } = string.Empty;

    public string? Id { get; set; }

    public string? Version { get; set; }

    public object[] Extensions { get; set; } = Array.Empty<object>();
}
