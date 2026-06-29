using Umbraco.Cms.Api.Common.OpenApi;

namespace Umbraco.Cms.Api.Management.OpenApi;

/// <summary>
/// Management-specific extension methods for <see cref="BackOfficeOpenApiDocumentBuilder"/>.
/// </summary>
public static class BackOfficeOpenApiDocumentBuilderExtensions
{
    /// <summary>
    /// Adds backoffice authentication requirements to the document.
    /// </summary>
    /// <param name="documentBuilder">The document builder.</param>
    /// <returns>The same builder for chaining.</returns>
    public static BackOfficeOpenApiDocumentBuilder WithBackOfficeAuthentication(
        this BackOfficeOpenApiDocumentBuilder documentBuilder)
        => documentBuilder.ConfigureOpenApiOptions(options => options.AddBackofficeSecurityRequirements());
}
