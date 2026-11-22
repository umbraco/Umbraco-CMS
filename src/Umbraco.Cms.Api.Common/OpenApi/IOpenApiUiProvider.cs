using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Umbraco.Cms.Api.Common.OpenApi;

/// <summary>
/// Serves a browsable UI for the OpenAPI documents listed in <see cref="UmbracoOpenApiUiOptions"/>.
/// </summary>
/// <remarks>
/// Umbraco generates the OpenAPI documents itself and does not depend on any particular UI. A site uses at most one
/// provider, registered with <c>AddOpenApiUi&lt;TProvider&gt;()</c>. The UI is only served when
/// <see cref="UmbracoOpenApiOptions.Enabled"/> and <see cref="UmbracoOpenApiOptions.DefaultUiEnabled"/> are both
/// <c>true</c>. A UI can hook into the request pipeline as middleware, as endpoints, or both.
/// </remarks>
public interface IOpenApiUiProvider
{
    /// <summary>
    /// Adds middleware that serves the UI, for UIs implemented as middleware.
    /// </summary>
    /// <param name="app">The application builder, at the end of the Umbraco request pipeline.</param>
    void UseOpenApiUi(IApplicationBuilder app)
    {
    }

    /// <summary>
    /// Maps endpoints that serve the UI, for UIs implemented as endpoints.
    /// </summary>
    /// <param name="endpoints">The endpoint route builder.</param>
    void MapOpenApiUi(IEndpointRouteBuilder endpoints)
    {
    }

    /// <summary>
    /// Gets the root-relative paths the UI uses as the OAuth redirect target after authorizing.
    /// </summary>
    /// <remarks>
    /// These paths are registered, on every known back-office host, as redirect URIs of the
    /// <see cref="Umbraco.Cms.Core.Constants.OAuthClientIds.OpenApiUi"/> OAuth client.
    /// </remarks>
    /// <returns>The redirect paths, each starting with <c>/</c>.</returns>
    IEnumerable<string> GetOAuthRedirectPaths();
}
