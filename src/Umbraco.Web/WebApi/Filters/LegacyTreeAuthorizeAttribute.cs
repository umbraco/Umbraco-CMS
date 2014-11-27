using System.Web.Http;
using System.Web.Http.Controllers;
using Umbraco.Core;

namespace Umbraco.Web.WebApi.Filters
{
    internal class LegacyTreeAuthorizeAttribute : AuthorizeAttribute
    {
        protected override bool IsAuthorized(HttpActionContext actionContext)
        {
            var httpContext = actionContext.Request.TryGetHttpContext();
            if (httpContext)
            {
                var treeRequest = httpContext.Result.Request.QueryString["treeType"];
                if (treeRequest.IsNullOrWhiteSpace()) return false;

                var tree = ApplicationContext.Current.Services.ApplicationTreeService.GetByAlias(treeRequest);
                if (tree == null) return false;

                return UmbracoContext.Current.Security.CurrentUser != null
                       && UmbracoContext.Current.Security.UserHasAppAccess(tree.ApplicationAlias, UmbracoContext.Current.Security.CurrentUser);
            }
            return false;

            
        }
    }
}