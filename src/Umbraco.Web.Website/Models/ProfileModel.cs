using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Web.Common.Models;

namespace Umbraco.Cms.Web.Website.Models;

/// <summary>
///     A readonly member profile model
/// </summary>
public class ProfileModel : PostRedirectModel
{
    [ReadOnly(true)]
    public Guid Key { get; set; }

    [Required]
    [EmailAddress]
    [Display(Name = "Email")]
    public string Email { get; set; } = null!;

    /// <summary>
    ///     The member's real name
    /// </summary>
    public string? Name { get; set; }

    [ReadOnly(true)]
    public string UserName { get; set; } = null!;

    [ReadOnly(true)]
    public string? Comments { get; set; }

    [ReadOnly(true)]
    public bool IsApproved { get; set; }

    [ReadOnly(true)]
    public bool IsLockedOut { get; set; }

    [ReadOnly(true)]
    public DateTime? LastLockoutDate { get; set; }

    [ReadOnly(true)]
    public DateTime CreatedDate { get; set; }

    [ReadOnly(true)]
    public DateTime? LastLoginDate { get; set; }

    [ReadOnly(true)]
    public DateTime? LastPasswordChangedDate { get; set; }

    /// <summary>
    ///     The list of member properties
    /// </summary>
    /// <remarks>
    ///     Adding items to this list on the front-end will not add properties to the member in the database.
    /// </remarks>
    public List<MemberPropertyModel> MemberProperties { get; set; } = new();
}
