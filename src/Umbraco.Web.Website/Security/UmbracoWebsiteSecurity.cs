using Umbraco.Core.Models.Security;
using Umbraco.Core.Security;

namespace Umbraco.Web.Website.Security
{
    public class UmbracoWebsiteSecurity : IUmbracoWebsiteSecurity
    {
        public void RegisterMember(RegisterModel model, out RegisterMemberStatus status, bool logMemberIn = true)
        {
            throw new System.NotImplementedException();
        }

        public void UpdateMemberProfile(ProfileModel model, out UpdateMemberProfileStatus status, out string errorMessage)
        {
            throw new System.NotImplementedException();
        }

        public bool IsLoggedIn()
        {
            throw new System.NotImplementedException();
        }

        public bool Login(string username, string password)
        {
            throw new System.NotImplementedException();
        }

        public void LogOut()
        {
            throw new System.NotImplementedException();
        }
    }
}
