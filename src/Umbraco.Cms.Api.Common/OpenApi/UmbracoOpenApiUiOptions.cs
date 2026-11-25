using Umbraco.Cms.Core;

namespace Umbraco.Cms.Api.Common.OpenApi;

/// <summary>
/// Describes what an <see cref="IOpenApiUiProvider"/> should present, independent of the UI that presents it.
/// </summary>
public class UmbracoOpenApiUiOptions
{
    /// <summary>
    /// Gets the OpenAPI documents to offer in the UI.
    /// </summary>
    /// <remarks>
    /// Populated by <c>AddOpenApiDocumentToUi</c>. Each document appears once, in the order it was added; a UI may
    /// present them in a different order, for example sorted by <see cref="OpenApiUiDocument.Title"/>.
    /// </remarks>
    public IList<OpenApiUiDocument> Documents { get; } = new List<OpenApiUiDocument>();

    /// <summary>
    /// Gets the names of the OAuth security schemes, as declared in the OpenAPI documents, that the UI should
    /// authorize with the <see cref="Constants.OAuthClientIds.OpenApiUi"/> client, using the authorization code flow
    /// with PKCE.
    /// </summary>
    /// <remarks>
    /// Some UIs apply the client ID to every scheme and do not need this; others configure each scheme by name.
    /// </remarks>
    public ISet<string> OAuthSecuritySchemes { get; } = new HashSet<string>(StringComparer.Ordinal);
}
