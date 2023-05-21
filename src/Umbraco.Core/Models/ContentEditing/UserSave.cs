using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Models.ContentEditing;

/// <summary>
///     Represents the data used to persist a user
/// </summary>
/// <remarks>
///     This will be different from the model used to display a user and we don't want to "Overpost" data back to the
///     server,
///     and there will most likely be different bits of data required for updating passwords which will be different from
///     the
///     data used to display vs save
/// </remarks>
[DataContract(Name = "user", Namespace = "")]
public class UserSave : EntityBasic, IValidatableObject
{
    [DataMember(Name = "changePassword", IsRequired = true)]
    public ChangingPasswordModel? ChangePassword { get; set; }

    [DataMember(Name = "id", IsRequired = true)]
    [Required]
    public new int Id { get; set; }

    [DataMember(Name = "username", IsRequired = true)]
    [Required]
    public string Username { get; set; } = null!;

    [DataMember(Name = "culture", IsRequired = true)]
    [Required]
    public string Culture { get; set; } = null!;

    [DataMember(Name = "email", IsRequired = true)]
    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;

    [DataMember(Name = "userGroups")]
    [Required]
    public IEnumerable<string> UserGroups { get; set; } = null!;

    [DataMember(Name = "startContentIds")]
    public int[]? StartContentIds { get; set; }

    [DataMember(Name = "startMediaIds")]
    public int[]? StartMediaIds { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (UserGroups.Any() == false)
        {
            yield return new ValidationResult("A user must be assigned to at least one group", new[] { "UserGroups" });
        }
    }
}
