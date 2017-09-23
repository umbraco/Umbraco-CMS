using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using Umbraco.Core;

namespace Umbraco.Web.Models.ContentEditing
{
    /// <summary>
    /// Used to assign user group permissions to a content node
    /// </summary>
    [DataContract(Name = "contentPermission", Namespace = "")]
    public class UserGroupPermissionsSave : IValidatableObject
    {
        public UserGroupPermissionsSave()
        {
            AssignedPermissions = new Dictionary<int, IEnumerable<string>>();
        }

        //TODO: we should have an option to clear the permissions assigned to this node and instead just have them inherit - yes once we actually have inheritance!

        [DataMember(Name = "contentId", IsRequired = true)]
        [Required]
        public int ContentId { get; set; }

        /// <summary>
        /// A dictionary of permissions to assign, the key is the user group id
        /// </summary>
        [DataMember(Name = "permissions")]
        public IDictionary<int, IEnumerable<string>> AssignedPermissions { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (AssignedPermissions.SelectMany(x => x.Value).Any(x => x.IsNullOrWhiteSpace()))
            {
                yield return new ValidationResult("A permission value cannot be null or empty", new[] { "Permissions" });
            }
        }
    }
}
