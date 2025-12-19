using Asp.Versioning;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Common.OpenApi;

/// <summary>
/// Transforms OpenAPI operation IDs using Umbraco's naming conventions.
/// </summary>
/// <remarks>
/// This transformer can be registered manually for custom OpenAPI configurations.
/// </remarks>
public class UmbracoOperationIdTransformer : IOpenApiOperationTransformer
{
    /// <summary>
    /// Transforms the specified OpenAPI operation, setting its operation ID using a custom selector.
    /// </summary>
    /// <param name="operation">The <see cref="OpenApiOperation"/> to modify.</param>
    /// <param name="context">The <see cref="OpenApiOperationTransformerContext"/> associated with the <paramref name="operation"/>.</param>
    /// <param name="cancellationToken">The cancellation token to use.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    public Task TransformAsync(
        OpenApiOperation operation,
        OpenApiOperationTransformerContext context,
        CancellationToken cancellationToken)
    {
        operation.OperationId = GenerateOperationId(context);
        return Task.CompletedTask;
    }

    private static string GenerateOperationId(OpenApiOperationTransformerContext context)
    {
        ApiDescription apiDescription = context.Description;
        if (apiDescription.ActionDescriptor is not ControllerActionDescriptor controllerActionDescriptor)
        {
            throw new ArgumentException($"This handler operates only on {nameof(ControllerActionDescriptor)}.");
        }

        ApiVersion defaultVersion = context.ApplicationServices.GetRequiredService<IOptions<ApiVersioningOptions>>().Value.DefaultApiVersion;
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
