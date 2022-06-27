using System.ComponentModel.DataAnnotations;

namespace Umbraco.Cms.Web.Common.Models;

public class LoginModel : PostRedirectModel
{
    [Required]
    [Display(Name = "User name")]
    public string Username { get; set; } = null!;

    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    [StringLength(256)]
    public string Password { get; set; } = null!;

    [Display(Name = "Remember me?")]
    public bool RememberMe { get; set; }
}
