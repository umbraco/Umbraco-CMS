using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;

namespace Umbraco.Web.Models.ContentEditing
{
    /// <summary>
    /// A model representing a member to be displayed in the back office
    /// </summary>
    [DataContract(Name = "content", Namespace = "")]
    public class MemberDisplay : ListViewAwareContentItemDisplayBase<ContentPropertyDisplay, IMember>
    {
        public MemberDisplay()
        {
            MemberProviderFieldMapping = new Dictionary<string, string>();
        }

        [DataMember(Name = "username")]
        public string Username { get; set; }

        [DataMember(Name = "email")]
        public string Email { get; set; }

        [DataMember(Name = "membershipScenario")]
        public MembershipScenario MembershipScenario { get; set; }

        /// <summary>
        /// This is used to indicate how to map the membership provider properties to the save model, this mapping
        /// will change if a developer has opted to have custom member property aliases specified in their membership provider config, 
        /// or if we are editing a member that is not an Umbraco member (custom provider)
        /// </summary>
        [DataMember(Name = "fieldConfig")]
        public IDictionary<string, string> MemberProviderFieldMapping { get; set; }
        
    }
}