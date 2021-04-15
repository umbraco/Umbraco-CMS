using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Web.Common.ApplicationBuilder
{
    /// <summary>
    /// A builder to allow encapsulating the enabled routing features in Umbraco
    /// </summary>
    public interface IUmbracoApplicationBuilder
    {
        IRuntimeState RuntimeState { get; }
        IServiceProvider ApplicationServices { get; }
        IEndpointRouteBuilder EndpointRouteBuilder { get; }
        IApplicationBuilder AppBuilder { get; }
    }
}
