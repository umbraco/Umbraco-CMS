using System.ComponentModel.DataAnnotations;

namespace Umbraco.Cms.Api.Management.ViewModels.JsonPatch;

public class JsonPatchViewModel
{
    [Required]
    public string Op { get; set; } = string.Empty;

    [Required]
    public string Path { get; set; } = string.Empty;

    [Required]
    public object Value { get; set; } = null!;
}
