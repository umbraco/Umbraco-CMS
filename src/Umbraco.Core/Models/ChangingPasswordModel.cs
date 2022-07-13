using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Models;

/// <summary>
///     A model representing the data required to set a member/user password depending on the provider installed.
/// </summary>
public class ChangingPasswordModel
{
    /// <summary>
    ///     The password value
    /// </summary>
    [DataMember(Name = "newPassword")]
    public string? NewPassword { get; set; }

    /// <summary>
    ///     The old password - used to change a password when: EnablePasswordRetrieval = false
    /// </summary>
    [DataMember(Name = "oldPassword")]
    public string? OldPassword { get; set; }

    /// <summary>
    ///     The ID of the current user/member requesting the password change
    ///     For users, required to allow changing password without the entire UserSave model
    /// </summary>
    [DataMember(Name = "id")]
    public int Id { get; set; }
}
