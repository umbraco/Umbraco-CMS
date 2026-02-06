using System.Reflection;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Umbraco.Cms.Api.Common.OpenApi;

namespace Umbraco.Cms.Api.Common.Configuration;

/// <summary>
/// Configures the OpenAPI options for the Default API.
/// </summary>
public class ConfigureDefaultApiOptions : ConfigureUmbracoOpenApiOptionsBase
{
    /// <inheritdoc />
    protected override string ApiName => DefaultApiConfiguration.ApiName;

    /// <inheritdoc />
    protected override string ApiTitle => "Default API";

    /// <inheritdoc />
    protected override string ApiVersion => "Latest";

    /// <inheritdoc />
    protected override string ApiDescription => "All endpoints not defined under specific APIs";

    /// <inheritdoc />
    protected override bool ShouldInclude(ApiDescription apiDescription)
    {
        // Exclude controllers with ExcludeFromDefaultOpenApiDocumentAttribute
        if (apiDescription.ActionDescriptor is ControllerActionDescriptor controllerActionDescriptor
            && controllerActionDescriptor.ControllerTypeInfo.GetCustomAttribute<ExcludeFromDefaultOpenApiDocumentAttribute>() is not null)
        {
            return false;
        }

        // Include if explicitly mapped to this document
        if (base.ShouldInclude(apiDescription))
        {
            return true;
        }

        // Include endpoints not explicitly assigned to another document
        ApiVersionMetadata apiVersionMetadata = apiDescription.ActionDescriptor.GetApiVersionMetadata();
        return string.IsNullOrEmpty(apiVersionMetadata.Name);
    }
}
