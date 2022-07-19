using System.ComponentModel.DataAnnotations;

namespace Umbraco.Cms.Core.Models;

/// <summary>
///     A model representing an attempt at changing a password
/// </summary>
public class PasswordChangedModel
{
    /// <summary>
    ///     The error affiliated with the failing password changes, null if changing was successful
    /// </summary>
    public ValidationResult? ChangeError { get; set; }

    /// <summary>
    ///     If the password was reset, this is the value it has been changed to
    /// </summary>
    public string? ResetPassword { get; set; }
}
