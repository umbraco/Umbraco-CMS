using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Umbraco.Cms.Web.Common.ActionsResults;

/// <summary>
///     Returns the Umbraco maintenance result.
/// </summary>
public class MaintenanceResult : IActionResult
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MaintenanceResult"/> class using the default maintenance view.
    /// </summary>
    /// <remarks>
    /// This is the built-in view, not the one configured for the site. To honour the site's configuration, pass
    /// <c>GlobalSettings.MaintenanceViewPath</c> to the overload taking a view path.
    /// </remarks>
    public MaintenanceResult()
        : this("~/umbraco/UmbracoWebsite/Maintenance.cshtml")
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MaintenanceResult"/> class using a custom view path.
    /// </summary>
    /// <param name="viewName">The view path to render, e.g. <c>~/umbraco/UmbracoWebsite/Maintenance.cshtml</c>.</param>
    public MaintenanceResult(string viewName)
        => ViewName = viewName;

    /// <summary>
    /// Gets the path of the view that will be rendered.
    /// </summary>
    /// <remarks>
    /// Exposed so the chosen view can be inspected without executing the result. Tests rely on this to assert
    /// which view the maintenance mode filter selects for a given runtime level.
    /// </remarks>
    public string ViewName { get; }

    /// <inheritdoc />
    public async Task ExecuteResultAsync(ActionContext context)
    {
        HttpResponse response = context.HttpContext.Response;

        response.Clear();

        response.StatusCode = StatusCodes.Status503ServiceUnavailable;

        var viewResult = new ViewResult { ViewName = ViewName };

        await viewResult.ExecuteResultAsync(context);
    }
}
