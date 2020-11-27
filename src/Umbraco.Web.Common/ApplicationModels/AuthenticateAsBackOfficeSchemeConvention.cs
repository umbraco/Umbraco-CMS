using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Umbraco.Web.Common.Filters;

namespace Umbraco.Web.Common.ApplicationModels
{
    /// <summary>
    /// Ensures all requests with this convention are authenticated with the back office scheme
    /// </summary>
    public class AuthenticateAsBackOfficeSchemeConvention : IActionModelConvention
    {
        public void Apply(ActionModel action)
        {
            action.Filters.Add(new EnsureUmbracoBackOfficeAuthentication());
        }
    }
}
