using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Common.OpenApi;

/// <summary>
/// Removes unwanted MIME types from OpenAPI operations, keeping only the content types
/// declared by <c>[Consumes]</c> for request bodies or <c>application/json</c> as the default.
/// </summary>
internal class MimeTypesTransformer : IOpenApiOperationTransformer
{
    /// <inheritdoc/>
    public Task TransformAsync(
        OpenApiOperation operation,
        OpenApiOperationTransformerContext context,
        CancellationToken cancellationToken)
    {
        // For request bodies, keep only the content types declared in [Consumes], or fall back to application/json.
        if (operation.RequestBody?.Content is { } requestContent)
        {
            var explicitContentTypes = context.Description.ActionDescriptor.EndpointMetadata
                .OfType<ConsumesAttribute>()
                .SelectMany(p => p.ContentTypes)
                .Distinct()
                .ToArray();

            if (explicitContentTypes.Length != 0)
            {
                // Replace content types entirely with what [Consumes] declares,
                // preserving the schema from the existing entry.
                OpenApiMediaType? existingMediaType = requestContent.Values.FirstOrDefault();
                requestContent.Clear();
                foreach (var contentType in explicitContentTypes)
                {
                    requestContent[contentType] = existingMediaType ?? new OpenApiMediaType();
                }
            }
            else
            {
                RemoveNonJsonMimeTypes(requestContent);
            }
        }

        // For responses, always keep only application/json.
        foreach (IOpenApiResponse response in (operation.Responses ?? []).Values)
        {
            if (response is OpenApiResponse openApiResponse)
            {
                RemoveNonJsonMimeTypes(openApiResponse.Content);
            }
        }

        return Task.CompletedTask;
    }

    private static void RemoveNonJsonMimeTypes(IDictionary<string, OpenApiMediaType>? content)
    {
        if (content?.ContainsKey("application/json") != true)
        {
            return;
        }

        content.RemoveAll(r => r.Key != "application/json");
    }
}
