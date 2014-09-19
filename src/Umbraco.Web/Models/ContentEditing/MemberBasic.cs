using System.Runtime.Serialization;
using Umbraco.Core.Models;

namespace Umbraco.Web.Models.ContentEditing
{
    /// <summary>
    /// Used for basic member information
    /// </summary>
    public class MemberBasic : ContentItemBasic<ContentPropertyBasic, IMember>
    {
        [DataMember(Name = "username")]
        public string Username { get; set; }

        [DataMember(Name = "email")]
        public string Email { get; set; }
    }
}