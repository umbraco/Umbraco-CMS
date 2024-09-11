using System.ComponentModel.DataAnnotations;

namespace Umbraco.Cms.Api.Management.ViewModels.Folder;

public class FolderModelBase
{
    [Required]
    public string Name { get; set; } = string.Empty;
}
