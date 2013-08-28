using System.Linq;
using System.Web.Mvc;
using System.Xml;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic.member;
using Umbraco.Web.Models;
using Umbraco.Web.Mvc;

namespace Umbraco.Web.Controllers
{
    public class RegisterController : SurfaceController
    {
        [HttpPost]
        public ActionResult HandleRegisterMember([Bind(Prefix="registerModel")]RegisterModel model)
        {
            //TODO: Use new Member API
            if (ModelState.IsValid)
            {
                var user = new User(0);

                var mt = MemberType.GetByAlias(model.MemberTypeAlias) ?? MemberType.MakeNew(user, model.MemberTypeAlias);

                var member = Member.MakeNew(model.Email, mt, user);
                member.Email = model.Email;
                member.LoginName = model.Email;
                member.Password = model.Password;

                foreach (var property in model.MemberProperties)
                {
                    member.getProperty(property.Alias).Value = property.Value;
                }

                member.Save();

                member.XmlGenerate(new XmlDocument());

                Member.AddMemberToCache(member);

                Response.Redirect("/");
            }

            return CurrentUmbracoPage();
        }
    }
}
