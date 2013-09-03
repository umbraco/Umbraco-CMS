using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using umbraco.cms.businesslogic.member;
using Umbraco.Core;

namespace Umbraco.Web.Models
{
    public class RegisterModel
    {
        public RegisterModel()
        {
            this.MemberTypeAlias = Constants.Conventions.MemberTypes.Member;

            this.RedirectOnSucces = false;

            this.RedirectUrl = "/";

            this.UsernameIsEmail = true;

            var memberType = MemberType.GetByAlias(this.MemberTypeAlias);

            if (memberType != null)
            {
                this.MemberProperties = new List<UmbracoProperty>();

                foreach (var prop in memberType.PropertyTypes.Where(memberType.MemberCanEdit))
                {
                    this.MemberProperties.Add(new UmbracoProperty
                                              {
                                                  Alias = prop.Alias,
                                                  Name = prop.Name,
                                                  Value = string.Empty
                                              });
                }
            }
        }

        [Required]
        [RegularExpression(@"[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?",
            ErrorMessage = "Please enter a valid e-mail address")]
        public string Email { get; set; }

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
