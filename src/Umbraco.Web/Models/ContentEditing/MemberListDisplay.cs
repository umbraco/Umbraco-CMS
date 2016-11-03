using System.Collections.Generic;
using System.Runtime.Serialization;
using Umbraco.Core.Models;

namespace Umbraco.Web.Models.ContentEditing
{
    /// <summary>
    /// A model representing a member list to be displayed in the back office
    /// </summary>
    [DataContract(Name = "content", Namespace = "")]
    public class MemberListDisplay : ContentItemDisplayBase<ContentPropertyDisplay, IMember>
    {
        /// <summary>
        /// The allowed 'actions' based on the user's permissions - Create, Update, Publish, Send to publish
        /// </summary>
        /// <remarks>
        /// Each char represents a button which we can then map on the front-end to the correct actions
        /// </remarks>
        [DataMember(Name = "allowedActions")]
        public IEnumerable<char> AllowedActions { get; set; }
    }
}