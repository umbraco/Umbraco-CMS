using System;
using Microsoft.AspNetCore.Http;

namespace Umbraco.Web.Common.Lifetime
{
    public interface IUmbracoRequestLifetime
    {
        event EventHandler<HttpContext> RequestStart;
        event EventHandler<HttpContext> RequestEnd;
    }
}
