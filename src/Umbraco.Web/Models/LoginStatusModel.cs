using System;
using System.ComponentModel.DataAnnotations;
using System.Web;
using Umbraco.Core;
using Umbraco.Web.Security;

namespace Umbraco.Web.Models
{
    /// <summary>
    /// The model representing the status of a logged in member 
    /// </summary>
    public class LoginStatusModel
    {
        /// <summary>
        /// Creates a new empty LoginStatusModel
        /// </summary>
        /// <returns></returns>
        public static LoginStatusModel CreateModel()
        {
            var model = new LoginStatusModel(false);
            return model;
        }

        private LoginStatusModel(bool doLookup)
        {
            if (doLookup && HttpContext.Current != null && ApplicationContext.Current != null)
            {
                var helper = new MembershipHelper(ApplicationContext.Current, new HttpContextWrapper(HttpContext.Current));
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
        /// This will construct a new LoginStatusModel and perform a lookup for hte curently logged in member
        /// </summary>
        [Obsolete("Do not use this ctor as it will perform business logic lookups. Use the MembershipHelper.GetCurrentLoginStatus or the static LoginStatusModel.CreateModel() to create an empty model.")]
        public LoginStatusModel()
            : this(true)
        {
            
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