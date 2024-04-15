using System.ComponentModel.DataAnnotations;

namespace Umbraco.Cms.Api.Management.ViewModels.Segment;

public class SegmentResponseModel
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Alias { get; set; } = string.Empty;
}
