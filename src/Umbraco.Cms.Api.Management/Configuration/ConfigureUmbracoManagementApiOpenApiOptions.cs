using Microsoft.AspNetCore.OpenApi;
using Umbraco.Cms.Api.Common.Configuration;
using Umbraco.Cms.Api.Common.OpenApi;
using Umbraco.Cms.Api.Management.DependencyInjection;
using Umbraco.Cms.Api.Management.Extensions;
using Umbraco.Cms.Api.Management.OpenApi;

namespace Umbraco.Cms.Api.Management.Configuration;

/// <summary>
/// Configures the OpenAPI options for the Umbraco Management API.
/// </summary>
public class ConfigureUmbracoManagementApiOpenApiOptions : ConfigureUmbracoOpenApiOptionsBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigureUmbracoManagementApiOpenApiOptions"/> class.
    /// </summary>
    /// <param name="schemaIdSelector">The schema ID selector.</param>
    public ConfigureUmbracoManagementApiOpenApiOptions(ISchemaIdSelector schemaIdSelector)
        : base(schemaIdSelector)
    {
    }

    /// <inheritdoc />
    protected override string ApiName => ManagementApiConfiguration.ApiName;

    /// <inheritdoc />
    protected override string ApiTitle => ManagementApiConfiguration.ApiTitle;

    /// <inheritdoc />
    protected override string ApiVersion => "Latest";

    /// <inheritdoc />
    protected override string ApiDescription =>
        "This shows all APIs available in this version of Umbraco - including all the legacy apis that are available for backward compatibility";

    /// <inheritdoc />
    protected override void ConfigureOpenApi(OpenApiOptions options)
    {
        base.ConfigureOpenApi(options);

        // Sets Security requirement on backoffice apis
        options.AddBackofficeSecurityRequirements();

        options.AddOperationTransformer<ResponseHeaderTransformer>();
        options.AddOperationTransformer<NotificationHeaderTransformer>();
    }
}
