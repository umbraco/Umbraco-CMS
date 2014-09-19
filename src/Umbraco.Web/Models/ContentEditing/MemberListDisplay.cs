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
    }
}