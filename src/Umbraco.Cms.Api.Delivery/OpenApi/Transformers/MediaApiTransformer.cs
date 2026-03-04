using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using Umbraco.Cms.Api.Delivery.Configuration;
using Umbraco.Cms.Api.Delivery.Controllers.Media;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Delivery.OpenApi.Transformers;

/// <summary>
/// Transforms OpenAPI operations for the Media API, adding relevant parameters and documentation.
/// </summary>
internal sealed class MediaApiTransformer : DeliveryApiTransformerBase
{
    protected override string DocumentationLink => DeliveryApiConfiguration.ApiDocumentationMediaArticleLink;

    /// <inheritdoc/>
    protected override bool ShouldApply(OpenApiOperationTransformerContext context) =>
        context.Description.ActionDescriptor is ControllerActionDescriptor description
        && description.ControllerTypeInfo.Implements<MediaApiControllerBase>();

    /// <inheritdoc/>
    protected override Task ApplyAsync(
        OpenApiOperation operation,
        OpenApiOperationTransformerContext context,
        CancellationToken cancellationToken)
    {
        operation.Parameters ??= new List<IOpenApiParameter>();
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
                AddQueryParameterDocumentation(parameter, FetchQueryParameterExamples(), "Specifies the media items to fetch");
                break;
            case "filter":
                AddQueryParameterDocumentation(parameter, FilterQueryParameterExamples(), "Defines how to filter the fetched media items");
                break;
            case "sort":
                AddQueryParameterDocumentation(parameter, SortQueryParameterExamples(), "Defines how to sort the found media items");
                break;
            case "skip":
                parameter.Description = PaginationDescription(true, "media");
                break;
            case "take":
                parameter.Description = PaginationDescription(false, "media");
                break;
            default:
                return;
        }
    }

    private Dictionary<string, IOpenApiExample> FetchQueryParameterExamples() =>
        new()
        {
            { "Select all children at root level", new OpenApiExample { Value = "children:/" } },
            { "Select all children of a media item by id", new OpenApiExample { Value = "children:id" } },
            { "Select all children of a media item by path", new OpenApiExample { Value = "children:path" } },
        };

    private Dictionary<string, IOpenApiExample> FilterQueryParameterExamples() =>
        new()
        {
            { "Default filter", new OpenApiExample { Value = new JsonArray(string.Empty) } },
            { "Filter by media type", new OpenApiExample { Value = new JsonArray("mediaType:alias1") } },
            { "Filter by name", new OpenApiExample { Value = new JsonArray("name:nodeName") } },
        };

    private Dictionary<string, IOpenApiExample> SortQueryParameterExamples() =>
        new()
        {
            { "Default sort", new OpenApiExample { Value = new JsonArray(string.Empty) } },
            { "Sort by create date", new OpenApiExample { Value = new JsonArray("createDate:asc", "createDate:desc") } },
            { "Sort by name", new OpenApiExample { Value = new JsonArray("name:asc", "name:desc") } },
            { "Sort by sort order", new OpenApiExample { Value = new JsonArray("sortOrder:asc", "sortOrder:desc") } },
            { "Sort by update date", new OpenApiExample { Value = new JsonArray("updateDate:asc", "updateDate:desc") } },
        };
}
