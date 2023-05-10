using Asp.Versioning;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Umbraco.Cms.Api.Common.OpenApi;
using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Extensions;
using OperationIdRegexes = Umbraco.Cms.Api.Common.OpenApi.OperationIdRegexes;

namespace Umbraco.Cms.Api.Common.Configuration;

public class ConfigureUmbracoSwaggerGenOptions : IConfigureOptions<SwaggerGenOptions>
{
    private readonly IOptions<ApiVersioningOptions> _apiVersioningOptions;

    public ConfigureUmbracoSwaggerGenOptions(IOptions<ApiVersioningOptions> apiVersioningOptions)
        => _apiVersioningOptions = apiVersioningOptions;

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

        swaggerGenOptions.CustomOperationIds(description =>
            CustomOperationId(description, _apiVersioningOptions.Value));

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
        swaggerGenOptions.DocumentFilter<MimeTypeDocumentFilter>();
        swaggerGenOptions.SchemaFilter<EnumSchemaFilter>();
        swaggerGenOptions.CustomSchemaIds(SchemaIdGenerator.Generate);
        swaggerGenOptions.SupportNonNullableReferenceTypes();
        swaggerGenOptions.UseOneOfForPolymorphism();
        swaggerGenOptions.UseAllOfForInheritance();
        var cachedApiElementNamespace = typeof(ApiElement).Namespace ?? string.Empty;
        swaggerGenOptions.SelectDiscriminatorNameUsing(type =>
        {
            if (type.Namespace != null && type.Namespace.StartsWith(cachedApiElementNamespace))
            {
                // We do not show type on delivery, as it is read only.
                return null;
            }

            if (type.GetInterfaces().Any())
            {
                return "$type";
            }

            return null;
        });
        swaggerGenOptions.SelectDiscriminatorValueUsing(x => x.Name);
    }

    private static string CustomOperationId(ApiDescription api, ApiVersioningOptions apiVersioningOptions)
    {
        ApiVersion defaultVersion = apiVersioningOptions.DefaultApiVersion;
        var httpMethod = api.HttpMethod?.ToLower().ToFirstUpper() ?? "Get";

        // if the route info "Name" is supplied we'll use this explicitly as the operation ID
        // - usage example: [HttpGet("my-api/route}", Name = "MyCustomRoute")]
        if (string.IsNullOrWhiteSpace(api.ActionDescriptor.AttributeRouteInfo?.Name) == false)
        {
            var explicitOperationId = api.ActionDescriptor.AttributeRouteInfo!.Name;
            return explicitOperationId.InvariantStartsWith(httpMethod)
                ? explicitOperationId
                : $"{httpMethod}{explicitOperationId}";
        }

        var relativePath = api.RelativePath;

        if (string.IsNullOrWhiteSpace(relativePath))
        {
            throw new Exception(
                $"There is no relative path for controller action {api.ActionDescriptor.RouteValues["controller"]}");
        }

        // Remove the prefixed base path with version, e.g. /umbraco/management/api/v1/tracked-reference/{id} => tracked-reference/{id}
        var unprefixedRelativePath = OperationIdRegexes
            .VersionPrefixRegex()
            .Replace(relativePath, string.Empty);

        // Remove template placeholders, e.g. tracked-reference/{id} => tracked-reference/Id
        var formattedOperationId = OperationIdRegexes
            .TemplatePlaceholdersRegex()
            .Replace(unprefixedRelativePath, m => $"By{m.Groups[1].Value.ToFirstUpper()}");

        // Remove dashes (-) and slashes (/) and convert the following letter to uppercase with
        // the word "By" in front, e.g. tracked-reference/Id => TrackedReferenceById
        formattedOperationId = OperationIdRegexes
            .ToCamelCaseRegex()
            .Replace(formattedOperationId, m => m.Groups[1].Value.ToUpper());

        //Get map to version attribute
        string? version = null;

        if (api.ActionDescriptor is ControllerActionDescriptor controllerActionDescriptor)
        {
            var versionAttributeValue = controllerActionDescriptor.MethodInfo.GetMapToApiVersionAttributeValue();

            // We only wanna add a version, if it is not the default one.
            if (string.Equals(versionAttributeValue, defaultVersion.ToString()) == false)
            {
                version = versionAttributeValue;
            }
        }

        // Return the operation ID with the formatted http method verb in front, e.g. GetTrackedReferenceById
        return $"{httpMethod}{formattedOperationId.ToFirstUpper()}{version}";
    }

    // see https://github.com/domaindrivendev/Swashbuckle.AspNetCore#change-operation-sort-order-eg-for-ui-sorting
    private static string ActionOrderBy(ApiDescription apiDesc)
        => $"{apiDesc.GroupName}_{apiDesc.ActionDescriptor.AttributeRouteInfo?.Template ?? apiDesc.ActionDescriptor.RouteValues["controller"]}_{apiDesc.ActionDescriptor.RouteValues["action"]}_{apiDesc.HttpMethod}";

}
