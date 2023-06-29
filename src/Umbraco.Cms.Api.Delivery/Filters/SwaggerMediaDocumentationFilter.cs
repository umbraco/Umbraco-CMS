using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Umbraco.Cms.Api.Delivery.Configuration;
using Umbraco.Cms.Api.Delivery.Controllers;

namespace Umbraco.Cms.Api.Delivery.Filters;

internal class SwaggerMediaDocumentationFilter : SwaggerDocumentationFilterBase<MediaApiControllerBase>
{
    protected override string DocumentationLink => DeliveryApiConfiguration.ApiDocumentationMediaArticleLink;

    protected override void ApplyOperation(OpenApiOperation operation, OperationFilterContext context)
    {
        operation.Parameters ??= new List<OpenApiParameter>();

        AddExpand(operation);

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
            case "sort":
                parameter.Description = "Currently unsupported - intended for future (or custom) extension.";
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
            { "Select all", new OpenApiExample { Value = new OpenApiString(string.Empty) } },
            {
                "Select all children of a node by id",
                new OpenApiExample { Value = new OpenApiString("children:id") }
            },
            {
                "Select all children of a node by path",
                new OpenApiExample { Value = new OpenApiString("children:path") }
            }
        };
}
