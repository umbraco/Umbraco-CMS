using System;
using System.Web;
using Umbraco.Core;
using Umbraco.Web.Security;

namespace Umbraco.Web.Models
{
    /// <summary>
    /// The model 
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
                var model = helper.GetLoginStatusModel();
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
        [Obsolete("Do not use this ctor as it will perform business logic lookups. Use the MembershipHelper.GetLoginStatusModel or the static LoginStatusModel.CreateModel() to create an empty model.")]
        public LoginStatusModel()
            : this(true)
        {
            
        }

        public string Name { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public bool IsLoggedIn { get; set; }
    }
}