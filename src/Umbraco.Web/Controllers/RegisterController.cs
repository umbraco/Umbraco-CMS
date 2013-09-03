using System.Linq;
using System.Web.Mvc;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic.member;
using Umbraco.Web.Models;
using Umbraco.Web.Mvc;

namespace Umbraco.Web.Controllers
{
    public class RegisterController : SurfaceController
    {
        [HttpPost]
        public ActionResult HandleRegisterMember([Bind(Prefix = "registerModel")]RegisterModel model)
        {
            //TODO: Use new Member API and use the MemberShipProvider variables for validating password strength etc
            if (ModelState.IsValid)
            {
                model.Username = (model.UsernameIsEmail || model.Username == null) ? model.Email : model.Username;

                var member = Member.GetMemberFromLoginName(model.Username);
                if (member != null)
                {
                    ModelState.AddModelError((model.UsernameIsEmail || model.Username == null) 
                                                ? "registerModel.Email" 
                                                : "registerModel.Username",
                                                "A member with this username already exists.");

                    return CurrentUmbracoPage();
                }

                member = Member.GetMemberFromEmail(model.Email);
                if (member != null)
                {
                    ModelState.AddModelError("registerModel.Email", "A member with this e-mail address already exists.");

                    return CurrentUmbracoPage();
                }

                member = CreateNewMember(model);

                // Log member in
                Member.AddMemberToCache(member);

                if (model.RedirectOnSucces)
                {
                    return Redirect(model.RedirectUrl);
                }

                TempData.Add("FormSuccess", true);
                return RedirectToCurrentUmbracoPage();
            }

            return CurrentUmbracoPage();
        }

        private static Member CreateNewMember(RegisterModel model)
        {
            var user = new User(0);

            var mt = MemberType.GetByAlias(model.MemberTypeAlias) ?? MemberType.MakeNew(user, model.MemberTypeAlias);

            var member = Member.MakeNew(model.Username, mt, user);

            if (model.Name != null)
            {
                member.Text = model.Name;
            }

            member.Email = model.Email;
            member.Password = model.Password;

            if (model.MemberProperties != null)
            {
                foreach (var property in model.MemberProperties.Where(p => p.Value != null))
                {
                    member.getProperty(property.Alias).Value = property.Value;
                }
            }

            member.Save();
            return member;
        }
    }
}
