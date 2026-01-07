namespace Umbraco.Cms.Api.Common.OpenApi;

/// <summary>
/// Options for configuring OpenAPI documents and UI.
/// </summary>
public class UmbracoOpenApiOptions
{
    /// <summary>
    /// Gets or sets whether OpenAPI documents are enabled.
    /// Default: enabled in non-production environments to avoid exposing
    /// API structure on public-facing websites.
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
    /// Use {documentName} as a placeholder for the document name.
    /// Default: "{backOfficePath}/openapi/{documentName}.json".
    /// </summary>
    public string RouteTemplate { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the route prefix for OpenAPI UI.
    /// Default: "{backOfficePath}/openapi".
    /// </summary>
    public string UiRoutePrefix { get; set; } = string.Empty;
}
