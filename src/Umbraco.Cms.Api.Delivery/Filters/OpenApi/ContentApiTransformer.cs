using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using Umbraco.Cms.Api.Delivery.Configuration;
using Umbraco.Cms.Api.Delivery.Controllers.Content;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Delivery.Filters.OpenApi;

/// <summary>
/// Transforms OpenAPI operations for the Content API, adding relevant parameters and documentation.
/// </summary>
internal sealed class ContentApiTransformer : DeliveryApiTransformerBase
{
    /// <inheritdoc/>
    protected override string DocumentationLink => DeliveryApiConfiguration.ApiDocumentationContentArticleLink;

    /// <inheritdoc/>
    protected override bool ShouldApply(OpenApiOperationTransformerContext context) =>
        context.Description.ActionDescriptor is ControllerActionDescriptor description
        && description.ControllerTypeInfo.Implements<ContentApiControllerBase>();

    /// <inheritdoc/>
    protected override Task ApplyAsync(
        OpenApiOperation operation,
        OpenApiOperationTransformerContext context,
        CancellationToken cancellationToken)
    {
        operation.Parameters ??= new List<IOpenApiParameter>();
        operation.Parameters.Add(
            new OpenApiParameter
            {
                Name = Core.Constants.DeliveryApi.HeaderNames.AcceptLanguage,
                In = ParameterLocation.Header,
                Required = false,
                Description = "Defines the language to return. Use this when querying language variant content items.",
                Schema = new OpenApiSchema { Type = JsonSchemaType.String },
                Examples = new Dictionary<string, IOpenApiExample>
                {
                    { "Default", new OpenApiExample { Value = JsonValue.Create(string.Empty) } },
                    { "English culture", new OpenApiExample { Value = JsonValue.Create("en-us") } },
                },
            });

        operation.Parameters.Add(
            new OpenApiParameter
            {
                Name = Core.Constants.DeliveryApi.HeaderNames.AcceptSegment,
                In = ParameterLocation.Header,
                Required = false,
                Description = "Defines the segment to return. Use this when querying segment variant content items.",
                Schema = new OpenApiSchema { Type = JsonSchemaType.String },
                Examples = new Dictionary<string, IOpenApiExample>
                {
                    { "Default", new OpenApiExample { Value = JsonValue.Create(string.Empty) } },
                    { "Segment One", new OpenApiExample { Value = JsonValue.Create("segment-one") } },
                },
            });

        operation.Parameters.Add(
            new OpenApiParameter
            {
                Name = Core.Constants.DeliveryApi.HeaderNames.Preview,
                In = ParameterLocation.Header,
                Required = false,
                Description = "Whether to request draft content.",
                Schema = new OpenApiSchema { Type = JsonSchemaType.Boolean },
            });

        operation.Parameters.Add(
            new OpenApiParameter
            {
                Name = Core.Constants.DeliveryApi.HeaderNames.StartItem,
                In = ParameterLocation.Header,
                Required = false,
                Description = "URL segment or GUID of a root content item.",
                Schema = new OpenApiSchema { Type = JsonSchemaType.String },
            });


        foreach (OpenApiParameter parameter in operation.Parameters?.OfType<OpenApiParameter>() ?? [])
        {
            ApplyParameter(parameter);
        }

        return Task.CompletedTask;
    }


    private void ApplyParameter(OpenApiParameter parameter)
    {
        switch (parameter.Name)
        {
            case "fetch":
                AddQueryParameterDocumentation(parameter, FetchQueryParameterExamples(), "Specifies the content items to fetch");
                break;
            case "filter":
                AddQueryParameterDocumentation(parameter, FilterQueryParameterExamples(), "Defines how to filter the fetched content items");
                break;
            case "sort":
                AddQueryParameterDocumentation(parameter, SortQueryParameterExamples(), "Defines how to sort the found content items");
                break;
            case "skip":
                parameter.Description = PaginationDescription(true, "content");
                break;
            case "take":
                parameter.Description = PaginationDescription(false, "content");
                break;
            default:
                return;
        }
    }

    private Dictionary<string, IOpenApiExample> FetchQueryParameterExamples() =>
        new()
        {
            { "Select all", new OpenApiExample { Value = JsonValue.Create(string.Empty) } },
            { "Select all ancestors of a node by id", new OpenApiExample { Value = JsonValue.Create("ancestors:id") } },
            { "Select all ancestors of a node by path", new OpenApiExample { Value = JsonValue.Create("ancestors:path") } },
            { "Select all children of a node by id", new OpenApiExample { Value = JsonValue.Create("children:id") } },
            { "Select all children of a node by path", new OpenApiExample { Value = JsonValue.Create("children:path") } },
            { "Select all descendants of a node by id", new OpenApiExample { Value = JsonValue.Create("descendants:id") } },
            { "Select all descendants of a node by path", new OpenApiExample { Value = JsonValue.Create("descendants:path") } },
        };

    private Dictionary<string, IOpenApiExample> FilterQueryParameterExamples() =>
        new()
        {
            { "Default filter", new OpenApiExample { Value = JsonValue.Create(string.Empty) } },
            { "Filter by content type (equals)", new OpenApiExample { Value = new JsonArray { JsonValue.Create("contentType:alias1") } } },
            { "Filter by name (contains)", new OpenApiExample { Value = new JsonArray { JsonValue.Create("name:nodeName") } } },
            { "Filter by creation date (less than)", new OpenApiExample { Value = new JsonArray { JsonValue.Create("createDate<2024-01-01") } } },
            { "Filter by update date (greater than or equal)", new OpenApiExample { Value = new JsonArray { JsonValue.Create("updateDate>:2023-01-01") } } },
        };

    private Dictionary<string, IOpenApiExample> SortQueryParameterExamples() =>
        new()
        {
            { "Default sort", new OpenApiExample { Value = JsonValue.Create(string.Empty) } },
            { "Sort by create date", new OpenApiExample { Value = new JsonArray { JsonValue.Create("createDate:asc"), JsonValue.Create("createDate:desc") } } },
            { "Sort by level", new OpenApiExample { Value = new JsonArray { JsonValue.Create("level:asc"), JsonValue.Create("level:desc") } } },
            { "Sort by name", new OpenApiExample { Value = new JsonArray { JsonValue.Create("name:asc"), JsonValue.Create("name:desc") } } },
            { "Sort by sort order", new OpenApiExample { Value = new JsonArray { JsonValue.Create("sortOrder:asc"), JsonValue.Create("sortOrder:desc") } } },
            { "Sort by update date", new OpenApiExample { Value = new JsonArray { JsonValue.Create("updateDate:asc"), JsonValue.Create("updateDate:desc") } } },
        };
}
