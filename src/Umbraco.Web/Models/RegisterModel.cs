﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web;
using System.Web.Mvc;
using Umbraco.Core;
using Umbraco.Web.Security;
using Current = Umbraco.Web.Composing.Current;

namespace Umbraco.Web.Models
{
    [ModelBinder(typeof(RegisterModelBinder))]
    public class RegisterModel : PostRedirectModel
    {
        /// <summary>
        /// Creates a new empty RegisterModel.
        /// </summary>
        /// <returns></returns>
        public static RegisterModel CreateModel()
        {
            return new RegisterModel(false);
        }

        private RegisterModel(bool doLookup)
        {
            MemberTypeAlias = Constants.Conventions.MemberTypes.DefaultAlias;
            UsernameIsEmail = true;
            MemberProperties = new List<UmbracoProperty>();
            LoginOnSuccess = true;
            CreatePersistentLoginCookie = true;
            if (doLookup && Current.UmbracoContext != null)
            {
                var helper = Current.Factory.GetInstance<MembershipHelper>();
                var model = helper.CreateRegistrationModel(MemberTypeAlias);
                MemberProperties = model.MemberProperties;
            }
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
