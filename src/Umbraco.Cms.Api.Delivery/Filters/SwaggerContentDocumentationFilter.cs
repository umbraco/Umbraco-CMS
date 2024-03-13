using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Umbraco.Cms.Api.Delivery.Configuration;
using Umbraco.Cms.Api.Delivery.Controllers;
using Umbraco.Cms.Api.Delivery.Controllers.Content;

namespace Umbraco.Cms.Api.Delivery.Filters;

internal sealed class SwaggerContentDocumentationFilter : SwaggerDocumentationFilterBase<ContentApiControllerBase>
{
    protected override string DocumentationLink => DeliveryApiConfiguration.ApiDocumentationContentArticleLink;

    protected override void ApplyOperation(OpenApiOperation operation, OperationFilterContext context)
    {
        operation.Parameters ??= new List<OpenApiParameter>();

        AddExpand(operation, context);

        AddFields(operation, context);

        operation.Parameters.Add(new OpenApiParameter
        {
            Name = "Accept-Language",
            In = ParameterLocation.Header,
            Required = false,
            Description = "Defines the language to return. Use this when querying language variant content items.",
            Schema = new OpenApiSchema { Type = "string" },
            Examples = new Dictionary<string, OpenApiExample>
            {
                { "Default", new OpenApiExample { Value = new OpenApiString(string.Empty) } },
                { "English culture", new OpenApiExample { Value = new OpenApiString("en-us") } }
            }
        });

        AddApiKey(operation);

        operation.Parameters.Add(new OpenApiParameter
        {
            Name = "Preview",
            In = ParameterLocation.Header,
            Required = false,
            Description = "Whether to request draft content.",
            Schema = new OpenApiSchema { Type = "boolean" }
        });

        operation.Parameters.Add(new OpenApiParameter
        {
            Name = "Start-Item",
            In = ParameterLocation.Header,
            Required = false,
            Description = "URL segment or GUID of a root content item.",
            Schema = new OpenApiSchema { Type = "string" }
        });
    }

    protected override void ApplyParameter(OpenApiParameter parameter, ParameterFilterContext context)
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

    private Dictionary<string, OpenApiExample> FetchQueryParameterExamples() =>
        new()
        {
            { "Select all", new OpenApiExample { Value = new OpenApiString(string.Empty) } },
            {
                "Select all ancestors of a node by id",
                new OpenApiExample { Value = new OpenApiString("ancestors:id") }
            },
            {
                "Select all ancestors of a node by path",
                new OpenApiExample { Value = new OpenApiString("ancestors:path") }
            },
            {
                "Select all children of a node by id",
                new OpenApiExample { Value = new OpenApiString("children:id") }
            },
            {
                "Select all children of a node by path",
                new OpenApiExample { Value = new OpenApiString("children:path") }
            },
            {
                "Select all descendants of a node by id",
                new OpenApiExample { Value = new OpenApiString("descendants:id") }
            },
            {
                "Select all descendants of a node by path",
                new OpenApiExample { Value = new OpenApiString("descendants:path") }
            }
        };

    private Dictionary<string, OpenApiExample> FilterQueryParameterExamples() =>
        new()
        {
            { "Default filter", new OpenApiExample { Value = new OpenApiString(string.Empty) } },
            {
                "Filter by content type (equals)",
                new OpenApiExample { Value = new OpenApiArray { new OpenApiString("contentType:alias1") } }
            },
            {
                "Filter by name (contains)",
                new OpenApiExample { Value = new OpenApiArray { new OpenApiString("name:nodeName") } }
            },
            {
                "Filter by creation date (less than)",
                new OpenApiExample { Value = new OpenApiArray { new OpenApiString("createDate<2024-01-01") } }
            },
            {
                "Filter by update date (greater than or equal)",
                new OpenApiExample { Value = new OpenApiArray { new OpenApiString("updateDate>:2023-01-01") } }
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
                "Sort by level",
                new OpenApiExample
                {
                    Value = new OpenApiArray { new OpenApiString("level:asc"), new OpenApiString("level:desc") }
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
