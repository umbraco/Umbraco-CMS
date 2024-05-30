using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Web.Common.Controllers;

/// <summary>
///     Provides a base class for auto-routed Umbraco API controllers.
/// </summary>
[Obsolete("""
WARNING
The UmbracoAPIController does not work exactly as in previous versions of Umbraco because serialization is now done using System.Text.Json.
Please verify your API responses still work as expect.

We recommend using regular ASP.NET Core ApiControllers for your APIs so that OpenAPI specifications are generated.
Read more about this here: https://learn.microsoft.com/en-us/aspnet/core/web-api/

UmbracoAPIController will be removed in Umbraco 15.
""")]
public abstract class UmbracoApiController : UmbracoApiControllerBase, IDiscoverable
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="UmbracoApiController" /> class.
    /// </summary>
    protected UmbracoApiController()
    {
    }
}
