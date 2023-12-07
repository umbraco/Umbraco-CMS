using System.ComponentModel.DataAnnotations;

namespace Umbraco.Cms.Api.Management.ViewModels.RecycleBin;

public class RecycleBinItemResponseModel : INamedEntityPresentationModel
{
    [Required]
    public Guid Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Type { get; set; } = string.Empty;

    [Required]
    public string Icon { get; set; } = string.Empty;

    [Required]
    public bool HasChildren { get; set; }

    [Required]
    public bool IsContainer { get; set; }

    [Required]
    public Guid? ParentId { get; set; }
}

