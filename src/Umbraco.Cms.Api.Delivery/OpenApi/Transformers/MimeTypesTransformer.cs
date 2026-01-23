using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Delivery.OpenApi.Transformers;

/// <summary>
/// This filter explicitly removes all other mime types than application/json from a named OpenAPI document when application/json is accepted.
/// </summary>
internal class MimeTypesTransformer : IOpenApiDocumentTransformer
{
    /// <summary>
    /// Transforms the specified OpenAPI document by removing all other mime types than application/json from request and response bodies when application/json is accepted.
    /// </summary>
    /// <param name="document">The <see cref="OpenApiDocument"/> to modify.</param>
    /// <param name="context">The <see cref="OpenApiDocumentTransformerContext"/> associated with the <see paramref="document"/>.</param>
    /// <param name="cancellationToken">The cancellation token to use.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    public Task TransformAsync(
            OpenApiDocument document,
            OpenApiDocumentTransformerContext context,
            CancellationToken cancellationToken)
    {
        OpenApiOperation[] operations = document.Paths
            .SelectMany(path => (path.Value.Operations ?? []).Values)
            .ToArray();

        IOpenApiRequestBody[] requestBodies = operations.Select(operation => operation.RequestBody).WhereNotNull().ToArray();
        foreach (IOpenApiRequestBody requestBody in requestBodies)
        {
            RemoveUnwantedMimeTypes(requestBody.Content);
        }

        IOpenApiResponse[] responses = operations.SelectMany(operation => (operation.Responses ?? []).Values).WhereNotNull().ToArray();
        foreach (IOpenApiResponse response in responses)
        {
            RemoveUnwantedMimeTypes(response.Content);
        }

        return Task.CompletedTask;
    }

    private static void RemoveUnwantedMimeTypes(IDictionary<string, OpenApiMediaType>? content)
    {
        if (content?.ContainsKey("application/json") == true)
        {
            content.RemoveAll(r => r.Key != "application/json");
        }
    }
}
