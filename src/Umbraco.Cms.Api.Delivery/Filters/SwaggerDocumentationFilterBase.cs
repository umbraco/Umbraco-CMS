using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Umbraco.Cms.Api.Delivery.Filters;

internal abstract class SwaggerDocumentationFilterBase<TBaseController>
    : SwaggerFilterBase<TBaseController>, IOperationFilter, IParameterFilter
    where TBaseController : Controller
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (CanApply(context))
        {
            ApplyOperation(operation, context);
        }
    }

    public void Apply(OpenApiParameter parameter, ParameterFilterContext context)
    {
        if (CanApply(context))
        {
            ApplyParameter(parameter, context);
        }
    }

    protected abstract string DocumentationLink { get; }

    protected abstract void ApplyOperation(OpenApiOperation operation, OperationFilterContext context);

    protected abstract void ApplyParameter(OpenApiParameter parameter, ParameterFilterContext context);

    protected void AddQueryParameterDocumentation(OpenApiParameter parameter, Dictionary<string, OpenApiExample> examples, string description)
    {
        parameter.Description = QueryParameterDescription(description);
        parameter.Examples = examples;
    }

    protected void AddExpand(OpenApiOperation operation, OperationFilterContext context)
    {
        if (IsApiV1(context))
        {
            AddExpandV1(operation);
        }
        else
        {
            AddExpand(operation);
        }
    }

    protected void AddFields(OpenApiOperation operation, OperationFilterContext context)
    {
        if (IsApiV1(context))
        {
            // "fields" is not a thing in Delivery API V1
            return;
        }

        AddFields(operation);
    }

    protected void AddApiKey(OpenApiOperation operation) =>
        operation.Parameters.Add(new OpenApiParameter
        {
            Name = "Api-Key",
            In = ParameterLocation.Header,
            Required = false,
            Description = "API key specified through configuration to authorize access to the API.",
            Schema = new OpenApiSchema { Type = "string" }
        });

    protected string PaginationDescription(bool skip, string itemType)
        => $"Specifies the number of found {itemType} items to {(skip ? "skip" : "take")}. Use this to control pagination of the response.";

    private string QueryParameterDescription(string description)
        => $"{description}. Refer to [the documentation]({DocumentationLink}#query-parameters) for more details on this.";

    // FIXME: remove this when Delivery API V1 has been removed (expectedly in V15)
    private static bool IsApiV1(OperationFilterContext context)
        => context.ApiDescription.RelativePath?.Contains("api/v1") is true;

    // FIXME: remove this when Delivery API V1 has been removed (expectedly in V15)
    private void AddExpandV1(OpenApiOperation operation)
        => operation.Parameters.Add(new OpenApiParameter
        {
            Name = "expand",
            In = ParameterLocation.Query,
            Required = false,
            Description = QueryParameterDescription("Defines the properties that should be expanded in the response"),
            Schema = new OpenApiSchema { Type = "string" },
            Examples = new Dictionary<string, OpenApiExample>
            {
                { "Expand none", new OpenApiExample { Value = new OpenApiString(string.Empty) } },
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

    private void AddExpand(OpenApiOperation operation)
        => operation.Parameters.Add(new OpenApiParameter
        {
            Name = "expand",
            In = ParameterLocation.Query,
            Required = false,
            Description = QueryParameterDescription("Defines the properties that should be expanded in the response"),
            Schema = new OpenApiSchema { Type = "string" },
            Examples = new Dictionary<string, OpenApiExample>
            {
                { "Expand none", new OpenApiExample { Value = new OpenApiString(string.Empty) } },
                { "Expand all properties", new OpenApiExample { Value = new OpenApiString("properties[$all]") } },
                {
                    "Expand specific property",
                    new OpenApiExample { Value = new OpenApiString("properties[alias1]") }
                },
                {
                    "Expand specific properties",
                    new OpenApiExample { Value = new OpenApiString("properties[alias1,alias2]") }
                },
                {
                    "Expand nested properties",
                    new OpenApiExample { Value = new OpenApiString("properties[alias1[properties[nestedAlias1,nestedAlias2]]]") }
                }
            }
        });

    private void AddFields(OpenApiOperation operation)
        => operation.Parameters.Add(new OpenApiParameter
        {
            Name = "fields",
            In = ParameterLocation.Query,
            Required = false,
            Description = QueryParameterDescription("Explicitly defines which properties should be included in the response (by default all properties are included)"),
            Schema = new OpenApiSchema { Type = "string" },
            Examples = new Dictionary<string, OpenApiExample>
            {
                { "Include all properties", new OpenApiExample { Value = new OpenApiString("properties[$all]") } },
                {
                    "Include only specific property",
                    new OpenApiExample { Value = new OpenApiString("properties[alias1]") }
                },
                {
                    "Include only specific properties",
                    new OpenApiExample { Value = new OpenApiString("properties[alias1,alias2]") }
                },
                {
                    "Include only specific nested properties",
                    new OpenApiExample { Value = new OpenApiString("properties[alias1[properties[nestedAlias1,nestedAlias2]]]") }
                }
            }
        });
}
