using System.IO.Pipelines;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Umbraco.Cms.Api.Common.OpenApi;

/// <summary>
/// Transformer to fix file return types in OpenAPI schema.
/// </summary>
/// <remarks>Can be removed once https://github.com/dotnet/aspnetcore/pull/63504 and
/// https://github.com/dotnet/aspnetcore/pull/64562 are released.</remarks>
public class FixFileReturnTypesTransformer : IOpenApiSchemaTransformer
{
    private static readonly Type[] _binaryStringTypes =
    [
        typeof(IFormFile),
        typeof(FileResult),
        typeof(Stream),
        typeof(PipeReader),
    ];

    /// <inheritdoc />
    public Task TransformAsync(
        OpenApiSchema schema,
        OpenApiSchemaTransformerContext context,
        CancellationToken cancellationToken)
    {
        if (_binaryStringTypes.Any(possibleBaseType => possibleBaseType.IsAssignableFrom(context.JsonTypeInfo.Type)) is false)
        {
            return Task.CompletedTask;
        }

        // Clear all properties
        schema.Properties?.Clear();
        schema.Required?.Clear();

        // Make it an inline schema
        schema.Metadata?.Remove("x-schema-id");

        // Set type to string with binary format
        schema.Type = JsonSchemaType.String;
        schema.Format = "binary";
        return Task.CompletedTask;
    }
}
