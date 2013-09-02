using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using umbraco.cms.businesslogic.member;

namespace Umbraco.Web.Models
{
    public class RegisterModel
    {
        public RegisterModel()
        {
            this.MemberTypeAlias = "UmbracoMember";

            var memberType = MemberType.GetByAlias(this.MemberTypeAlias);

            if (memberType != null)
            {
                this.MemberProperties = new List<UmbracoProperty>();

                var memberTypeProperties = memberType.PropertyTypes.ToList();

                if (memberTypeProperties.Where(memberType.MemberCanEdit).Any())
                {
                    memberTypeProperties = memberTypeProperties.Where(memberType.MemberCanEdit).ToList();
                }

                foreach (var prop in memberTypeProperties)
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

        [Required]
        public string Password { get; set; }

        public string Name { get; set; }

        public string MemberTypeAlias { get; set; }

        public List<UmbracoProperty> MemberProperties { get; set; }
    }
}
