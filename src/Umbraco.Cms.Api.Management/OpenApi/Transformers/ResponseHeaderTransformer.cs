using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Api.Management.OpenApi.Transformers;

/// <summary>
/// Transforms OpenAPI operations to include response headers for specific status codes.
/// </summary>
internal sealed class ResponseHeaderTransformer : IOpenApiOperationTransformer
{
    /// <inheritdoc/>
    public Task TransformAsync(
        OpenApiOperation operation,
        OpenApiOperationTransformerContext context,
        CancellationToken cancellationToken)
    {
        foreach ((var key, IOpenApiResponse value) in operation.Responses ?? new OpenApiResponses())
        {
            switch (int.Parse(key))
            {
                case StatusCodes.Status201Created:
                    // NOTE: The header order matters to the back-office client. Do not change.
                    var response = (OpenApiResponse)value;
                    SetHeader(
                        response,
                        Constants.Headers.GeneratedResource,
                        "Identifier of the newly created resource",
                        JsonSchemaType.String);
                    SetHeader(
                        response,
                        Constants.Headers.Location,
                        "Location of the newly created resource",
                        JsonSchemaType.String,
                        "uri");
                    break;
            }
        }

        return Task.CompletedTask;
    }

    private static void SetHeader(
        OpenApiResponse value,
        string headerName,
        string description,
        JsonSchemaType type,
        string? format = null)
    {
        value.Headers ??= new Dictionary<string, IOpenApiHeader>();
        value.Headers[headerName] = new OpenApiHeader
        {
            Description = description,
            Schema = new OpenApiSchema { Description = description, Type = type, Format = format }
        };
    }
}
