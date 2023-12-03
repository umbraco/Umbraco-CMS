using Asp.Versioning;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Umbraco.Cms.Api.Common.Attributes;
using Umbraco.Cms.Api.Common.OpenApi;
using Umbraco.Cms.Core.DependencyInjection;
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
        swaggerGenOptions.DocInclusionPredicate((documentGroupName, apiDescription) =>
        {
            if (apiDescription.ActionDescriptor is ControllerActionDescriptor controllerActionDescriptor)
            {
                MapToApiAttribute? mapToApiAttribute = controllerActionDescriptor.MethodInfo.GetMapToApiAttribute();

                if (mapToApiAttribute != null)
                {
                    return mapToApiAttribute.ApiName == documentGroupName;
                }
            }

            if (string.IsNullOrWhiteSpace(apiDescription.GroupName))
            {
                return documentGroupName == DefaultApiConfiguration.ApiName;
            }

            if (documentGroupName == apiDescription.GroupName)
            {
                return true;
            }

            IOptions<SwaggerGenOptions> swaggerOptions = StaticServiceProvider.Instance.GetRequiredService<IOptions<SwaggerGenOptions>>();
            if (swaggerOptions.Value.SwaggerGeneratorOptions.SwaggerDocs.ContainsKey(apiDescription.GroupName))
            {
                return false;
            }
            else
            {
                return documentGroupName == DefaultApiConfiguration.ApiName;
            }
        });

        swaggerGenOptions.TagActionsBy(TagActionsBy);
        swaggerGenOptions.OrderActionsBy(ActionOrderBy);
        swaggerGenOptions.SchemaFilter<EnumSchemaFilter>();
        swaggerGenOptions.CustomSchemaIds(_schemaIdSelector.SchemaId);
        swaggerGenOptions.SupportNonNullableReferenceTypes();
    }

    private static List<string> TagActionsBy(ApiDescription apiDescription)
    {
        if (apiDescription.GroupName != null)
        {
            return new List<string> { apiDescription.GroupName };
        }

        return new List<string>();
    }

    // see https://github.com/domaindrivendev/Swashbuckle.AspNetCore#change-operation-sort-order-eg-for-ui-sorting
    private static string ActionOrderBy(ApiDescription apiDescription)
    {
        var orderBySections = new List<string>();

        if (apiDescription.GroupName != null)
        {
            orderBySections.Add(apiDescription.GroupName);
        }
        else
        {
            orderBySections.Add(DefaultApiConfiguration.ApiName);
        }

        if (apiDescription.ActionDescriptor.AttributeRouteInfo?.Template != null)
        {
            orderBySections.Add(apiDescription.ActionDescriptor.AttributeRouteInfo.Template);
        }
        else if (apiDescription.ActionDescriptor.RouteValues.TryGetValue("controller", out string? controllerValue) && !string.IsNullOrEmpty(controllerValue))
        {
            orderBySections.Add(controllerValue);
        }

        if (apiDescription.ActionDescriptor.RouteValues.TryGetValue("action", out string? actionValue) && !string.IsNullOrEmpty(actionValue))
        {
            orderBySections.Add(actionValue);
        }

        if (apiDescription.HttpMethod != null)
        {
            orderBySections.Add(apiDescription.HttpMethod);
        }

        return string.Join('_', orderBySections);
    }
}
