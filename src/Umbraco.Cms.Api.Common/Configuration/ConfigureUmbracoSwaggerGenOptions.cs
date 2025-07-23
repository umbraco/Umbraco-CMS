using Asp.Versioning;
using Microsoft.AspNetCore.Mvc.Abstractions;
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
    private readonly IOperationIdSelector _operationIdSelector;
    private readonly ISchemaIdSelector _schemaIdSelector;
    private readonly ISubTypesSelector _subTypesSelector;

    public ConfigureUmbracoSwaggerGenOptions(
        IOperationIdSelector operationIdSelector,
        ISchemaIdSelector schemaIdSelector,
        ISubTypesSelector subTypesSelector)
    {
        _operationIdSelector = operationIdSelector;
        _schemaIdSelector = schemaIdSelector;
        _subTypesSelector = subTypesSelector;
    }

    public void Configure(SwaggerGenOptions swaggerGenOptions)
    {
        swaggerGenOptions.SwaggerDoc(
            DefaultApiConfiguration.ApiName,
            new OpenApiInfo
            {
                Title = "Default API",
                Version = "Latest",
                Description = "All endpoints not defined under specific APIs",
            });

        swaggerGenOptions.CustomOperationIds(description => _operationIdSelector.OperationId(description));
        swaggerGenOptions.DocInclusionPredicate((name, api) =>
        {
            if (api.ActionDescriptor is ControllerActionDescriptor controllerActionDescriptor
                && controllerActionDescriptor.MethodInfo.HasMapToApiAttribute(name))
            {
                return true;
            }

            ApiVersionMetadata apiVersionMetadata = api.ActionDescriptor.GetApiVersionMetadata();
            return apiVersionMetadata.Name == name
                   || (string.IsNullOrEmpty(apiVersionMetadata.Name) && name == DefaultApiConfiguration.ApiName);
        });
        swaggerGenOptions.TagActionsBy(api => new[] { api.GroupName });
        swaggerGenOptions.OrderActionsBy(ActionOrderBy);
        swaggerGenOptions.SchemaFilter<EnumSchemaFilter>();
        swaggerGenOptions.CustomSchemaIds(_schemaIdSelector.SchemaId);
        swaggerGenOptions.SelectSubTypesUsing(_subTypesSelector.SubTypes);
        swaggerGenOptions.SupportNonNullableReferenceTypes();
    }

    // see https://github.com/domaindrivendev/Swashbuckle.AspNetCore#change-operation-sort-order-eg-for-ui-sorting
    private static string ActionOrderBy(ApiDescription apiDesc)
        => $"{apiDesc.GroupName}_{apiDesc.ActionDescriptor.AttributeRouteInfo?.Template ?? apiDesc.ActionDescriptor.RouteValues["controller"]}_{(apiDesc.ActionDescriptor.RouteValues.TryGetValue("action", out var action) ? action : null)}_{apiDesc.HttpMethod}";
}
