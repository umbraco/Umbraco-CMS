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
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            //ignore anything that is not IRenderController
            if ((filterContext.Controller is IRenderController) == false)
                return;

            var viewCtx = new ViewContext(
                filterContext.Controller.ControllerContext, 
                new DummyView(), 
                filterContext.Controller.ViewData, filterContext.Controller.TempData, 
                new StringWriter());

            //set the special data token
            filterContext.RequestContext.RouteData.DataTokens[Constants.DataTokenCurrentViewContext] = viewCtx;
        }

        private class DummyView : IView
        {
            public void Render(ViewContext viewContext, TextWriter writer)
            {
            }
        }
    }
}