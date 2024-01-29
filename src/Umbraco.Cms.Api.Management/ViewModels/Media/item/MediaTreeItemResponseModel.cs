using System.ComponentModel.DataAnnotations;
using Umbraco.Cms.Api.Management.ViewModels.Tree;

namespace Umbraco.Cms.Api.Management.ViewModels.Media.Item;

public class MediaTreeItemResponseModel : ContentTreeItemResponseModel
{
    [Required]
    public string Icon { get; set; } = string.Empty;
}
