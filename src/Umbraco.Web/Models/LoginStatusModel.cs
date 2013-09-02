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
                var member = Member.GetCurrentMember();
                if (member != null)
                {
                    this.Name = member.Text;
                    this.Username = member.LoginName;
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