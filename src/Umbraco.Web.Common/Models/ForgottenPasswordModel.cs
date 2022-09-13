using System.ComponentModel.DataAnnotations;

namespace Umbraco.Cms.Web.Common.Models;

public class ForgottenPasswordModel : PostRedirectModel
{
    [Required]
    [Display(Name = "User name")]
    public string Username { get; set; } = null!;
}
