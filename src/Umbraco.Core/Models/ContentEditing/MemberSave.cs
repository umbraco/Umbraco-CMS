using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using Umbraco.Cms.Core.Models.Validation;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Models.ContentEditing
{
    /// <inheritdoc />
    public class MemberSave : ContentBaseSave<IMember>
    {

        [DataMember(Name = "username", IsRequired = true)]
        [RequiredForPersistence(AllowEmptyStrings = false, ErrorMessage = "Required")]
        public string Username { get; set; } = null!;

        [DataMember(Name = "email", IsRequired = true)]
        [RequiredForPersistence(AllowEmptyStrings = false, ErrorMessage = "Required")]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [DataMember(Name = "password")]
        public ChangingPasswordModel? Password { get; set; }

        [DataMember(Name = "memberGroups")]
        public IEnumerable<string>? Groups { get; set; }

        /// <summary>
        /// Returns the value from the Comments property
        /// </summary>
        public string? Comments => GetPropertyValue<string>(Constants.Conventions.Member.Comments);

        [DataMember(Name = "isLockedOut")]
        public bool IsLockedOut { get; set; }

        [DataMember(Name = "isApproved")]
        public bool IsApproved { get; set; }

        private T? GetPropertyValue<T>(string alias)
        {
            var prop = Properties.FirstOrDefault(x => x.Alias == alias);
            if (prop == null) return default;
            var converted = prop.Value.TryConvertTo<T>();
            return converted.Result ?? default;
        }
    }
}
