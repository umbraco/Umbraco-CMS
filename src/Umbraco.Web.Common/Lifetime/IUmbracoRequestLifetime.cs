using System;
using Microsoft.AspNetCore.Http;

namespace Umbraco.Web.Common.Lifetime
{
    // TODO: Should be killed and replaced with IEventAggregator
    public interface IUmbracoRequestLifetime
    {        
        event EventHandler<HttpContext> RequestStart;
        event EventHandler<HttpContext> RequestEnd;
    }
}
