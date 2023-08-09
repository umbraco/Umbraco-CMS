using System.ComponentModel.DataAnnotations;

namespace Umbraco.Cms.Web.Common.Models;

/// <summary>
///     A model used for setting a new password for a user that has been invited to Umbraco.
/// </summary>
public class InvitePasswordModel
{
    /// <summary>
    ///     Gets or sets the desired password for the user.
    /// </summary>
    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "New password")]
    [StringLength(256)]
    public string NewPassword { get; set; } = null!;
}
