using System.ComponentModel.DataAnnotations;
using Umbraco.Cms.Api.Management.ViewModels.Item;

namespace Umbraco.Cms.Api.Management.ViewModels.RecycleBin;

public abstract class RecycleBinItemResponseModelBase
{
    [Required]
    public Guid Id { get; set; }

    public DateTimeOffset CreateDate { get; set; }

    [Required]
    public bool HasChildren { get; set; }

    public ItemReferenceByIdResponseModel? Parent { get; set; }
}
