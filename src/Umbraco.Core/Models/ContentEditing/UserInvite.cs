using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Models.ContentEditing;

/// <summary>
///     Represents the data used to invite a user
/// </summary>
[DataContract(Name = "user", Namespace = "")]
public class UserInvite : EntityBasic, IValidatableObject
{
    [DataMember(Name = "userGroups")]
    [Required]
    public IEnumerable<string> UserGroups { get; set; } = null!;

    [DataMember(Name = "email", IsRequired = true)]
    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;

    [DataMember(Name = "username")]
    public string? Username { get; set; }

    [DataMember(Name = "message")]
    public string? Message { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (UserGroups.Any() == false)
        {
            yield return new ValidationResult(
                "A user must be assigned to at least one group",
                new[] { nameof(UserGroups) });
        }

        IOptionsSnapshot<SecuritySettings> securitySettings =
            validationContext.GetRequiredService<IOptionsSnapshot<SecuritySettings>>();

        if (securitySettings.Value.UsernameIsEmail == false && Username.IsNullOrWhiteSpace())
        {
            yield return new ValidationResult("A username cannot be empty", new[] { nameof(Username) });
        }
    }
}
