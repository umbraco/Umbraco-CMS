using System.Web.Mvc;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Services;
using Umbraco.Web.Models;

namespace Umbraco.Web.Mvc
{

    /// <summary>
    /// The default front-end rendering controller.
    /// </summary>
    public class RenderMvcController : UmbracoRenderController, IRenderMvcController
    {
        // TODO: in vNext remove this and the default will be UmbracoRenderController

        public RenderMvcController()
        {         
        }

        public RenderMvcController(IGlobalSettings globalSettings, IUmbracoContextAccessor umbracoContextAccessor, ServiceContext services, AppCaches appCaches, IProfilingLogger profilingLogger, UmbracoHelper umbracoHelper)
            : base(globalSettings, umbracoContextAccessor, services, appCaches, profilingLogger, umbracoHelper)
        {
        }

        /// <summary>
        /// The default action to render the front-end view.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [RenderIndexActionSelector]
        public virtual ActionResult Index(ContentModel model)
        {
            return CurrentTemplate(model);
        }
    }
}
