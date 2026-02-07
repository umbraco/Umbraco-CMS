using Microsoft.AspNetCore.Builder;
using Umbraco.Cms.Web.Common.ApplicationBuilder;

namespace Umbraco.Extensions;

/// <summary>
/// <see cref="IApplicationBuilder" /> extensions for the Umbraco Delivery API.
/// </summary>
public static class DeliveryApiApplicationBuilderExtensions
{
    /// <summary>
    /// Sets up routes for the Umbraco Delivery API.
    /// </summary>
    /// <remarks>
    /// This method maps attribute-routed controllers including the Delivery API endpoints.
    /// Call this when using <c>AddDeliveryApi()</c> without <c>AddBackOffice()</c>, as the
    /// backoffice endpoints normally handle the controller mapping.
    /// </remarks>
    /// <param name="builder">The Umbraco endpoint builder context.</param>
    /// <returns>The <see cref="IUmbracoEndpointBuilderContext" /> for chaining.</returns>
    public static IUmbracoEndpointBuilderContext UseDeliveryApiEndpoints(this IUmbracoEndpointBuilderContext builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.EndpointRouteBuilder.MapControllers();

        return builder;
    }
}
