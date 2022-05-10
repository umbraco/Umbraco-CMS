using System.Net;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;

namespace Umbraco.Cms.Web.Common.Filters;

/// <summary>
///     Forces the response to have a specific http status code
/// </summary>
public class StatusCodeResultAttribute : ActionFilterAttribute
{
    private readonly HttpStatusCode _statusCode;

    public StatusCodeResultAttribute(HttpStatusCode statusCode) => _statusCode = statusCode;

    public override void OnActionExecuted(ActionExecutedContext context)
    {
        base.OnActionExecuted(context);

        HttpContext httpContext = context.HttpContext;

        httpContext.Response.StatusCode = (int)_statusCode;

        var disableIisCustomErrors = httpContext.RequestServices.GetService<IOptions<WebRoutingSettings>>()?.Value
            .TrySkipIisCustomErrors ?? false;
        IStatusCodePagesFeature? statusCodePagesFeature = httpContext.Features.Get<IStatusCodePagesFeature>();

        if (statusCodePagesFeature != null)
        {
            // if IIS Custom Errors are disabled, we won't enable the Status Code Pages
            statusCodePagesFeature.Enabled = !disableIisCustomErrors;
        }
    }
}
