using System;
using System.Web.Mvc;
using Umbraco.Core.Cache;
using Umbraco.Core.Composing;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Services;
using Umbraco.Web.Models;

namespace Umbraco.Web.Mvc
{
    /// <summary>
    /// The interface that must be implemented for a controller to be designated to execute for route hijacking
    /// </summary>
    public interface IRenderMvcController : IRenderController
    {
        // TODO: In vNext remove this and the default will be IRenderController

        /// <summary>
        /// The default action to render the front-end view
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        ActionResult Index(ContentModel model);
    }
}
