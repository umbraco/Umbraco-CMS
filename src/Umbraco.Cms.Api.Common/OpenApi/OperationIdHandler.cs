using Asp.Versioning;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Options;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Common.OpenApi;

// NOTE: Left unsealed on purpose, so it is extendable.
public class OperationIdHandler : IOperationIdHandler
{
    private readonly ApiVersioningOptions _apiVersioningOptions;

    public OperationIdHandler(IOptions<ApiVersioningOptions> apiVersioningOptions)
        => _apiVersioningOptions = apiVersioningOptions.Value;

    public bool CanHandle(ApiDescription apiDescription)
    {
        if (apiDescription.ActionDescriptor is not ControllerActionDescriptor controllerActionDescriptor)
        {
            return false;
        }

        return CanHandle(apiDescription, controllerActionDescriptor);
    }

    protected virtual bool CanHandle(ApiDescription apiDescription, ControllerActionDescriptor controllerActionDescriptor)
        => controllerActionDescriptor.ControllerTypeInfo.Namespace?.StartsWith("Umbraco.Cms.Api") is true;

    public virtual string Handle(ApiDescription apiDescription)
        => UmbracoOperationId(apiDescription);

    /// <summary>
    ///     Generates a unique operation identifier for a given API following Umbraco's operation id naming conventions.
    /// </summary>
    protected string UmbracoOperationId(ApiDescription apiDescription)
    {
        if (apiDescription.ActionDescriptor is not ControllerActionDescriptor controllerActionDescriptor)
        {
            throw new ArgumentException($"This handler operates only on {nameof(ControllerActionDescriptor)}.");
        }

        ApiVersion defaultVersion = _apiVersioningOptions.DefaultApiVersion;
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
            throw new InvalidOperationException(
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

        // Get map to version attribute
        string? version = null;

        var versionAttributeValue = controllerActionDescriptor.MethodInfo.GetMapToApiVersionAttributeValue();

        // We only want to add a version, if it is not the default one.
        if (string.Equals(versionAttributeValue, defaultVersion.ToString()) == false)
        {
            version = versionAttributeValue;
        }

        // Return the operation ID with the formatted http method verb in front, e.g. GetTrackedReferenceById
        return $"{httpMethod}{formattedOperationId.ToFirstUpper()}{version}";
    }
}
