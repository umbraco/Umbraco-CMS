using System.Web.Mvc;
using Umbraco.Web.Models;

namespace Umbraco.Web.Mvc
{
    /// <summary>
    /// The interface that must be implemented for a controller to be designated to execute for route hijacking
    /// </summary>
    public interface IRenderMvcController : IController
    {
        /// <summary>
        /// The default action to render the front-end view
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        ActionResult Index(RenderModel model);
    }
}