using System.IO;
using System.Web.Mvc;

namespace Umbraco.Web.Mvc
{
    /// <summary>
    /// This is a special filter which is required for the RTE to be able to render Partial View Macros that 
    /// contain forms when the RTE value is resolved outside of an MVC view being rendered
    /// </summary>
    /// <remarks>
    /// The entire way that we support partial view macros that contain forms isn't really great, these forms
    /// need to be executed as ChildActions so that the ModelState,ViewData,TempData get merged into that action
    /// so the form can show errors, viewdata, etc...
    /// Under normal circumstances, macros will be rendered after a ViewContext is created but in some cases 
    /// developers will resolve the RTE value in the controller, in this case the Form won't be rendered correctly
    /// with merged ModelState from the controller because the special DataToken hasn't been set yet (which is 
    /// normally done in the UmbracoViewPageOfModel when a real ViewContext is available.
    /// So we need to detect if the currently rendering controller is IRenderController and if so we'll ensure that
    /// this DataToken exists before the action executes in case the developer resolves an RTE value that contains
    /// a partial view macro form.
    /// </remarks>
    internal class EnsurePartialViewMacroViewContextFilterAttribute : ActionFilterAttribute
    {
        /// <summary>
        /// Ensures the custom ViewContext datatoken is set before the RenderController action is invoked,
        /// this ensures that any calls to GetPropertyValue with regards to RTE or Grid editors can still
        /// render any PartialViewMacro with a form and maintain ModelState
        /// </summary>
        /// <param name="filterContext"></param>
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            //ignore anything that is not IRenderController
            if ((filterContext.Controller is IRenderController) == false && filterContext.IsChildAction == false)
                return;

            SetViewContext(filterContext);
        }

        /// <summary>
        /// Ensures that the custom ViewContext datatoken is set after the RenderController action is invoked,
        /// this ensures that any custom ModelState that may have been added in the RenderController itself is 
        /// passed onwards in case it is required when rendering a PartialViewMacro with a form
        /// </summary>
        /// <param name="filterContext">The filter context.</param>
        public override void OnResultExecuting(ResultExecutingContext filterContext)
        {
            //ignore anything that is not IRenderController
            if ((filterContext.Controller is IRenderController) == false && filterContext.IsChildAction == false)
                return;

            SetViewContext(filterContext);
        }

        private void SetViewContext(ControllerContext controllerContext)
        {
            var viewCtx = new ViewContext(
                controllerContext,
                new DummyView(),
                controllerContext.Controller.ViewData, controllerContext.Controller.TempData,
                new StringWriter());

            //set the special data token
            controllerContext.RequestContext.RouteData.DataTokens[Constants.DataTokenCurrentViewContext] = viewCtx;
        }

        private class DummyView : IView
        {
            public void Render(ViewContext viewContext, TextWriter writer)
            {
            }
        }
    }
}