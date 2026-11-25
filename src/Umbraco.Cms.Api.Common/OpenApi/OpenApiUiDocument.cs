namespace Umbraco.Cms.Api.Common.OpenApi;

/// <summary>
/// An OpenAPI document offered in the OpenAPI UI.
/// </summary>
/// <param name="Name">The name of the document, as registered with <c>AddOpenApi</c>.</param>
/// <param name="Title">The title to display for the document.</param>
/// <param name="Url">The root-relative URL the document is served from.</param>
public sealed record OpenApiUiDocument(string Name, string Title, string Url);
