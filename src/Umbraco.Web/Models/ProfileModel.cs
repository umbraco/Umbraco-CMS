using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using umbraco.cms.businesslogic.member;

namespace Umbraco.Web.Models
{
    public class ProfileModel
    {
        public ProfileModel()
        {
            if (Member.IsLoggedOn())
            {
                //TODO Use new Member API
                var member = Member.GetCurrentMember();

                if (member != null)
                {
                    this.Name = member.Text;

                    this.Email = member.Email;

                    this.MemberProperties = new List<UmbracoProperty>();

                    var memberType = MemberType.GetByAlias(member.ContentType.Alias);

                    foreach (var prop in memberType.PropertyTypes.Where(memberType.MemberCanEdit))
                    {
                        var value = string.Empty;
                        var propValue = member.getProperty(prop.Alias);
                        if (propValue != null)
                        {
                            value = propValue.Value.ToString();
                        }

                        this.MemberProperties.Add(new UmbracoProperty
                                                  {
                                                      Alias = prop.Alias,
                                                      Name = prop.Name,
                                                      Value = value
                                                  });
                    }
                }
            }
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
