using Umbraco.Core.Models.Security;

namespace Umbraco.Core.Security
{
    public interface IUmbracoWebsiteSecurity
    {
        // TODO: this should return the member, but in what form?  MembershipUser is in place on MembershipHelper, but
        // isn't appropriate for when we're using ASP.NET Identity.
        void RegisterMember(RegisterModel model, out RegisterMemberStatus status, bool logMemberIn = true);

        // TODO: again, should this return the member?
        void UpdateMemberProfile(ProfileModel model, out UpdateMemberProfileStatus status, out string errorMessage);

        bool Login(string username, string password);

        bool IsLoggedIn();

        void LogOut();
    }
}
