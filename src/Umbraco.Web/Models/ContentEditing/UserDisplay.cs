using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.Serialization;

namespace Umbraco.Web.Models.ContentEditing
{
    /// <summary>
    /// Represents a user that is being edited
    /// </summary>
    [DataContract(Name = "user", Namespace = "")]
    [ReadOnly(true)]
    public class UserDisplay : UserBasic
    {
        public UserDisplay() : base()
        {
        }

        /// <summary>
        /// The list of group aliases assigned to the user
        /// </summary>
        [DataMember(Name = "userGroups")]
        public IEnumerable<UserGroupBasic> UserGroups { get; set; }        

        /// <summary>
        /// Gets the available cultures (i.e. to populate a drop down)
        /// The key is the culture stored in the database, the value is the Name
        /// </summary>
        [DataMember(Name = "availableCultures")]
        public IDictionary<string, string> AvailableCultures { get; set; }
        
        [DataMember(Name = "startContentIds")]
        public IEnumerable<EntityBasic> StartContentIds { get; set; }

        [DataMember(Name = "startMediaIds")]
        public IEnumerable<EntityBasic> StartMediaIds { get; set; }

        /// <summary>
        /// If the password is reset on save, this value will be populated
        /// </summary>
        [DataMember(Name = "resetPasswordValue")]
        [ReadOnly(true)]
        public string ResetPasswordValue { get; set; }

        /// <summary>
        /// This is an info flag to denote if this object is the equivalent of the currently logged in user
        /// </summary>
        [DataMember(Name = "isCurrentUser")]
        [ReadOnly(true)]
        public bool IsCurrentUser { get; set; }
    }
}