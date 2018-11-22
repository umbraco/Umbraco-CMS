using System;
using System.Web.Http.Filters;
using System.Web.Mvc;
using System.Web.Routing;
using System.Windows.Forms;
using Umbraco.Web.Models;

namespace Umbraco.Web.Mvc
{
    /// <summary>
    /// The interface that must be implemented for a controller to be designated to execute for route hijacking
    /// </summary>
    public interface IRenderMvcController : IRenderController
    {
        /// <summary>
        /// The default action to render the front-end view
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        ActionResult Index(RenderModel model);
    }
}