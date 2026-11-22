using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Api.Common.OpenApi;

namespace Umbraco.Cms.Api.OpenApi.SwaggerUI;

/// <summary>
/// Serves the OpenAPI documents with Swagger UI.
/// </summary>
/// <remarks>
/// Swagger UI is configured through <see cref="Swashbuckle.AspNetCore.SwaggerUI.SwaggerUIOptions"/>, so it can be
/// customized further with <c>services.PostConfigure&lt;SwaggerUIOptions&gt;(...)</c>.
/// </remarks>
public sealed class SwaggerUiOpenApiUiProvider : IOpenApiUiProvider
{
    /// <summary>
    /// The page Swagger UI completes the OAuth authorization code flow on, relative to its route prefix.
    /// </summary>
    internal const string OAuthRedirectPage = "oauth2-redirect.html";

    private readonly IOptions<UmbracoOpenApiOptions> _openApiOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="SwaggerUiOpenApiUiProvider"/> class.
    /// </summary>
    /// <param name="openApiOptions">The Umbraco OpenAPI options.</param>
    public SwaggerUiOpenApiUiProvider(IOptions<UmbracoOpenApiOptions> openApiOptions)
        => _openApiOptions = openApiOptions;

    /// <inheritdoc />
    public void UseOpenApiUi(IApplicationBuilder app) => app.UseSwaggerUI();

    /// <inheritdoc />
    public IEnumerable<string> GetOAuthRedirectPaths()
        => [$"/{_openApiOptions.Value.UiRoutePrefix.Trim('/')}/{OAuthRedirectPage}"];
}
