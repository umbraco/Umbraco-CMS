using System.Web;
using Umbraco.Cms.Core.Runtime;

namespace Umbraco.Web.Runtime
{
    public class AspNetUmbracoBootPermissionChecker : IUmbracoBootPermissionChecker
    {
        public void ThrowIfNotPermissions()
        {
            new AspNetHostingPermission(AspNetHostingPermissionLevel.Unrestricted).Demand();
        }
    }
}
