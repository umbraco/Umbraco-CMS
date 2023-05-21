using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Web.Common.ApplicationBuilder;

/// <summary>
///     A builder to allow encapsulating the enabled endpoints in Umbraco
/// </summary>
internal class UmbracoEndpointBuilder : IUmbracoEndpointBuilderContext
{
    public UmbracoEndpointBuilder(IServiceProvider services, IRuntimeState runtimeState, IApplicationBuilder appBuilder, IEndpointRouteBuilder endpointRouteBuilder)
    {
        ApplicationServices = services;
        EndpointRouteBuilder = endpointRouteBuilder;
        RuntimeState = runtimeState;
        AppBuilder = appBuilder;
    }

    public IServiceProvider ApplicationServices { get; }

    public IEndpointRouteBuilder EndpointRouteBuilder { get; }

    public IRuntimeState RuntimeState { get; }

    public IApplicationBuilder AppBuilder { get; }
}
