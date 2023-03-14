using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;

namespace Umbraco.Cms.Web.BackOffice.Filters;

/// <summary>
///     If Umbraco.Core.UseHttps property in web.config is set to true, this filter will redirect any http access to https.
/// </summary>
public class UmbracoRequireHttpsAttribute : RequireHttpsAttribute
{
    protected override void HandleNonHttpsRequest(AuthorizationFilterContext filterContext)
    {
        // just like the base class does, we'll just resolve the required services from the httpcontext.
        // we want to re-use their code so we don't have much choice, else we have to do some code tricks,
        // this is just easiest.
        IOptionsSnapshot<GlobalSettings> optionsAccessor = filterContext.HttpContext.RequestServices
            .GetRequiredService<IOptionsSnapshot<GlobalSettings>>();
        if (optionsAccessor.Value.UseHttps)
        {
            // only continue if this flag is set
            base.HandleNonHttpsRequest(filterContext);
        }
    }
}
