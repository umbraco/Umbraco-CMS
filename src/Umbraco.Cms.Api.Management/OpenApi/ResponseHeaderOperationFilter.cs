using Microsoft.AspNetCore.Http;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using Umbraco.Cms.Api.Management.DependencyInjection;
using Umbraco.Cms.Core;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.OpenApi;

internal sealed class ResponseHeaderOperationFilter : IOperationFilter
{
    /// <summary>
    /// Modifies the specified OpenAPI operation by adding response headers to 201 Created responses, if the operation is mapped to the management API.
    /// </summary>
    /// <param name="operation">The OpenAPI operation whose responses may be modified.</param>
    /// <param name="context">The filter context containing metadata about the API operation.</param>
    /// <remarks>
    /// This method adds the <c>GeneratedResource</c> and <c>Location</c> headers to 201 Created responses. Header order is important for the back-office client.
    /// </remarks>
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (context.MethodInfo.HasMapToApiAttribute(ManagementApiConfiguration.ApiName) is false || operation.Responses is null)
        {
            return;
        }

        foreach ((var key, IOpenApiResponse value) in operation.Responses)
        {
            if (value is not OpenApiResponse openApiResponse)
            {
                continue;
            }

            switch (int.Parse(key))
            {
                case StatusCodes.Status201Created:
                    // NOTE: The header order matters to the back-office client. Do not change.
                    SetHeader(openApiResponse, Constants.Headers.GeneratedResource, "Identifier of the newly created resource", JsonSchemaType.String);
                    SetHeader(openApiResponse, Constants.Headers.Location, "Location of the newly created resource", JsonSchemaType.String, "uri");
                    break;
            }
        }
    }

    private static void SetHeader(OpenApiResponse value, string headerName, string description, JsonSchemaType type, string? format = null)
    {
        value.Headers ??= new Dictionary<string, IOpenApiHeader>();
        value.Headers[headerName] = new OpenApiHeader
        {
            Description = description,
            Schema = new OpenApiSchema { Description = description, Type = type, Format = format },
        };
    }
}
