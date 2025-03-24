using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Umbraco.Cms.Web.Common.Mvc;

/// <summary>
///     Options for globally configuring MVC for Umbraco
/// </summary>
/// <remarks>
///     We generally don't want to change the global MVC settings since we want to be unobtrusive as possible but some
///     global mods are needed - so long as they don't interfere with normal user usages of MVC.
/// </remarks>
public class ManagementApiMvcConfigurationOptions : IConfigureOptions<MvcOptions>
{
    /// <inheritdoc />
    public void Configure(MvcOptions options)
    {
        options.ModelMetadataDetailsProviders.Add(new ManagementApiSystemTextJsonValidationMetadataProvider(JsonNamingPolicy.CamelCase));
    }
}
