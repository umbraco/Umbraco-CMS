using System.ComponentModel.DataAnnotations;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Web.Security;
using Current = Umbraco.Web.Composing.Current;

namespace Umbraco.Web.Models
{
    /// <summary>
    /// The model representing the status of a logged in member.
    /// </summary>
    public class LoginStatusModel
    {
        /// <summary>
        /// Creates a new empty LoginStatusModel.
        /// </summary>
        /// <returns></returns>
        public static LoginStatusModel CreateModel()
        {
            return new LoginStatusModel(false);
        }

        private LoginStatusModel(bool doLookup)
        {
            if (doLookup && Current.UmbracoContext != null)
            {
                var helper = Current.Factory.GetInstance<MembershipHelper>();
                var model = helper.GetCurrentLoginStatus();
                if (model != null)
                {
                    Name = model.Name;
                    Username = model.Username;
                    Email = model.Email;
                    IsLoggedIn = true;
                }
            }
        }


        /// <summary>
        /// The name of the member
        /// </summary>
        [Required]
        public string Name { get; set; }

        /// <summary>
        /// The username of the member
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// The email of the member
        /// </summary>
        [Required]
        public string Email { get; set; }

        /// <summary>
        /// True, if the member is currently logged in
        /// </summary>
        public bool IsLoggedIn { get; set; }
    }
}
