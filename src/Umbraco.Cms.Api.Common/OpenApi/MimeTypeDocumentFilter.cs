using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Common.OpenApi;

/// <summary>
/// This filter explicitly removes all other mime types than application/json from a named OpenAPI document when application/json is accepted.
/// </summary>
public class MimeTypeDocumentFilter : IDocumentFilter
{
    private readonly string _documentName;

    public MimeTypeDocumentFilter(string documentName) => _documentName = documentName;

    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        if (context.DocumentName != _documentName)
        {
            return;
        }

        OpenApiOperation[] operations = swaggerDoc.Paths
            .SelectMany(path => path.Value.Operations?.Values ?? Enumerable.Empty<OpenApiOperation>())
            .ToArray();

        void RemoveUnwantedMimeTypes(IDictionary<string, OpenApiMediaType>? content)
        {
            if (content == null)
            {
                return;
            }

            if (content.ContainsKey("application/json"))
            {
                content.RemoveAll(r => r.Key != "application/json");
            }
        }

        IOpenApiRequestBody[] requestBodies =
            operations.Select(operation => operation.RequestBody).WhereNotNull().ToArray();
        foreach (OpenApiRequestBody requestBody in requestBodies)
        {
            RemoveUnwantedMimeTypes(requestBody.Content);
        }

        IOpenApiResponse[] responses =
            operations.SelectMany(operation => operation.Responses?.Values  ?? Enumerable.Empty<IOpenApiResponse>() ).WhereNotNull().ToArray();
        foreach (IOpenApiResponse response in responses)
        {
            RemoveUnwantedMimeTypes(response.Content);
        }
    }
}
