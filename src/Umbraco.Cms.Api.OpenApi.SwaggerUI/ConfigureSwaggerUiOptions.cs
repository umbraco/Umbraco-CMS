using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerUI;
using Umbraco.Cms.Api.Common.OpenApi;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Api.OpenApi.SwaggerUI;

/// <summary>
/// Configures Swagger UI from the UI-independent Umbraco OpenAPI options.
/// </summary>
internal sealed class ConfigureSwaggerUiOptions : IConfigureOptions<SwaggerUIOptions>
{
    private readonly IOptions<UmbracoOpenApiOptions> _openApiOptions;
    private readonly IOptions<UmbracoOpenApiUiOptions> _uiOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigureSwaggerUiOptions"/> class.
    /// </summary>
    /// <param name="openApiOptions">The Umbraco OpenAPI options.</param>
    /// <param name="uiOptions">The UI-independent options describing what the UI presents.</param>
    public ConfigureSwaggerUiOptions(IOptions<UmbracoOpenApiOptions> openApiOptions, IOptions<UmbracoOpenApiUiOptions> uiOptions)
    {
        _openApiOptions = openApiOptions;
        _uiOptions = uiOptions;
    }

    /// <inheritdoc />
    public void Configure(SwaggerUIOptions options)
    {
        UmbracoOpenApiUiOptions uiOptions = _uiOptions.Value;

        options.RoutePrefix = _openApiOptions.Value.UiRoutePrefix;

        // See https://swagger.io/docs/open-source-tools/swagger-ui/usage/configuration/
        options.ConfigObject.PersistAuthorization = true;
        options.ConfigObject.Filter = string.Empty;

        options.OAuthClientId(Constants.OAuthClientIds.OpenApiUi);
        options.OAuthUsePkce();

        foreach (OpenApiUiDocument document in uiOptions.Documents.OrderBy(x => x.Title))
        {
            options.SwaggerEndpoint(document.Url, document.Title);
        }
    }
}
