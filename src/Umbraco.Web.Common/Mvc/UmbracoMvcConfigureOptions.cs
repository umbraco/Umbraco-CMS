using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Web.Common.Filters;
using Umbraco.Cms.Web.Common.ModelBinders;

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
    /// <inheritdoc />
    public void Configure(MvcOptions options)
    {
        options.ModelBinderProviders.Insert(0, new ContentModelBinderProvider());
        options.Filters.Insert(0, new EnsurePartialViewMacroViewContextFilterAttribute());
    }
}
