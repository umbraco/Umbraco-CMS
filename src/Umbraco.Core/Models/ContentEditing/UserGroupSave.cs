using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Models.ContentEditing;

[DataContract(Name = "userGroup", Namespace = "")]
public class UserGroupSave : EntityBasic, IValidatableObject
{
    /// <summary>
    ///     The action to perform when saving this user group
    /// </summary>
    /// <remarks>
    ///     If either of the Publish actions are specified an exception will be thrown.
    /// </remarks>
    [DataMember(Name = "action", IsRequired = true)]
    [Required]
    public ContentSaveAction Action { get; set; }

    [DataMember(Name = "alias", IsRequired = true)]
    [Required]
    public override string Alias { get; set; } = string.Empty;

    [DataMember(Name = "sections")]
    public IEnumerable<string>? Sections { get; set; }

    [DataMember(Name = "users")]
    public IEnumerable<int>? Users { get; set; }

    [DataMember(Name = "startContentId")]
    public int? StartContentId { get; set; }

    [DataMember(Name = "startMediaId")]
    public int? StartMediaId { get; set; }

    [DataMember(Name = "hasAccessToAllLanguages")]
    public bool HasAccessToAllLanguages { get; set; }

    /// <summary>
    ///     The list of letters (permission codes) to assign as the default for the user group
    /// </summary>
    [DataMember(Name = "defaultPermissions")]
    public IEnumerable<string>? DefaultPermissions { get; set; }

    /// <summary>
    ///     The assigned permissions for content
    /// </summary>
    /// <remarks>
    ///     The key is the content id and the list is the list of letters (permission codes) to assign
    /// </remarks>
    [DataMember(Name = "assignedPermissions")]
    public IDictionary<int, IEnumerable<string>>? AssignedPermissions { get; set; }

    /// <summary>
    /// The ids of allowed languages
    /// </summary>
    [DataMember(Name = "allowedLanguages")]
    public IEnumerable<int>? AllowedLanguages { get; set; }

    /// <summary>
    ///     The real persisted user group
    /// </summary>
    [IgnoreDataMember]
    public IUserGroup? PersistedUserGroup { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (DefaultPermissions?.Any(x => x.IsNullOrWhiteSpace()) ?? false)
        {
            yield return new ValidationResult("A permission value cannot be null or empty", new[] { "Permissions" });
        }

        if (AssignedPermissions is not null)
        {
            foreach (KeyValuePair<int, IEnumerable<string>> assignedPermission in AssignedPermissions)
            {
                foreach (var permission in assignedPermission.Value)
                {
                    if (permission.IsNullOrWhiteSpace())
                    {
                        yield return new ValidationResult(
                            "A permission value cannot be null or empty",
                            new[] { "AssignedPermissions" });
                    }
                }
            }
        }
    }
}
