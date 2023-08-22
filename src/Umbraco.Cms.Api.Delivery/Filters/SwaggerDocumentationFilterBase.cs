using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Delivery.Filters;

internal abstract class SwaggerDocumentationFilterBase<TBaseController> : IOperationFilter, IParameterFilter
    where TBaseController : Controller
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (CanApply(context.MethodInfo))
        {
            ApplyOperation(operation, context);
        }
    }

    public void Apply(OpenApiParameter parameter, ParameterFilterContext context)
    {
        if (CanApply(context.ParameterInfo.Member))
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

    protected void AddExpand(OpenApiOperation operation) =>
        operation.Parameters.Add(new OpenApiParameter
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

    private bool CanApply(MemberInfo member)
        => member.DeclaringType?.Implements<TBaseController>() is true;
}
