using System.Linq;
using System.Web.Mvc;
using System.Xml;
using umbraco.cms.businesslogic.member;
using Umbraco.Web.Models;
using Umbraco.Web.Mvc;

namespace Umbraco.Web.Controllers
{
    public class ProfileController : SurfaceController
    {
        [HttpPost]
        public ActionResult HandleUpdateProfile([Bind(Prefix="profileModel")]ProfileModel model)
        {
            //TODO: Use new Member API
            if (ModelState.IsValid)
            {
                var member = Member.GetCurrentMember();
                if (member != null)
                {
                    if (model.Name != null)
                    {
                        member.Text = model.Name;
                    }

                    member.Email = model.Email;
                    member.LoginName = model.Email;

                    if (model.MemberProperties != null)
                    {
                        foreach (var property in model.MemberProperties.Where(p => p.Value != null))
                        {
                            member.getProperty(property.Alias).Value = property.Value;
                        }
                    }

                    member.Save();

                    member.XmlGenerate(new XmlDocument());

                    Member.AddMemberToCache(member);

                    Response.Redirect("/");
                }
            }

            return CurrentUmbracoPage();
        }
    }
}
