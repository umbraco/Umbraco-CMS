using Microsoft.AspNetCore.Http;

namespace Umbraco.Cms.Web.Common.Routing;

public class UmbracoRequestOptions
{
    /// <summary>
    ///     Gets the delegate that checks if we're gonna handle a request as a client-side request
    ///     this returns true by default and can be overwritten in Startup.cs
    /// </summary>
    public Func<HttpRequest, bool> HandleAsServerSideRequest { get; set; } = x => false;
}
