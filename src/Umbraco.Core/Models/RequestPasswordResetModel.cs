using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Models;

/// <summary>
/// Represents a model for requesting a password reset.
/// </summary>
[DataContract(Name = "requestPasswordReset", Namespace = "")]
public class RequestPasswordResetModel
{
    /// <summary>
    /// Gets or sets the email address of the user requesting the password reset.
    /// </summary>
    [Required]
    [DataMember(Name = "email", IsRequired = true)]
    public string Email { get; set; } = null!;
}
