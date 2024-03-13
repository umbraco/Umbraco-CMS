using System.ComponentModel.DataAnnotations;

namespace Umbraco.Cms.Api.Management.ViewModels.Server;

public class VersionResponseModel
{
    [Required]
    public string Version { get; set; } = string.Empty;
}
