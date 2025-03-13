using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Umbraco.Cms.Api.Delivery.Configuration;
using Umbraco.Cms.Api.Delivery.Controllers;
using Umbraco.Cms.Api.Delivery.Controllers.Media;

namespace Umbraco.Cms.Api.Delivery.Filters;

internal sealed class SwaggerMediaDocumentationFilter : SwaggerDocumentationFilterBase<MediaApiControllerBase>
{
    protected override string DocumentationLink => DeliveryApiConfiguration.ApiDocumentationMediaArticleLink;

    protected override void ApplyOperation(OpenApiOperation operation, OperationFilterContext context)
    {
        operation.Parameters ??= new List<OpenApiParameter>();

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

    private Dictionary<string, OpenApiExample> FetchQueryParameterExamples() =>
        new()
        {
            {
                "Select all children at root level",
                new OpenApiExample { Value = new OpenApiString("children:/") }
            },
            {
                "Select all children of a media item by id",
                new OpenApiExample { Value = new OpenApiString("children:id") }
            },
            {
                "Select all children of a media item by path",
                new OpenApiExample { Value = new OpenApiString("children:path") }
            }
        };

    private Dictionary<string, OpenApiExample> FilterQueryParameterExamples() =>
        new()
        {
            { "Default filter", new OpenApiExample { Value = new OpenApiString(string.Empty) } },
            {
                "Filter by media type",
                new OpenApiExample { Value = new OpenApiArray { new OpenApiString("mediaType:alias1") } }
            },
            {
                "Filter by name",
                new OpenApiExample { Value = new OpenApiArray { new OpenApiString("name:nodeName") } }
            }
        };

    private Dictionary<string, OpenApiExample> SortQueryParameterExamples() =>
        new()
        {
            { "Default sort", new OpenApiExample { Value = new OpenApiString(string.Empty) } },
            {
                "Sort by create date",
                new OpenApiExample
                {
                    Value = new OpenApiArray
                    {
                        new OpenApiString("createDate:asc"), new OpenApiString("createDate:desc")
                    }
                }
            },
            {
                "Sort by name",
                new OpenApiExample
                {
                    Value = new OpenApiArray { new OpenApiString("name:asc"), new OpenApiString("name:desc") }
                }
            },
            {
                "Sort by sort order",
                new OpenApiExample
                {
                    Value = new OpenApiArray
                    {
                        new OpenApiString("sortOrder:asc"), new OpenApiString("sortOrder:desc")
                    }
                }
            },
            {
                "Sort by update date",
                new OpenApiExample
                {
                    Value = new OpenApiArray
                    {
                        new OpenApiString("updateDate:asc"), new OpenApiString("updateDate:desc")
                    }
                }
            }
        };
}
