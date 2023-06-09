using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Umbraco.Cms.Api.Delivery.Configuration;

namespace Umbraco.Cms.Api.Delivery.Filters;

public class AddCompleteSwaggerDocumentationFilter : IOperationFilter, IParameterFilter
{
    private const string DocumentationReference =
        $"*For more information, see the [Query parameters]({DeliveryApiConfiguration.ApiDocumentationArticleLink}#query-parameters) section in our dedicated documentation article.*";

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        operation.Parameters ??= new List<OpenApiParameter>();

        operation.Parameters.Add(new OpenApiParameter
        {
            Name = "expand",
            In = ParameterLocation.Query,
            Required = false,
            Description = DocumentationReference,
            Schema = new OpenApiSchema { Type = "String" },
            Examples = new Dictionary<string, OpenApiExample>
            {
                { "Expand none", new OpenApiExample { Value = new OpenApiString("") } },
                { "Expand all", new OpenApiExample { Value = new OpenApiString("all") } },
                {
                    "Expand specific property",
                    new OpenApiExample { Value = new OpenApiString("property:alias1") }
                },
                {
                    "Expand specific properties",
                    new OpenApiExample { Value = new OpenApiString("property:alias1,alias2") }
                }
            }
        });
    }

    public void Apply(OpenApiParameter parameter, ParameterFilterContext context)
    {
        switch (parameter.Name)
        {
            case "fetch":
                AddQueryParameterDocumentation(parameter, FetchQueryParameterExamples());
                break;
            case "filter":
                AddQueryParameterDocumentation(parameter, FilterQueryParameterExamples());
                break;
            case "sort":
                AddQueryParameterDocumentation(parameter, SortQueryParameterExamples());
                break;
            default:
                return;
        }
    }

    private void AddQueryParameterDocumentation(OpenApiParameter parameter, Dictionary<string, OpenApiExample> examples)
    {
        parameter.Description = DocumentationReference;
        parameter.Examples = examples;
    }

    private Dictionary<string, OpenApiExample> FetchQueryParameterExamples() =>
        new()
        {
            { "Select all", new OpenApiExample { Value = new OpenApiString("") } },
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
            { "Default filter", new OpenApiExample { Value = new OpenApiString("") } },
            {
                "Filter by content type",
                new OpenApiExample { Value = new OpenApiArray { new OpenApiString("contentType:alias1") } }
            },
            {
                "Filter by name",
                new OpenApiExample { Value = new OpenApiArray { new OpenApiString("name:nodeName") } }
            }
        };

    private Dictionary<string, OpenApiExample> SortQueryParameterExamples() =>
        new()
        {
            { "Default sort", new OpenApiExample { Value = new OpenApiString("") } },
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
