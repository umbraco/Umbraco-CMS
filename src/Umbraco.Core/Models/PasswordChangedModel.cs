using System.ComponentModel.DataAnnotations;
using Umbraco.Cms.Core.Models.Membership;

namespace Umbraco.Cms.Core.Models;

/// <summary>
///     A model representing an attempt at changing a password
/// </summary>
public class PasswordChangedModel : ErrorMessageResult
{
    /// <summary>
    ///     If the password was reset, this is the value it has been changed to
    /// </summary>
    public string? ResetPassword { get; set; }
}
