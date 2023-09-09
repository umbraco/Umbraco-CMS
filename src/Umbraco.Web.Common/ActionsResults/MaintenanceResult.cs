using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Web;

namespace Umbraco.Cms.Web.Common.ActionsResults;

/// <summary>
///     Returns the Umbraco maintenance result
/// </summary>
public class MaintenanceResult : IActionResult
{
    /// <inheritdoc />
    public async Task ExecuteResultAsync(ActionContext context)
    {
        HttpResponse response = context.HttpContext.Response;

        response.Clear();

        response.StatusCode = StatusCodes.Status503ServiceUnavailable;

        var viewResult = new ViewResult { ViewName = "~/umbraco/UmbracoWebsite/Maintenance.cshtml" };

        await viewResult.ExecuteResultAsync(context);
    }
}
