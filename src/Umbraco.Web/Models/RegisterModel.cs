using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using umbraco.cms.businesslogic.member;
using Umbraco.Core;

namespace Umbraco.Web.Models
{
    public class RegisterModel
    {
        [Required]
        [RegularExpression(@"[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?",
            ErrorMessage = "Please enter a valid e-mail address")]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        public string MemberTypeAlias { get; set; }

        public List<UmbracoProperty> MemberProperties { get; set; }

        public void FillModel(RegisterModel registerModel, IDictionary<string, object> macroParameters)
        {
            registerModel.MemberTypeAlias = macroParameters.GetValueAsString("memberTypeAlias", "UmbracoMember");

            registerModel.MemberProperties = new List<UmbracoProperty>();

            var memberType = MemberType.GetByAlias(registerModel.MemberTypeAlias);

            var memberTypeProperties = memberType.PropertyTypes.ToList();

            if (memberTypeProperties.Where(memberType.MemberCanEdit).Any())
            {
                memberTypeProperties = memberTypeProperties.Where(memberType.MemberCanEdit).ToList();
            }

            foreach (var prop in memberTypeProperties)
            {
                registerModel.MemberProperties.Add(new UmbracoProperty { Alias = prop.Alias, Name = prop.Name, Value = string.Empty });
            }
        }
    }

    public class UmbracoProperty
    {
        public string Alias { get; set; }
        public string Value { get; set; }
        public string Name { get; set; }
    }
}
