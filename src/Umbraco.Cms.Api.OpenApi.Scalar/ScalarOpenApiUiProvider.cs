using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Scalar.AspNetCore;
using Umbraco.Cms.Api.Common.OpenApi;
using Umbraco.Cms.Core.Security;

namespace Umbraco.Cms.Api.OpenApi.Scalar;

/// <summary>
/// Serves the OpenAPI documents with the Scalar API reference.
/// </summary>
/// <remarks>
/// Scalar is configured through <see cref="ScalarOptions"/>, so it can be customized further with
/// <c>services.PostConfigure&lt;ScalarOptions&gt;(...)</c>.
/// </remarks>
public sealed class ScalarOpenApiUiProvider : IOpenApiUiProvider
{
    private readonly IOptions<UmbracoOpenApiOptions> _openApiOptions;
    private readonly IOptions<UmbracoOpenApiUiOptions> _uiOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="ScalarOpenApiUiProvider"/> class.
    /// </summary>
    /// <param name="openApiOptions">The Umbraco OpenAPI options.</param>
    /// <param name="uiOptions">The UI-independent options describing what the UI presents.</param>
    public ScalarOpenApiUiProvider(IOptions<UmbracoOpenApiOptions> openApiOptions, IOptions<UmbracoOpenApiUiOptions> uiOptions)
    {
        _openApiOptions = openApiOptions;
        _uiOptions = uiOptions;
    }

    /// <inheritdoc />
    public void MapOpenApiUi(IEndpointRouteBuilder endpoints)
        => endpoints.MapScalarApiReference(RedirectPath.TrimEnd('/'), ConfigureForRequest);

    /// <inheritdoc />
    public IEnumerable<string> GetOAuthRedirectPaths() => [RedirectPath];

    // Scalar completes the authorization code flow by reading the redirect target in its popup, so any same-origin
    // page works; the reference's own root keeps the registered redirect URI independent of the selected document.
    private string RedirectPath => $"/{_openApiOptions.Value.UiRoutePrefix.Trim('/')}/";

    private void ConfigureForRequest(ScalarOptions options, HttpContext httpContext)
    {
        // Scalar bootstraps with an inline script, which a nonce-based Content-Security-Policy only runs when it
        // carries the nonce issued for this request.
        options.Nonce = httpContext.RequestServices.GetService<ICspNonceService>()?.GetNonce();

        // The authorization server only accepts absolute redirect URIs on the host serving the request.
        var redirectUri = $"{httpContext.Request.Scheme}://{httpContext.Request.Host}{RedirectPath}";

        foreach (var scheme in _uiOptions.Value.OAuthSecuritySchemes)
        {
            options.AddAuthorizationCodeFlow(scheme, flow => flow.RedirectUri = redirectUri);
        }
    }
}
