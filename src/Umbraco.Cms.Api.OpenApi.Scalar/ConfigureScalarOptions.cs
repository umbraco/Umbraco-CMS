using Microsoft.Extensions.Options;
using Scalar.AspNetCore;
using Umbraco.Cms.Api.Common.OpenApi;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Api.OpenApi.Scalar;

/// <summary>
/// Configures the Scalar API reference from the UI-independent Umbraco OpenAPI options.
/// </summary>
internal sealed class ConfigureScalarOptions : IConfigureOptions<ScalarOptions>
{
    private readonly IOptions<UmbracoOpenApiOptions> _openApiOptions;
    private readonly IOptions<UmbracoOpenApiUiOptions> _uiOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigureScalarOptions"/> class.
    /// </summary>
    /// <param name="openApiOptions">The Umbraco OpenAPI options.</param>
    /// <param name="uiOptions">The UI-independent options describing what the UI presents.</param>
    public ConfigureScalarOptions(IOptions<UmbracoOpenApiOptions> openApiOptions, IOptions<UmbracoOpenApiUiOptions> uiOptions)
    {
        _openApiOptions = openApiOptions;
        _uiOptions = uiOptions;
    }

    /// <inheritdoc />
    public void Configure(ScalarOptions options)
    {
        UmbracoOpenApiUiOptions uiOptions = _uiOptions.Value;

        options.PersistentAuthentication = true;

        // Each of these reaches Scalar's hosted services; a site opts in rather than out.
        options.DisableTelemetry();
        options.DisableAgent();
        options.DefaultFonts = false;

        // A page opened for a single document resolves it from the route pattern rather than the document list.
        options.OpenApiRoutePattern = _openApiOptions.Value.RouteTemplate;

        var isDefault = true;
        foreach (OpenApiUiDocument document in uiOptions.Documents.OrderBy(x => x.Title))
        {
            options.AddDocument(document.Name, document.Title, isDefault: isDefault);
            isDefault = false;
        }

        foreach (var scheme in uiOptions.OAuthSecuritySchemes)
        {
            options.AddAuthorizationCodeFlow(scheme, flow =>
            {
                flow.ClientId = Constants.OAuthClientIds.OpenApiUi;
                flow.Pkce = Pkce.Sha256;
            });
        }

        options.AddPreferredSecuritySchemes(uiOptions.OAuthSecuritySchemes);
    }
}
