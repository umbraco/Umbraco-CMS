using System.ComponentModel;
using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Models.ContentEditing;

/// <summary>
///     Represents a user that is being edited
/// </summary>
[DataContract(Name = "user", Namespace = "")]
[ReadOnly(true)]
public class UserDisplay : UserBasic
{
    public UserDisplay()
    {
        AvailableCultures = new Dictionary<string, string>();
        StartContentIds = new List<EntityBasic>();
        StartMediaIds = new List<EntityBasic>();
        Navigation = new List<EditorNavigation>();
    }

    [DataMember(Name = "navigation")]
    [ReadOnly(true)]
    public IEnumerable<EditorNavigation> Navigation { get; set; }

    /// <summary>
    ///     Gets the available cultures (i.e. to populate a drop down)
    ///     The key is the culture stored in the database, the value is the Name
    /// </summary>
    [DataMember(Name = "availableCultures")]
    public IDictionary<string, string> AvailableCultures { get; set; }

    [DataMember(Name = "startContentIds")]
    public IEnumerable<EntityBasic> StartContentIds { get; set; }

    [DataMember(Name = "startMediaIds")]
    public IEnumerable<EntityBasic> StartMediaIds { get; set; }

    /// <summary>
    ///     If the password is reset on save, this value will be populated
    /// </summary>
    [DataMember(Name = "resetPasswordValue")]
    [ReadOnly(true)]
    public string? ResetPasswordValue { get; set; }

    /// <summary>
    ///     A readonly value showing the user's current calculated start content ids
    /// </summary>
    [DataMember(Name = "calculatedStartContentIds")]
    [ReadOnly(true)]
    public IEnumerable<EntityBasic>? CalculatedStartContentIds { get; set; }

    /// <summary>
    ///     A readonly value showing the user's current calculated start media ids
    /// </summary>
    [DataMember(Name = "calculatedStartMediaIds")]
    [ReadOnly(true)]
    public IEnumerable<EntityBasic>? CalculatedStartMediaIds { get; set; }

    [DataMember(Name = "failedPasswordAttempts")]
    [ReadOnly(true)]
    public int FailedPasswordAttempts { get; set; }

    [DataMember(Name = "lastLockoutDate")]
    [ReadOnly(true)]
    public DateTime? LastLockoutDate { get; set; }

    [DataMember(Name = "lastPasswordChangeDate")]
    [ReadOnly(true)]
    public DateTime? LastPasswordChangeDate { get; set; }

    [DataMember(Name = "createDate")]
    [ReadOnly(true)]
    public DateTime CreateDate { get; set; }

    [DataMember(Name = "updateDate")]
    [ReadOnly(true)]
    public DateTime UpdateDate { get; set; }
}
