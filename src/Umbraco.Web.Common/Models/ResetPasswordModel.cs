using System.ComponentModel.DataAnnotations;

namespace Umbraco.Cms.Web.Common.Models;

public class ResetPasswordModel : PostRedirectModel
{
    [Required]
    public string UserId { get; set; } = null!;

    [Required]
    public string Token { get; set; } = null!;

    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    [StringLength(256)]
    public string Password { get; set; } = null!;

    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Confirm Password")]
    [StringLength(256)]
    [Compare(nameof(Password), ErrorMessage = "Your passwords do not match")]
    public string ConfirmPassword { get; set; } = null!;
}
