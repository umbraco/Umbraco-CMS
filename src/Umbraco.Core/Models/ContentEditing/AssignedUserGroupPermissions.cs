using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Models.ContentEditing;

/// <summary>
///     The user group permissions assigned to a content node
/// </summary>
/// <remarks>
///     The underlying <see cref="EntityBasic" /> data such as Name, etc... is that of the User Group
/// </remarks>
[DataContract(Name = "userGroupPermissions", Namespace = "")]
public class AssignedUserGroupPermissions : EntityBasic
{
    /// <summary>
    ///     The assigned permissions for the user group organized by permission group name
    /// </summary>
    [DataMember(Name = "permissions")]
    public IDictionary<string, IEnumerable<Permission>>? AssignedPermissions { get; set; }

    /// <summary>
    ///     The default permissions for the user group organized by permission group name
    /// </summary>
    [DataMember(Name = "defaultPermissions")]
    public IDictionary<string, IEnumerable<Permission>>? DefaultPermissions { get; set; }

    public static IDictionary<string, IEnumerable<Permission>> ClonePermissions(
        IDictionary<string, IEnumerable<Permission>>? permissions)
    {
        var result = new Dictionary<string, IEnumerable<Permission>>();
        if (permissions is not null)
        {
            foreach (KeyValuePair<string, IEnumerable<Permission>> permission in permissions)
            {
                result[permission.Key] = new List<Permission>(permission.Value.Select(x => (Permission)x.Clone()));
            }
        }

        return result;
    }
}
