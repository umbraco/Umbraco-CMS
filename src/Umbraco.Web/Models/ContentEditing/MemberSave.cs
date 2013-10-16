using System.Runtime.Serialization;
using Newtonsoft.Json.Linq;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Validation;

namespace Umbraco.Web.Models.ContentEditing
{
    /// <summary>
    /// A model representing a member to be saved
    /// </summary>
    public class MemberSave : ContentBaseItemSave<IMember>
    {
        public MemberSave()
        {
            Password = new ChangePasswordModel();
        }

        [DataMember(Name = "username", IsRequired = true)]
        [RequiredForPersistence(AllowEmptyStrings = false, ErrorMessage = "Required")]
        public string Username { get; set; }

        [DataMember(Name = "email", IsRequired = true)]
        [RequiredForPersistence(AllowEmptyStrings = false, ErrorMessage = "Required")]
        public string Email { get; set; }
        
        [DataMember(Name = "password")]
        public ChangePasswordModel Password { get; set; }
    }
}