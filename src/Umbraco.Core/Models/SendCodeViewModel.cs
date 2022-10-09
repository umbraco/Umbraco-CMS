using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Models;

/// <summary>
///     Used for 2FA verification
/// </summary>
[DataContract(Name = "code", Namespace = "")]
public class Verify2FACodeModel
{
    [Required]
    [DataMember(Name = "code", IsRequired = true)]
    public string? Code { get; set; }

    [Required]
    [DataMember(Name = "provider", IsRequired = true)]
    public string? Provider { get; set; }

    /// <summary>
    ///     Flag indicating whether the sign-in cookie should persist after the browser is closed.
    /// </summary>
    [DataMember(Name = "isPersistent", IsRequired = true)]
    public bool IsPersistent { get; set; }

    /// <summary>
    ///     Flag indicating whether the current browser should be remember, suppressing all further two factor authentication
    ///     prompts.
    /// </summary>
    [DataMember(Name = "rememberClient", IsRequired = true)]
    public bool RememberClient { get; set; }
}
