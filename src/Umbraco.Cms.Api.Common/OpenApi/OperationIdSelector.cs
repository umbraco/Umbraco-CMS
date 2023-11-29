using Asp.Versioning;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Common.OpenApi;

public class OperationIdSelector : IOperationIdSelector
{
    public virtual string? OperationId(ApiDescription apiDescription, ApiVersioningOptions apiVersioningOptions)
    {
        if (apiDescription.ActionDescriptor is not ControllerActionDescriptor controllerActionDescriptor
            || controllerActionDescriptor.ControllerTypeInfo.Namespace?.StartsWith("Umbraco.Cms.Api") is not true)
        {
            return null;
        }

        return UmbracoOperationId(apiDescription, apiVersioningOptions);
    }

    protected string? UmbracoOperationId(ApiDescription apiDescription, ApiVersioningOptions apiVersioningOptions)
    {
        if (apiDescription.ActionDescriptor is not ControllerActionDescriptor controllerActionDescriptor)
        {
            return null;
        }

        ApiVersion defaultVersion = apiVersioningOptions.DefaultApiVersion;
        var httpMethod = apiDescription.HttpMethod?.ToLower().ToFirstUpper() ?? "Get";

        // if the route info "Name" is supplied we'll use this explicitly as the operation ID
        // - usage example: [HttpGet("my-api/route}", Name = "MyCustomRoute")]
        if (string.IsNullOrWhiteSpace(apiDescription.ActionDescriptor.AttributeRouteInfo?.Name) == false)
        {
            var explicitOperationId = apiDescription.ActionDescriptor.AttributeRouteInfo!.Name;
            return explicitOperationId.InvariantStartsWith(httpMethod)
                ? explicitOperationId
                : $"{httpMethod}{explicitOperationId}";
        }

        var relativePath = apiDescription.RelativePath;

        if (string.IsNullOrWhiteSpace(relativePath))
        {
            throw new Exception(
                $"There is no relative path for controller action {apiDescription.ActionDescriptor.RouteValues["controller"]}");
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

        var versionAttributeValue = controllerActionDescriptor.MethodInfo.GetMapToApiVersionAttributeValue();

        // We only wanna add a version, if it is not the default one.
        if (string.Equals(versionAttributeValue, defaultVersion.ToString()) == false)
        {
            version = versionAttributeValue;
        }

        // Return the operation ID with the formatted http method verb in front, e.g. GetTrackedReferenceById
        return $"{httpMethod}{formattedOperationId.ToFirstUpper()}{version}";
    }
}
