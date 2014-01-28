using System.Web;
using umbraco.cms.businesslogic.member;
using Umbraco.Core;

namespace Umbraco.Web.Models
{
    public class LoginStatusModel
    {
        public LoginStatusModel()
        {
            if (HttpContext.Current != null 
                && HttpContext.Current.User != null 
                && HttpContext.Current.User.Identity.IsAuthenticated)
            {
                var member = ApplicationContext.Current.Services.MemberService.GetByUsername(
                    HttpContext.Current.User.Identity.Name);
                if (member != null)
                {
                    this.Name = member.Name;
                    this.Username = member.Username;
                    this.Email = member.Email;
                    this.IsLoggedIn = true;
                }   
            }
        }

        public string Name { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public bool IsLoggedIn { get; set; }
    }
}