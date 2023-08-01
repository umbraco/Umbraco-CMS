using Asp.Versioning;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Umbraco.Cms.Api.Common.OpenApi;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Common.Configuration;

public class ConfigureUmbracoSwaggerGenOptions : IConfigureOptions<SwaggerGenOptions>
{
    private readonly IOptions<ApiVersioningOptions> _apiVersioningOptions;
    private readonly IOperationIdSelector _operationIdSelector;
    private readonly ISchemaIdSelector _schemaIdSelector;

    public ConfigureUmbracoSwaggerGenOptions(
        IOptions<ApiVersioningOptions> apiVersioningOptions,
        IOperationIdSelector operationIdSelector,
        ISchemaIdSelector schemaIdSelector)
    {
        _apiVersioningOptions = apiVersioningOptions;
        _operationIdSelector = operationIdSelector;
        _schemaIdSelector = schemaIdSelector;
    }

    public void Configure(SwaggerGenOptions swaggerGenOptions)
    {
        swaggerGenOptions.SwaggerDoc(
            DefaultApiConfiguration.ApiName,
            new OpenApiInfo
            {
                Title = "Default API",
                Version = "Latest",
                Description = "All endpoints not defined under specific APIs"
            });

        swaggerGenOptions.CustomOperationIds(description => _operationIdSelector.OperationId(description, _apiVersioningOptions.Value));
        swaggerGenOptions.DocInclusionPredicate((name, api) =>
        {
            if (string.IsNullOrWhiteSpace(api.GroupName))
            {
                return false;
            }

            if (api.ActionDescriptor is ControllerActionDescriptor controllerActionDescriptor)
            {
                return controllerActionDescriptor.MethodInfo.HasMapToApiAttribute(name);
            }

            return false;
        });
        swaggerGenOptions.TagActionsBy(api => new[] { api.GroupName });
        swaggerGenOptions.OrderActionsBy(ActionOrderBy);
        swaggerGenOptions.SchemaFilter<EnumSchemaFilter>();
        swaggerGenOptions.CustomSchemaIds(_schemaIdSelector.SchemaId);
        swaggerGenOptions.SupportNonNullableReferenceTypes();
    }

    // see https://github.com/domaindrivendev/Swashbuckle.AspNetCore#change-operation-sort-order-eg-for-ui-sorting
    private static string ActionOrderBy(ApiDescription apiDesc)
        => $"{apiDesc.GroupName}_{apiDesc.ActionDescriptor.AttributeRouteInfo?.Template ?? apiDesc.ActionDescriptor.RouteValues["controller"]}_{apiDesc.ActionDescriptor.RouteValues["action"]}_{apiDesc.HttpMethod}";
}
