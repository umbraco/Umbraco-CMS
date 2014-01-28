using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using System.Xml;
using umbraco.cms.businesslogic.member;
using Umbraco.Web.Models;
using Umbraco.Web.Mvc;
using Umbraco.Core.Security;

namespace Umbraco.Web.Controllers
{
    public class UmbProfileController : SurfaceController
    {
        [HttpPost]
        public ActionResult HandleUpdateProfile([Bind(Prefix = "profileModel")] ProfileModel model)
        {
            if (Membership.Provider.IsUmbracoMembershipProvider() == false)
            {
                throw new NotSupportedException("Profile editing with the " + typeof(UmbProfileController) + " is not supported when not using the default Umbraco membership provider");
            }

            if (ModelState.IsValid == false)
            {
                return CurrentUmbracoPage();
            }

            if (HttpContext.User == null || HttpContext.User.Identity.IsAuthenticated == false)
            {
                throw new NotSupportedException("No member is currently logged in");
            }

            var member = Services.MemberService.GetByUsername(HttpContext.User.Identity.Name);
            if (member == null)
            {
                //this should never happen
                throw new InvalidOperationException("No member found with username: " + HttpContext.User.Identity.Name);
            }

            if (model.Name != null)
            {
                member.Name = model.Name;
            }
            member.Email = model.Email;
            member.Username = model.Email;
            if (model.MemberProperties != null)
            {
                //TODO: Shouldn't we ensure that none of these properties are flagged as non-editable to the member??
                foreach (var property in model.MemberProperties.Where(p => p.Value != null))
                {
                    if (member.Properties.Contains(property.Alias))
                    {
                        member.Properties[property.Alias].Value = property.Value;
                    }
                }
            }
            Services.MemberService.Save(member);
            //reset the FormsAuth cookie since the username might have changed
            FormsAuthentication.SetAuthCookie(member.Username, true);

            //TODO: Why are we redirecting to home again here?? 
            return Redirect("/");

        }
    }
}
