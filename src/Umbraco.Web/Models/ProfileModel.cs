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
    public class ProfileModel
    {
        public static ProfileModel CreateModel()
        {
            var model = new ProfileModel(false);
            return model;
        }

        private ProfileModel(bool doLookup)
        {
            MemberProperties = new List<UmbracoProperty>();
            if (doLookup)
            {
                var helper = new MembershipHelper(ApplicationContext.Current, new HttpContextWrapper(HttpContext.Current));
                var model = helper.CreateProfileModel();
                MemberProperties = model.MemberProperties;
            }   
        }

        [Obsolete("Do not use this ctor as it will perform business logic lookups. Use the MembershipHelper.CreateProfileModel or the static ProfileModel.CreateModel() to create an empty model.")]
        public ProfileModel()
            :this(true)
        {
        }

        [Required]
        [RegularExpression(@"[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?",
            ErrorMessage = "Please enter a valid e-mail address")]
        public string Email { get; set; }

        public string Name { get; set; }

        public string MemberTypeAlias { get; set; }

        public List<UmbracoProperty> MemberProperties { get; set; }
    }
}
