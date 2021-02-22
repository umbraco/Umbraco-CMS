using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Umbraco.Cms.Core.Models.Security
{
    /// <summary>
    /// A readonly member profile model
    /// </summary>
    public class ProfileModel : PostRedirectModel
    {
        [Required]
        [RegularExpression(@"[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?",
            ErrorMessage = "Please enter a valid e-mail address")]
        public string Email { get; set; }

        /// <summary>
        /// The member's real name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The member's member type alias
        /// </summary>
        [ReadOnly(true)]
        public string MemberTypeAlias { get; set; }

        [ReadOnly(true)]
        public string UserName { get; set; }

        [ReadOnly(true)]
        public string Comment { get; set; }

        [ReadOnly(true)]
        public bool IsApproved { get; set; }

        [ReadOnly(true)]
        public bool IsLockedOut { get; set; }

        [ReadOnly(true)]
        public DateTime LastLockoutDate { get; set; }

        [ReadOnly(true)]
        public DateTime CreationDate { get; set; }

        [ReadOnly(true)]
        public DateTime LastLoginDate { get; set; }

        [ReadOnly(true)]
        public DateTime LastActivityDate { get; set; }

        [ReadOnly(true)]
        public DateTime LastPasswordChangedDate { get; set; }

        /// <summary>
        /// The list of member properties
        /// </summary>
        /// <remarks>
        /// Adding items to this list on the front-end will not add properties to the member in the database.
        /// </remarks>
        public List<UmbracoProperty> MemberProperties { get; set; } = new List<UmbracoProperty>();
    }
}
