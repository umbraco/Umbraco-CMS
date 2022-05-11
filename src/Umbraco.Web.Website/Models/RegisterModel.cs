using System.ComponentModel.DataAnnotations;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Web.Common.Models;
using DataType = System.ComponentModel.DataAnnotations.DataType;

namespace Umbraco.Cms.Web.Website.Models;

public class RegisterModel : PostRedirectModel
{
    public RegisterModel()
    {
        MemberTypeAlias = Constants.Conventions.MemberTypes.DefaultAlias;
        UsernameIsEmail = true;
        MemberProperties = new List<MemberPropertyModel>();
    }

    [Required]
    [EmailAddress]
    [Display(Name = "Email")]
    public string Email { get; set; } = null!;

    /// <summary>
    ///     Returns the member properties
    /// </summary>
    public List<MemberPropertyModel> MemberProperties { get; set; }

    /// <summary>
    ///     The member type alias to use to register the member
    /// </summary>
    [Editable(false)]
    public string MemberTypeAlias { get; set; }

    /// <summary>
    ///     The members real name
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    ///     The members password
    /// </summary>
    [Required]
    [StringLength(256)]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; } = null!;

    [DataType(DataType.Password)]
    [Display(Name = "Confirm password")]
    [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
    public string ConfirmPassword { get; set; } = null!;

    /// <summary>
    ///     The username of the model, if UsernameIsEmail is true then this is ignored.
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    ///     Flag to determine if the username should be the email address, if true then the Username property is ignored
    /// </summary>
    public bool UsernameIsEmail { get; set; }
}
