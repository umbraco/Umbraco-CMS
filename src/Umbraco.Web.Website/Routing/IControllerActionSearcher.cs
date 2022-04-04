using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace Umbraco.Cms.Web.Website.Routing
{
    public interface IControllerActionSearcher
    {
        ControllerActionDescriptor? Find<T>(HttpContext httpContext, string? controller, string? action);
    }
}
