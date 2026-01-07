namespace Umbraco.Cms.Api.Common.OpenApi;

/// <summary>
/// Options for configuring OpenAPI and Swagger UI.
/// </summary>
public class UmbracoOpenApiOptions
{
    /// <summary>
    /// Gets or sets whether OpenAPI and Swagger UI are enabled.
    /// Default: enabled in non-production environments to avoid exposing
    /// API structure on public-facing websites.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Gets or sets the route template for OpenAPI JSON documents.
    /// Use {documentName} as a placeholder for the document name.
    /// Default: "{backOfficePath}/swagger/{documentName}/swagger.json".
    /// </summary>
    public string RouteTemplate { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the route prefix for Swagger UI.
    /// Default: "{backOfficePath}/swagger".
    /// </summary>
    public string UiRoutePrefix { get; set; } = string.Empty;
}
