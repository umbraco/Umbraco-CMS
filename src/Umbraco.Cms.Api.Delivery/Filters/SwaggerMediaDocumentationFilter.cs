using System.Text.Json.Nodes;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using Umbraco.Cms.Api.Delivery.Configuration;
using Umbraco.Cms.Api.Delivery.Controllers.Media;

namespace Umbraco.Cms.Api.Delivery.Filters;

internal sealed class SwaggerMediaDocumentationFilter : SwaggerDocumentationFilterBase<MediaApiControllerBase>
{
    protected override string DocumentationLink => DeliveryApiConfiguration.ApiDocumentationMediaArticleLink;

    protected override void ApplyOperation(OpenApiOperation operation, OperationFilterContext context)
    {
        operation.Parameters ??= new List<IOpenApiParameter>();

        AddExpand(operation, context);

        AddFields(operation, context);

        AddApiKey(operation);
    }

    protected override void ApplyParameter(OpenApiParameter parameter, ParameterFilterContext context)
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
            { "Default filter", new OpenApiExample { Value = string.Empty } },
            { "Filter by media type", new OpenApiExample { Value = new JsonArray { "mediaType:alias1" } } },
            { "Filter by name", new OpenApiExample { Value = new JsonArray { "name:nodeName" } } },
        };

    private Dictionary<string, IOpenApiExample> SortQueryParameterExamples() =>
        new()
        {
            { "Default sort", new OpenApiExample { Value = string.Empty } },
            { "Sort by create date", new OpenApiExample { Value = new JsonArray { "createDate:asc", "createDate:desc" } } },
            { "Sort by name", new OpenApiExample { Value = new JsonArray { "name:asc", "name:desc" } } },
            { "Sort by sort order", new OpenApiExample { Value = new JsonArray { "sortOrder:asc", "sortOrder:desc" } } },
            { "Sort by update date", new OpenApiExample { Value = new JsonArray { "updateDate:asc", "updateDate:desc" } } },
        };
}
