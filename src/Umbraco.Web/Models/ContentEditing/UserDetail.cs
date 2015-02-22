using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Umbraco.Web.Models.ContentEditing
{
    [DataContract(Name = "user", Namespace = "")]
    public class UserDetail : UserBasic
    {
        [DataMember(Name = "email", IsRequired = true)]
        [Required]
        public string Email { get; set; }

        [DataMember(Name = "locale", IsRequired = true)]
        [Required]
        public string Culture { get; set; }

        /// <summary>
        /// The MD5 lowercase hash of the email which can be used by gravatar
        /// </summary>
        [DataMember(Name = "emailHash")]
        public string EmailHash { get; set; }

        [DataMember(Name = "userType", IsRequired = true)]
        [Required]
        public string UserType { get; set; }

        /// <summary>
        /// Gets/sets the number of seconds for the user's auth ticket to expire
        /// </summary>
        [DataMember(Name = "remainingAuthSeconds")]
        public double SecondsUntilTimeout { get; set; }

        [DataMember(Name = "startContentId")]
        public int StartContentId { get; set; }

        [DataMember(Name = "startMediaId")]
        public int StartMediaId { get; set; }

        /// <summary>
        /// A list of sections the user is allowed to view.
        /// </summary>
        [DataMember(Name = "allowedSections")]
        public IEnumerable<string> AllowedSections { get; set; }

        
    }
}