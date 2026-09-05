using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using Umbraco.Cms.Api.Delivery.Configuration;
using Umbraco.Cms.Api.Delivery.Controllers.Content;
using Umbraco.Cms.Core;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Delivery.OpenApi.Transformers;

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
                Name = Constants.DeliveryApi.HeaderNames.AcceptLanguage,
                In = ParameterLocation.Header,
                Required = false,
                Description = "Defines the language to return. Use this when querying language variant content items.",
                Schema = new OpenApiSchema { Type = JsonSchemaType.String },
                Examples = new Dictionary<string, IOpenApiExample>
                {
                    { "Default", new OpenApiExample { Value = string.Empty } },
                    { "English culture", new OpenApiExample { Value = "en-us" } },
                },
            });

        operation.Parameters.Add(
            new OpenApiParameter
            {
                Name = Constants.DeliveryApi.HeaderNames.AcceptSegment,
                In = ParameterLocation.Header,
                Required = false,
                Description = "Defines the segment to return. Use this when querying segment variant content items.",
                Schema = new OpenApiSchema { Type = JsonSchemaType.String },
                Examples = new Dictionary<string, IOpenApiExample>
                {
                    { "Default", new OpenApiExample { Value = string.Empty } },
                    { "Segment One", new OpenApiExample { Value = "segment-one" } },
                },
            });

        operation.Parameters.Add(
            new OpenApiParameter
            {
                Name = Constants.DeliveryApi.HeaderNames.Preview,
                In = ParameterLocation.Header,
                Required = false,
                Description = "Whether to request draft content.",
                Schema = new OpenApiSchema { Type = JsonSchemaType.Boolean },
            });

        operation.Parameters.Add(
            new OpenApiParameter
            {
                Name = Constants.DeliveryApi.HeaderNames.StartItem,
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
            { "Select all", new OpenApiExample { Value = "" } },
            { "Select all ancestors of a node by id", new OpenApiExample { Value = "ancestors:id" } },
            { "Select all ancestors of a node by path", new OpenApiExample { Value = "ancestors:path" } },
            { "Select all children of a node by id", new OpenApiExample { Value = "children:id" } },
            { "Select all children of a node by path", new OpenApiExample { Value = "children:path" } },
            { "Select all descendants of a node by id", new OpenApiExample { Value = "descendants:id" } },
            { "Select all descendants of a node by path", new OpenApiExample { Value = "descendants:path" } },
        };

    private Dictionary<string, IOpenApiExample> FilterQueryParameterExamples() =>
        new()
        {
            { "Default filter", new OpenApiExample { Value = new JsonArray("") } },
            { "Filter by content type (equals)", new OpenApiExample { Value = new JsonArray("contentType:alias1") } },
            { "Filter by name (contains)", new OpenApiExample { Value = new JsonArray("name:nodeName") } },
            { "Filter by creation date (less than)", new OpenApiExample { Value = new JsonArray("createDate<2024-01-01") } },
            { "Filter by update date (greater than or equal)", new OpenApiExample { Value = new JsonArray("updateDate>:2023-01-01") } },
        };

    private Dictionary<string, IOpenApiExample> SortQueryParameterExamples() =>
        new()
        {
            { "Default sort", new OpenApiExample { Value = new JsonArray("") } },
            { "Sort by create date", new OpenApiExample { Value = new JsonArray("createDate:asc", "createDate:desc") } },
            { "Sort by level", new OpenApiExample { Value = new JsonArray("level:asc", "level:desc") } },
            { "Sort by name", new OpenApiExample { Value = new JsonArray("name:asc", "name:desc") } },
            { "Sort by sort order", new OpenApiExample { Value = new JsonArray("sortOrder:asc", "sortOrder:desc") } },
            { "Sort by update date", new OpenApiExample { Value = new JsonArray("updateDate:asc", "updateDate:desc") } },
        };
}
