using System.ComponentModel.DataAnnotations;

namespace Umbraco.Cms.Api.Management.ViewModels.TextFiles;

public class TextFileResponseModelBase : TextFileViewModelBase
{
    [Required]
    public string Path { get; set; } = string.Empty;
}
