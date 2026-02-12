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

    /// <summary>
    ///     Initializes a new instance of the <see cref="MimeTypeDocumentFilter"/> class.
    /// </summary>
    /// <param name="documentName">The name of the OpenAPI document to filter.</param>
    public MimeTypeDocumentFilter(string documentName) => _documentName = documentName;

    /// <inheritdoc/>
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        if (context.DocumentName != _documentName)
        {
            return;
        }

        OpenApiOperation[] operations = swaggerDoc.Paths
            .SelectMany(path => path.Value.Operations?.Values ?? Enumerable.Empty<OpenApiOperation>())
            .ToArray();

        static void RemoveUnwantedMimeTypes(IDictionary<string, OpenApiMediaType>? content)
        {
            if (content is null || content.ContainsKey("application/json") is false)
            {
                return;
            }

            content.RemoveAll(r => r.Key != "application/json");
        }

        OpenApiRequestBody[] requestBodies = operations
            .Select(operation => operation.RequestBody)
            .OfType<OpenApiRequestBody>()
            .ToArray();
        foreach (OpenApiRequestBody requestBody in requestBodies)
        {
            RemoveUnwantedMimeTypes(requestBody.Content);
        }

        OpenApiResponse[] responses = operations
            .SelectMany(operation => operation.Responses?.Values ?? Enumerable.Empty<IOpenApiResponse>())
            .OfType<OpenApiResponse>()
            .ToArray();
        foreach (OpenApiResponse response in responses)
        {
            RemoveUnwantedMimeTypes(response.Content);
        }
    }
}
