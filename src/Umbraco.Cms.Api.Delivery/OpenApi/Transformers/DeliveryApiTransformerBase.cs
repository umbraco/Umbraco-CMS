using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Umbraco.Cms.Api.Delivery.OpenApi.Transformers;

internal abstract class DeliveryApiTransformerBase : IOpenApiOperationTransformer
{
    /// <summary>
    /// Gets the link to the relevant documentation section.
    /// </summary>
    protected abstract string DocumentationLink { get; }

    /// <inheritdoc/>
    public async Task TransformAsync(
        OpenApiOperation operation,
        OpenApiOperationTransformerContext context,
        CancellationToken cancellationToken)
    {
        if (!ShouldApply(context))
        {
            return;
        }

        AddExpand(operation);
        AddFields(operation);

        await ApplyAsync(operation, context, cancellationToken);
    }

    /// <summary>
    /// Determines whether the transformer should be applied for the given context.
    /// </summary>
    /// <param name="context">The operation transformer context.</param>
    /// <returns>>True if the transformer should be applied; otherwise, false.</returns>
    protected abstract bool ShouldApply(OpenApiOperationTransformerContext context);

    /// <summary>
    /// Applies the specific transformations to the OpenAPI operation.
    /// </summary>
    /// <param name="operation">The <see cref="OpenApiOperation"/> to modify.</param>
    /// <param name="context">The <see cref="OpenApiOperationTransformerContext"/> associated with the <see paramref="operation"/>.</param>
    /// <param name="cancellationToken">The cancellation token to use.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    protected abstract Task ApplyAsync(
        OpenApiOperation operation,
        OpenApiOperationTransformerContext context,
        CancellationToken cancellationToken);

    private void AddExpand(OpenApiOperation operation)
    {
        operation.Parameters ??= new List<IOpenApiParameter>();
        operation.Parameters.Add(
            new OpenApiParameter
            {
                Name = "expand",
                In = ParameterLocation.Query,
                Required = false,
                Description = QueryParameterDescription("Defines the properties that should be expanded in the response"),
                Schema = new OpenApiSchema { Type = JsonSchemaType.String },
                Examples = new Dictionary<string, IOpenApiExample>
                {
                    { "Expand none", new OpenApiExample { Value = "" } },
                    { "Expand all properties", new OpenApiExample { Value = "properties[$all]" } },
                    { "Expand specific property", new OpenApiExample { Value = "properties[alias1]" } },
                    { "Expand specific properties", new OpenApiExample { Value = "properties[alias1,alias2]" } },
                    { "Expand nested properties", new OpenApiExample { Value = "properties[alias1[properties[nestedAlias1,nestedAlias2]]]" } },
                },
            });
    }

    private void AddFields(OpenApiOperation operation)
    {
        operation.Parameters ??= new List<IOpenApiParameter>();
        operation.Parameters.Add(
            new OpenApiParameter
            {
                Name = "fields",
                In = ParameterLocation.Query,
                Required = false,
                Description =
                    QueryParameterDescription("Explicitly defines which properties should be included in the response (by default all properties are included)"),
                Schema = new OpenApiSchema { Type = JsonSchemaType.String },
                Examples = new Dictionary<string, IOpenApiExample>
                {
                    { "Include all properties", new OpenApiExample { Value = "properties[$all]" } },
                    { "Include only specific property", new OpenApiExample { Value = "properties[alias1]" } },
                    { "Include only specific properties", new OpenApiExample { Value = "properties[alias1,alias2]" } },
                    { "Include only specific nested properties", new OpenApiExample { Value = "properties[alias1[properties[nestedAlias1,nestedAlias2]]]" } },
                },
            });
    }

    protected void AddQueryParameterDocumentation(OpenApiParameter parameter, Dictionary<string, IOpenApiExample> examples, string description)
    {
        parameter.Description = QueryParameterDescription(description);
        parameter.Examples = examples;
    }

    protected string PaginationDescription(bool skip, string itemType)
        => $"Specifies the number of found {itemType} items to {(skip ? "skip" : "take")}. Use this to control pagination of the response.";

    private string QueryParameterDescription(string description)
        => $"{description}. Refer to [the documentation]({DocumentationLink}#query-parameters) for more details on this.";
}
