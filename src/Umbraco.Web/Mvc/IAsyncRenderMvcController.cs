using System.Threading.Tasks;
using System.Web.Mvc;
using Umbraco.Web.Models;

namespace Umbraco.Web.Mvc
{
    /// <summary>
    /// The interface that can be implemented for an async controller to be designated to execute for route hijacking
    /// </summary>
    public interface IAsyncRenderMvcController : IController
    {
        /// <summary>
        /// The default action to render the front-end view
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        /// <remarks>
        /// Ideally we could have made a marker interface as a base interface for both the IRenderMvcController and IAsyncRenderMvcController
        /// however that would require breaking changes in other classes like DefaultRenderMvcControllerResolver since the result would have 
        /// to be changed. Instead we are hiding the underlying interface method.
        /// </remarks>
        Task<ActionResult> Index(RenderModel model);
    }
}