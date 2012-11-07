using Umbraco.Core.Models.Membership;
using Umbraco.Core.Services;

namespace Umbraco.Web.Services
{
    public static class UserServiceExtesions
    {
        public static IProfile GetCurrentBackOfficeUser(this IUserService userService)
        {
            return userService.GetCurrentBackOfficeUser(UmbracoContext.Current.HttpContext);
        }
    }
}