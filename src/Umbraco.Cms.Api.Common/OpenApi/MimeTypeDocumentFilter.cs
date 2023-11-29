using Microsoft.OpenApi.Models;
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
            .SelectMany(path => path.Value.Operations.Values)
            .ToArray();

        void RemoveUnwantedMimeTypes(IDictionary<string, OpenApiMediaType> content)
        {
            if (content.ContainsKey("application/json"))
            {
                content.RemoveAll(r => r.Key != "application/json");
            }

        }

        OpenApiRequestBody[] requestBodies = operations.Select(operation => operation.RequestBody).WhereNotNull().ToArray();
        foreach (OpenApiRequestBody requestBody in requestBodies)
        {
            RemoveUnwantedMimeTypes(requestBody.Content);
        }

        OpenApiResponse[] responses = operations.SelectMany(operation => operation.Responses.Values).WhereNotNull().ToArray();
        foreach (OpenApiResponse response in responses)
        {
            RemoveUnwantedMimeTypes(response.Content);
        }
    }
}
