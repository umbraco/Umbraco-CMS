using System;
using System.Runtime.Serialization;
using Umbraco.Core.Models;

namespace Umbraco.Web.Models.ContentEditing
{
    /// <summary>
    /// A model representing a member to be displayed in the back office
    /// </summary>
    [DataContract(Name = "content", Namespace = "")]
    public class MemberDisplay : ContentItemDisplayBase<ContentPropertyDisplay, IMember>
    {

        [DataMember(Name = "username")]
        public string Username { get; set; }

        [DataMember(Name = "email")]
        public string Email { get; set; }

        /// <summary>
        /// This is the unique Id stored in the database - but could also be the unique id for a custom membership provider
        /// </summary>
        [DataMember(Name = "key")]
        public Guid Key { get; set; }

    }
}