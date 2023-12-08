using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Umbraco.Cms.Core.Models.Validation;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Models.ContentEditing;

/// <inheritdoc />
public class MemberSave : ContentBaseSave<IMember>
{
    [DataMember(Name = "username", IsRequired = true)]
    [RequiredForPersistence(AllowEmptyStrings = false, ErrorMessage = "Required")]
    public string Username { get; set; } = null!;

    [DataMember(Name = "email", IsRequired = true)]
    [RequiredForPersistence(AllowEmptyStrings = false, ErrorMessage = "Required")]
    [EmailAddress]
    public string Email { get; set; } = null!;

    [DataMember(Name = "password")]
    public ChangingPasswordModel? Password { get; set; }

    [DataMember(Name = "memberGroups")]
    public IEnumerable<string>? Groups { get; set; }

    [DataMember(Name = "isLockedOut")]
    public bool IsLockedOut { get; set; }

    [DataMember(Name = "isApproved")]
    public bool IsApproved { get; set; }

    [DataMember(Name = "isTwoFactorEnabled")]
    public bool IsTwoFactorEnabled { get; set; }
}
