using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Models;

/// <summary>
///     Used for 2FA verification
/// </summary>
public class Verify2FACodeModel
{
    [Required]
    public required string Code { get; set; }

    [Required]
    public required string Provider { get; set; }

    /// <summary>
    ///     Flag indicating whether the sign-in cookie should persist after the browser is closed.
    /// </summary>
    public bool IsPersistent { get; set; }

    /// <summary>
    ///     Flag indicating whether the current browser should be remember, suppressing all further two factor authentication
    ///     prompts.
    /// </summary>
    public bool RememberClient { get; set; }
}
