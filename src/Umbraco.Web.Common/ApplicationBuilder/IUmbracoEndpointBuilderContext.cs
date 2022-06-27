using Microsoft.AspNetCore.Routing;

namespace Umbraco.Cms.Web.Common.ApplicationBuilder;

/// <summary>
///     A builder to allow encapsulating the enabled routing features in Umbraco
/// </summary>
public interface IUmbracoEndpointBuilderContext : IUmbracoApplicationBuilderServices
{
    IEndpointRouteBuilder EndpointRouteBuilder { get; }
}
