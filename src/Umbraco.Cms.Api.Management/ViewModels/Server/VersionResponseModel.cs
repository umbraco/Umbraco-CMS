using System.ComponentModel.DataAnnotations;

namespace Umbraco.Cms.Api.Management.ViewModels.Server;

[Obsolete("Not used. Will be removed in V15.")]
public class VersionResponseModel
{
    [Required]
    public string Version { get; set; } = string.Empty;
}
