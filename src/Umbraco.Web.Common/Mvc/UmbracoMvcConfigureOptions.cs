using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Web.Common.ModelBinders;
using Umbraco.Cms.Web.Common.Routing;
using Umbraco.Cms.Web.Common.Validators;

namespace Umbraco.Cms.Web.Common.Mvc;

/// <summary>
///     Options for globally configuring MVC for Umbraco
/// </summary>
/// <remarks>
///     We generally don't want to change the global MVC settings since we want to be unobtrusive as possible but some
///     global mods are needed - so long as they don't interfere with normal user usages of MVC.
/// </remarks>
public class UmbracoMvcConfigureOptions : IConfigureOptions<MvcOptions>
{
    private readonly GlobalSettings _globalSettings;

    public UmbracoMvcConfigureOptions(IOptions<GlobalSettings> globalSettings)
        => _globalSettings = globalSettings.Value;

    /// <inheritdoc />
    public void Configure(MvcOptions options)
    {
        options.ModelBinderProviders.Insert(0, new ContentModelBinderProvider());
        options.ModelValidatorProviders.Insert(0, new BypassRenderingModelValidatorProvider());
        options.ModelMetadataDetailsProviders.Add(new BypassRenderingModelValidationMetadataProvider());

        // these MVC options may be applied more than once; let's make sure we only add these conventions once.
        if (options.Conventions.Any(convention => convention is UmbracoBackofficeToken) is false)
        {
            // Replace the BackOfficeToken in routes.
            var backofficePath = Core.Constants.System.DefaultUmbracoPath.TrimStart(Core.Constants.CharArrays.TildeForwardSlash);
            options.Conventions.Add(new UmbracoBackofficeToken(Core.Constants.Web.AttributeRouting.BackOfficeToken, backofficePath));
        }
    }
}
