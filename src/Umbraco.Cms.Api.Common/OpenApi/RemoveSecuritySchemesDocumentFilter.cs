using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Umbraco.Cms.Api.Common.OpenApi;

/// <summary>
/// This filter explicitly removes all security schemes from a named OpenAPI document.
/// </summary>
public class RemoveSecuritySchemesDocumentFilter : IDocumentFilter
{
    private readonly string _documentName;

    public RemoveSecuritySchemesDocumentFilter(string documentName)
        => _documentName = documentName;

    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        if (context.DocumentName != _documentName)
        {
            return;
        }

        swaggerDoc.Components.SecuritySchemes.Clear();
    }
}
