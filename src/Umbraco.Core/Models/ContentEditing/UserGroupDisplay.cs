using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Models.ContentEditing;

[DataContract(Name = "userGroup", Namespace = "")]
public class UserGroupDisplay : UserGroupBasic
{
    public UserGroupDisplay()
    {
        Users = Enumerable.Empty<UserBasic>();
        AssignedPermissions = Enumerable.Empty<AssignedContentPermissions>();
    }

    [DataMember(Name = "users")]
    public IEnumerable<UserBasic> Users { get; set; }

    /// <summary>
    ///     The default permissions for the user group organized by permission group name
    /// </summary>
    [DataMember(Name = "defaultPermissions")]
    public IDictionary<string, IEnumerable<Permission>>? DefaultPermissions { get; set; }

    /// <summary>
    ///     The assigned permissions for the user group organized by permission group name
    /// </summary>
    [DataMember(Name = "assignedPermissions")]
    public IEnumerable<AssignedContentPermissions> AssignedPermissions { get; set; }
}
