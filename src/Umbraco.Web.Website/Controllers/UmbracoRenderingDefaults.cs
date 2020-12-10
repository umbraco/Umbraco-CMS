using System;
using Umbraco.Web.Common.Controllers;

namespace Umbraco.Web.Website.Controllers
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
