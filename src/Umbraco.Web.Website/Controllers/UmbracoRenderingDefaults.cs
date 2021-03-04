using System;
using Umbraco.Cms.Web.Common.Controllers;

namespace Umbraco.Cms.Web.Website.Controllers
{
    /// <summary>
    /// The defaults used for rendering Umbraco front-end pages
    /// </summary>
    public class UmbracoRenderingDefaults : IUmbracoRenderingDefaults
    {
        /// <inheritdoc/>
        public Type DefaultControllerType => typeof(RenderController);
    }
}
