using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Umbraco.Cms.Api.Common.OpenApi;

/// <summary>
/// This filter explicitly removes all security schemes from a named OpenAPI document.
/// </summary>
public class RemoveSecuritySchemesDocumentFilter : IDocumentFilter
{
    private readonly string _documentName;

    /// <summary>
    ///     Initializes a new instance of the <see cref="RemoveSecuritySchemesDocumentFilter"/> class.
    /// </summary>
    /// <param name="documentName">The name of the OpenAPI document to filter.</param>
    public RemoveSecuritySchemesDocumentFilter(string documentName)
        => _documentName = documentName;

    /// <inheritdoc/>
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        if (context.DocumentName != _documentName)
        {
            return;
        }

        swaggerDoc.Components?.SecuritySchemes?.Clear();
    }
}
