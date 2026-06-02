using Microsoft.AspNetCore.OpenApi;
using Umbraco.Cms.Api.Management.OpenApi.Transformers;

namespace Umbraco.Cms.Api.Management.OpenApi;

/// <summary>
/// Extension methods for configuring OpenAPI options for the Management API.
/// </summary>
public static class OpenApiOptionsExtensions
{
    /// <summary>
    /// Adds security requirements for backoffice APIs to the OpenAPI options.
    /// </summary>
    /// <param name="options">The <see cref="OpenApiOptions"/> instance to configure.</param>
    /// <returns>The configured <see cref="OpenApiOptions"/> instance.</returns>
    public static OpenApiOptions AddBackofficeSecurityRequirements(this OpenApiOptions options)
        => options
            .AddOperationTransformer<BackOfficeSecurityRequirementsTransformer>()
            .AddDocumentTransformer<BackOfficeSecurityRequirementsTransformer>();
}
