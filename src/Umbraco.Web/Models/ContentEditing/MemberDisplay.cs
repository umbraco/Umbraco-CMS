using System.Runtime.Serialization;

namespace Umbraco.Web.Models.ContentEditing
{
    /// <summary>
    /// A model representing a member to be displayed in the back office
    /// </summary>
    [DataContract(Name = "content", Namespace = "")]
    public class MemberDisplay : ListViewAwareContentItemDisplayBase<ContentPropertyDisplay>
    {
        [DataMember(Name = "username")]
        public string Username { get; set; }

        [DataMember(Name = "email")]
        public string Email { get; set; }

    }
}
