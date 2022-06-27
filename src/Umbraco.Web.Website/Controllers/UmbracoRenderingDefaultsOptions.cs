using Umbraco.Cms.Web.Common.Controllers;

namespace Umbraco.Cms.Web.Website.Controllers;

/// <summary>
///     The defaults used for rendering Umbraco front-end pages
/// </summary>
public class UmbracoRenderingDefaultsOptions
{
    /// <summary>
    ///     Gets the default umbraco render controller type
    /// </summary>
    public Type DefaultControllerType { get; set; } = typeof(RenderController);
}
