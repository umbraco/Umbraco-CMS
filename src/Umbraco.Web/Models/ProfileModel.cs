﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Umbraco.Web.Security;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Current = Umbraco.Web.Composing.Current;

namespace Umbraco.Web.Models
{
    /// <summary>
    /// A readonly member profile model
    /// </summary>
    [ModelBinder(typeof(ProfileModelBinder))]
    public class ProfileModel : PostRedirectModel
    {

        public static ProfileModel CreateModel()
        {
            var model = new ProfileModel(false);
            return model;
        }

        private ProfileModel(bool doLookup)
        {
            MemberProperties = new List<UmbracoProperty>();
            if (doLookup && Current.UmbracoContext != null)
            {
                var helper = Current.Factory.GetInstance<MembershipHelper>();
                var model = helper.GetCurrentMemberProfileModel();
                MemberProperties = model.MemberProperties;
            }
        }


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
        public string PasswordQuestion { get; set; }

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
        public List<UmbracoProperty> MemberProperties { get; set; }

        /// <summary>
        /// A custom model binder for MVC because the default ctor performs a lookup!
        /// </summary>
        internal class ProfileModelBinder : DefaultModelBinder
        {
            protected override object CreateModel(ControllerContext controllerContext, ModelBindingContext bindingContext, Type modelType)
            {
                return ProfileModel.CreateModel();
            }

        }
    }
}
