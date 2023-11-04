using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Umbraco.Cms.Api.Delivery.Configuration;
using Umbraco.Cms.Api.Delivery.Controllers;

namespace Umbraco.Cms.Api.Delivery.Filters;

internal sealed class SwaggerTranslationApiDocumentationFilter : SwaggerDocumentationFilterBase<TranslationApiControllerBase>
{
    protected override string DocumentationLink => DeliveryApiConfiguration.ApiDocumentationContentArticleLink;

    protected override void ApplyOperation(OpenApiOperation operation, OperationFilterContext context)
    {
        operation.Parameters ??= new List<OpenApiParameter>();

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
    }

    protected override void ApplyParameter(OpenApiParameter parameter, ParameterFilterContext context)
    {
        return;
    }
}
