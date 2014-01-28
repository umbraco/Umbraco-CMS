using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using umbraco.cms.businesslogic.member;
using Umbraco.Core;
using Umbraco.Web.Security;

namespace Umbraco.Web.Models
{
    public class RegisterModel
    {
        /// <summary>
        /// Creates a new empty RegisterModel
        /// </summary>
        /// <returns></returns>
        public static RegisterModel CreateModel()
        {
            var model = new RegisterModel(false);
            return model;
        }

        private RegisterModel(bool doLookup)
        {
            MemberTypeAlias = Constants.Conventions.MemberTypes.Member;
            RedirectOnSucces = false;
            RedirectUrl = "/";
            UsernameIsEmail = true;

            if (doLookup && HttpContext.Current != null && ApplicationContext.Current != null)
            {
                var helper = new MembershipHelper(ApplicationContext.Current, new HttpContextWrapper(HttpContext.Current));
                var model = helper.CreateRegistrationModel(MemberTypeAlias);
                MemberProperties = model.MemberProperties;
            }
        }

        [Obsolete("Do not use this ctor as it will perform business logic lookups. Use the MembershipHelper.CreateRegistrationModel or the static RegisterModel.CreateModel() to create an empty model.")]
        public RegisterModel()
            : this(true)
        {   
        }

        [Required]
        [RegularExpression(@"[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?",
            ErrorMessage = "Please enter a valid e-mail address")]
        public string Email { get; set; }

        /// <summary>
        /// Returns the member properties
        /// </summary>
        public List<UmbracoProperty> MemberProperties { get; set; }
        
        public string MemberTypeAlias { get; set; }

        public string Name { get; set; }

        [Required]
        public string Password { get; set; }
        
        public bool RedirectOnSucces { get; set; }
        
        public string RedirectUrl { get; set; }

        public string Username { get; set; }

        public bool UsernameIsEmail { get; set; }
    }
}
