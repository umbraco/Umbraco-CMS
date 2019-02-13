using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json.Linq;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Editors;
using Umbraco.Core.Models.Validation;
using Umbraco.Web.WebApi.Filters;

namespace Umbraco.Web.Models.ContentEditing
{
    /// <inheritdoc />
    public class MemberSave : ContentBaseSave<IMember>
    {

        [DataMember(Name = "username", IsRequired = true)]
        [RequiredForPersistence(AllowEmptyStrings = false, ErrorMessage = "Required")]
        public string Username { get; set; }

        [DataMember(Name = "email", IsRequired = true)]
        [RequiredForPersistence(AllowEmptyStrings = false, ErrorMessage = "Required")]
        public string Email { get; set; }

        [DataMember(Name = "password")]
        public ChangingPasswordModel Password { get; set; }

        [DataMember(Name = "memberGroups")]
        public IEnumerable<string> Groups { get; set; }

        [DataMember(Name = "comments")]
        public string Comments { get; set; }

        [DataMember(Name = "isLockedOut")]
        public bool IsLockedOut { get; set; }

        [DataMember(Name = "isApproved")]
        public bool IsApproved { get; set; }
        

        // TODO: Need to add question / answer support
    }
}
