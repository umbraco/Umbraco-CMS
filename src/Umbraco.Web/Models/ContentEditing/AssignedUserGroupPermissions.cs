using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Umbraco.Web.Models.ContentEditing
{
    /// <summary>
    /// The user group permissions assigned to a content node
    /// </summary>
    [DataContract(Name = "contentPermission", Namespace = "")]
    public class AssignedUserGroupPermissions : EntityBasic
    {
        /// <summary>
        /// The default permissions for the user group organized by permission group name
        /// </summary>
        [DataMember(Name = "permissions")]
        public IDictionary<string, IEnumerable<Permission>> AssignedPermissions { get; set; }
    }
}