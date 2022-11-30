using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Umbraco.Cms.Web.Common.Constants;
using Umbraco.Cms.Web.Common.Controllers;

namespace Umbraco.Cms.Web.Common.Filters;

/// <summary>
///     This is a special filter which is required for the RTE to be able to render Partial View Macros that
///     contain forms when the RTE value is resolved outside of an MVC view being rendered
/// </summary>
/// <remarks>
///     The entire way that we support partial view macros that contain forms isn't really great, these forms
///     need to be executed as ChildActions so that the ModelState,ViewData,TempData get merged into that action
///     so the form can show errors, viewdata, etc...
///     Under normal circumstances, macros will be rendered after a ViewContext is created but in some cases
///     developers will resolve the RTE value in the controller, in this case the Form won't be rendered correctly
///     with merged ModelState from the controller because the special DataToken hasn't been set yet (which is
///     normally done in the UmbracoViewPageOfModel when a real ViewContext is available.
///     So we need to detect if the currently rendering controller is IRenderController and if so we'll ensure that
///     this DataToken exists before the action executes in case the developer resolves an RTE value that contains
///     a partial view macro form.
/// </remarks>
public class EnsurePartialViewMacroViewContextFilterAttribute : ActionFilterAttribute
{
    /// <summary>
    ///     Ensures the custom ViewContext datatoken is set before the RenderController action is invoked,
    ///     this ensures that any calls to GetPropertyValue with regards to RTE or Grid editors can still
    ///     render any PartialViewMacro with a form and maintain ModelState
    /// </summary>
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (!(context.Controller is Controller controller))
        {
            return;
        }

        // ignore anything that is not IRenderController
        if (!(controller is IRenderController))
        {
            return;
        }

        SetViewContext(context, controller);
    }

    /// <summary>
    ///     Ensures that the custom ViewContext datatoken is set after the RenderController action is invoked,
    ///     this ensures that any custom ModelState that may have been added in the RenderController itself is
    ///     passed onwards in case it is required when rendering a PartialViewMacro with a form
    /// </summary>
    /// <param name="context">The filter context.</param>
    public override void OnResultExecuting(ResultExecutingContext context)
    {
        if (!(context.Controller is Controller controller))
        {
            return;
        }

        // ignore anything that is not IRenderController
        if (!(controller is IRenderController))
        {
            return;
        }

        SetViewContext(context, controller);
    }

    private void SetViewContext(ActionContext context, Controller controller)
    {
        var viewCtx = new ViewContext(
            context,
            new DummyView(),
            controller.ViewData,
            controller.TempData,
            new StringWriter(),
            new HtmlHelperOptions());

        // set the special data token
        context.RouteData.DataTokens[ViewConstants.DataTokenCurrentViewContext] = viewCtx;
    }

    private class DummyView : IView
    {
        public string Path { get; } = null!;

        public Task RenderAsync(ViewContext context) => Task.CompletedTask;
    }
}
