using System.ComponentModel.DataAnnotations;

namespace Umbraco.Cms.Api.Management.ViewModels.Culture;

public class CultureReponseModel
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string EnglishName { get; set; } = string.Empty;
}
