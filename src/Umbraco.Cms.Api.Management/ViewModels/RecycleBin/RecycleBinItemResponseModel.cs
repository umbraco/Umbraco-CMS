using System.ComponentModel.DataAnnotations;

namespace Umbraco.Cms.Api.Management.ViewModels.RecycleBin;

public class RecycleBinItemResponseModel : INamedEntityPresentationModel
{
    public Guid Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Type { get; set; } = string.Empty;

    [Required]
    public string Icon { get; set; } = string.Empty;

    public bool HasChildren { get; set; }

    public bool IsContainer { get; set; }

    public Guid? ParentId { get; set; }
}

