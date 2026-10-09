// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.Routing;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Web.Website.Controllers;

/// <summary>
///     Shared test infrastructure for exercising SurfaceController subclasses.
/// </summary>
internal static class SurfaceControllerTestHelpers
{
    // Shared across tests: the registrations are static (logging only) and IServiceProvider is safe for concurrent
    // resolution, so reusing a single instance avoids accumulating disposable provider state across large test runs.
    private static readonly Lazy<ServiceProvider> SharedServiceProvider = new(() =>
    {
        var services = new ServiceCollection();
        services.AddLogging();
        return services.BuildServiceProvider();
    });

    /// <summary>
    ///     Configures the <see cref="ControllerBase.ControllerContext" />, URL helper, TempData provider and the
    ///     <see cref="UmbracoRouteValues" /> feature required for a Surface controller under test.
    /// </summary>
    public static void ConfigureControllerContext(
        Controller controller,
        bool isAuthenticated = false,
        IPublishedContent? currentPage = null,
        string? userIdClaim = null)
    {
        ClaimsPrincipal user;
        if (isAuthenticated)
        {
            var claims = new List<Claim>();
            if (userIdClaim is not null)
            {
                claims.Add(new Claim(ClaimTypes.NameIdentifier, userIdClaim));
                claims.Add(new Claim(ClaimTypes.Name, userIdClaim));
            }

            user = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
        }
        else
        {
            user = new ClaimsPrincipal(new ClaimsIdentity());
        }

        var httpContext = new DefaultHttpContext
        {
            RequestServices = SharedServiceProvider.Value,
            User = user,
        };

        // Provide a routed request feature so CurrentPage/RedirectToCurrentUmbracoPage can resolve.
        var publishedRequestBuilder = new PublishedRequestBuilder(new Uri("http://localhost/"), Mock.Of<IFileService>());
        if (currentPage is not null)
        {
            publishedRequestBuilder.SetPublishedContent(currentPage);
        }

        var routeValues = new UmbracoRouteValues(publishedRequestBuilder.Build(), null);
        httpContext.Features.Set(routeValues);

        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());

        controller.ControllerContext = new ControllerContext { HttpContext = httpContext, RouteData = actionContext.RouteData };
        controller.Url = new UrlHelper(actionContext);
        controller.TempData = new TempDataDictionary(httpContext, new NullTempDataProvider());
    }

    private sealed class NullTempDataProvider : ITempDataProvider
    {
        public IDictionary<string, object?> LoadTempData(HttpContext context) => new Dictionary<string, object?>();

        public void SaveTempData(HttpContext context, IDictionary<string, object?> values)
        {
            // No-op for unit tests.
        }
    }
}
