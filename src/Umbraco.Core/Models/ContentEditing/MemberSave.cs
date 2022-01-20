﻿using System.Collections.Generic;
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
        public string Username { get; set; }

        [DataMember(Name = "email", IsRequired = true)]
        [RequiredForPersistence(AllowEmptyStrings = false, ErrorMessage = "Required")]
        [EmailAddress]
        public string Email { get; set; }

        [DataMember(Name = "password")]
        public ChangingPasswordModel Password { get; set; }

        [DataMember(Name = "memberGroups")]
        public IEnumerable<string> Groups { get; set; }

        /// <summary>
        /// Returns the value from the Comments property
        /// </summary>
        public string Comments => GetPropertyValue<string>(Constants.Conventions.Member.Comments);

        /// <summary>
        /// Returns the value from the IsLockedOut property
        /// </summary>
        public bool IsLockedOut => GetPropertyValue<bool>(Constants.Conventions.Member.IsLockedOut);

        /// <summary>
        /// Returns the value from the IsApproved property
        /// </summary>
        public bool IsApproved => GetPropertyValue<bool>(Constants.Conventions.Member.IsApproved);

        private T GetPropertyValue<T>(string alias)
        {
            var prop = Properties.FirstOrDefault(x => x.Alias == alias);
            if (prop == null) return default;
            var converted = prop.Value.TryConvertTo<T>();
            return converted.ResultOr(default);
        }
    }
}
