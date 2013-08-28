using umbraco.cms.businesslogic.member;

namespace Umbraco.Web.Models
{
    public class LoginStatusModel
    {
        public LoginStatusModel()
        {
            //TODO Use new Member API
            if (Member.IsLoggedOn())
            {
                this.Name = Member.GetCurrentMember().Text;
                this.Username = Member.GetCurrentMember().LoginName;
                this.Email = Member.GetCurrentMember().Email;
                this.IsLoggedIn = true;
            }
        }

        public string Name { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public bool IsLoggedIn { get; set; }
    }
}