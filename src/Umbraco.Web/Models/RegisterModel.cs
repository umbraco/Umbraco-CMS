using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using umbraco.cms.businesslogic.member;
using Umbraco.Core;
using Umbraco.Web.Security;

namespace Umbraco.Web.Models
{
    [ModelBinder(typeof(RegisterModelBinder))]
    public class RegisterModel : PostRedirectModel
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
            MemberTypeAlias = Constants.Conventions.MemberTypes.DefaultAlias;
            RedirectOnSucces = false;
            UsernameIsEmail = true;
            MemberProperties = new List<UmbracoProperty>();
            LoginOnSuccess = true;
            CreatePersistentLoginCookie = true;
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
        
        [ReadOnly(true)]
        [Obsolete("This is no longer used and will be removed from the codebase in future versions")]
        public bool RedirectOnSucces { get; set; }
 
        /// <summary>
        /// The username of the model, if UsernameIsEmail is true then this is ignored.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Flag to determine if the username should be the email address, if true then the Username property is ignored
        /// </summary>
        public bool UsernameIsEmail { get; set; }
        
        /// <summary>
        /// Specifies if the member should be logged in if they are succesfully created
        /// </summary>
        public bool LoginOnSuccess { get; set; }

        /// <summary>
        /// Default is true to create a persistent cookie if LoginOnSuccess is true
        /// </summary>
        public bool CreatePersistentLoginCookie { get; set; }

        /// <summary>
        /// A custom model binder for MVC because the default ctor performs a lookup!
        /// </summary>
        internal class RegisterModelBinder : DefaultModelBinder
        {
            protected override object CreateModel(ControllerContext controllerContext, ModelBindingContext bindingContext, Type modelType)
            {
                return RegisterModel.CreateModel();
            }
        }

    }
}
