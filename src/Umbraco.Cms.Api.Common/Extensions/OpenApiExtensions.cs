using Asp.Versioning;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.OpenApi;
using Umbraco.Cms.Api.Common.Configuration;

namespace Umbraco.Extensions;

public static class OpenApiExtensions
{
    /// <summary>
    /// Configures the default settings and transformers for all Umbraco APIs.
    /// </summary>
    /// <param name="options">The <see cref="OpenApiOptions"/> instance to configure.</param>
    /// <param name="name">The name of the API being configured.</param>
    /// <returns>The configured <see cref="OpenApiOptions"/> instance.</returns>
    public static OpenApiOptions ConfigureUmbracoDefaultApiOptions(this OpenApiOptions options, string name)
    {
        options.ShouldInclude = apiDescription => ShouldInclude(apiDescription, name);

        return options;
    }

    private static bool ShouldInclude(this ApiDescription apiDescription, string apiName)
    {
        if (apiDescription.ActionDescriptor is ControllerActionDescriptor controllerActionDescriptor && controllerActionDescriptor.HasMapToApiAttribute(apiName))
        {
            return true;
        }

        ApiVersionMetadata apiVersionMetadata = apiDescription.ActionDescriptor.GetApiVersionMetadata();
        return apiVersionMetadata.Name == apiName
               || (string.IsNullOrEmpty(apiVersionMetadata.Name) && apiName == DefaultApiConfiguration.ApiName);
    }
}
