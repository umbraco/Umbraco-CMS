using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Umbraco.Cms.Web.Common.ActionsResults;

/// <summary>
///     Returns the Umbraco maintenance result.
/// </summary>
public class MaintenanceResult : IActionResult
{
    private readonly string _viewName;

    /// <summary>
    /// Initializes a new instance of the <see cref="MaintenanceResult"/> class using the default maintenance view.
    /// </summary>
    public MaintenanceResult()
        : this("~/umbraco/UmbracoWebsite/Maintenance.cshtml")
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MaintenanceResult"/> class using a custom view path.
    /// </summary>
    /// <param name="viewName">The view path to render, e.g. <c>~/umbraco/UmbracoWebsite/Upgrading.cshtml</c>.</param>
    public MaintenanceResult(string viewName)
        => _viewName = viewName;

    /// <inheritdoc />
    public async Task ExecuteResultAsync(ActionContext context)
    {
        HttpResponse response = context.HttpContext.Response;

        response.Clear();

        response.StatusCode = StatusCodes.Status503ServiceUnavailable;

        var viewResult = new ViewResult { ViewName = _viewName };

        await viewResult.ExecuteResultAsync(context);
    }
}
