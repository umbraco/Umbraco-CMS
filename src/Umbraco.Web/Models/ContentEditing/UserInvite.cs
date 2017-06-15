using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Umbraco.Web.Models.ContentEditing
{
    /// <summary>
    /// Represents the data used to invite a user
    /// </summary>
    [DataContract(Name = "user", Namespace = "")]
    public class UserInvite : EntityBasic
    {
        [DataMember(Name = "userGroups")]
        [Required]
        public IEnumerable<string> UserGroups { get; set; }

        [DataMember(Name = "email", IsRequired = true)]
        [Required]
        [EmailAddress]
        public string Email { get; set; }        

        [DataMember(Name = "message")]
        public string Message { get; set; }
        
    }
}