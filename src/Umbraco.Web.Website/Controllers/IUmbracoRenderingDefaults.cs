using System;

namespace Umbraco.Cms.Web.Website.Controllers
{
    /// <summary>
    /// The defaults used for rendering Umbraco front-end pages
    /// </summary>
    public interface IUmbracoRenderingDefaults
    {
        /// <summary>
        /// Gets the default umbraco render controller type
        /// </summary>
        Type DefaultControllerType { get; }
    }
}
