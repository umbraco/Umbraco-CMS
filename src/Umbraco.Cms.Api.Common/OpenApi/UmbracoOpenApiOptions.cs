namespace Umbraco.Cms.Api.Common.OpenApi;

/// <summary>
/// Options for configuring OpenAPI documents and UI.
/// </summary>
/// <remarks>
/// These options are populated by <c>AddUmbracoOpenApi</c> during DI configuration, which resolves the back-office path
/// from <see cref="Core.Hosting.IHostingEnvironment"/> and sets the default values for
/// <see cref="RouteTemplate"/> and <see cref="UiRoutePrefix"/>. Consumers that read this options type before
/// <c>AddUmbracoOpenApi</c> has run will observe the uninitialised defaults (empty strings for the route properties). 
/// </remarks>
public class UmbracoOpenApiOptions
{
    /// <summary>
    /// Gets or sets whether OpenAPI documents are enabled.
    /// Configured to <c>true</c> in non-production environments by default; <c>false</c> until configured.
    /// This avoids exposing API structure on public-facing websites.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Gets or sets whether the default OpenAPI UI is enabled.
    /// Only applies when <see cref="Enabled"/> is true.
    /// Set to false to disable the default UI while keeping OpenAPI documents available,
    /// allowing you to use an alternative UI.
    /// Default: true.
    /// </summary>
    public bool DefaultUiEnabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the route template for OpenAPI JSON documents.
    /// Use <c>{documentName}</c> as a placeholder for the document name.
    /// </summary>
    /// <remarks>
    /// Populated by <c>AddUmbracoOpenApi</c> to <c>"{backOfficePath}/openapi/{documentName}.json"</c>. The initial
    /// <see cref="string.Empty"/> default is a sentinel for "not yet configured" — it is not a usable route template.
    /// </remarks>
    public string RouteTemplate { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the route prefix for OpenAPI UI.
    /// </summary>
    /// <remarks>
    /// Populated by <c>AddUmbracoOpenApi</c> to <c>"{backOfficePath}/openapi"</c>. The initial <see cref="string.Empty"/>
    /// default is a sentinel for "not yet configured" — it is not a usable route prefix.
    /// </remarks>
    public string UiRoutePrefix { get; set; } = string.Empty;
}
