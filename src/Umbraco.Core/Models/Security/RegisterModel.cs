using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Umbraco.Cms.Core.Models.Security
{
    public class RegisterModel : PostRedirectModel
    {
        /// <summary>
        /// Creates a new empty RegisterModel.
        /// </summary>
        /// <returns></returns>
        public static RegisterModel CreateModel()
        {
            return new RegisterModel();
        }

        private RegisterModel()
        {
            MemberTypeAlias = Constants.Conventions.MemberTypes.DefaultAlias;
            UsernameIsEmail = true;
            MemberProperties = new List<UmbracoProperty>();
            LoginOnSuccess = true;
            CreatePersistentLoginCookie = true;
        }

        [Required]
        [RegularExpression(@"[A-Za-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[A-Za-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[A-Za-z0-9](?:[A-Za-z0-9-]*[A-Za-z0-9])?\.)+[A-Za-z0-9](?:[A-Za-z0-9-]*[A-Za-z0-9])?",
            ErrorMessage = "Please enter a valid e-mail address")]
        public string Email { get; set; }

        /// <summary>
        /// Returns the member properties
        /// </summary>
        public List<UmbracoProperty> MemberProperties { get; set; }

        /// <summary>
        /// The member type alias to use to register the member
        /// </summary>
        [Editable(false)]
        public string MemberTypeAlias { get; set; }

        /// <summary>
        /// The members real name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The members password
        /// </summary>
        [Required]
        public string Password { get; set; }

        /// <summary>
        /// The username of the model, if UsernameIsEmail is true then this is ignored.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Flag to determine if the username should be the email address, if true then the Username property is ignored
        /// </summary>
        public bool UsernameIsEmail { get; set; }

        /// <summary>
        /// Specifies if the member should be logged in if they are successfully created
        /// </summary>
        public bool LoginOnSuccess { get; set; }

        /// <summary>
        /// Default is true to create a persistent cookie if LoginOnSuccess is true
        /// </summary>
        public bool CreatePersistentLoginCookie { get; set; }
    }
}
