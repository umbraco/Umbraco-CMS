using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Delivery.Configuration;
using Umbraco.Cms.Api.Delivery.OpenApi.Transformers;

namespace Umbraco.Cms.Api.Delivery.OpenApi;

/// <summary>
/// Extension methods for configuring OpenAPI support for the Delivery API.
/// </summary>
public static class OpenApiServiceCollectionExtensions
{
    /// <summary>
    /// Adds member authentication support to the Delivery API OpenAPI document.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to configure.</param>
    /// <returns>The configured <see cref="IServiceCollection"/> instance.</returns>
    /// <remarks>
    /// This enables the OAuth2 authorization code flow for member authentication in Swagger UI.
    /// Consult the Delivery API member authentication documentation for setup instructions.
    /// </remarks>
    public static IServiceCollection AddDeliveryApiOpenApiMemberAuthentication(this IServiceCollection services)
    {
        services.PostConfigure<OpenApiOptions>(
            DeliveryApiConfiguration.ApiName,
            options =>
            {
                options.AddDocumentTransformer<MemberAuthenticationSecurityRequirementsTransformer>();
                options.AddOperationTransformer<MemberAuthenticationSecurityRequirementsTransformer>();
            });

        return services;
    }
}
