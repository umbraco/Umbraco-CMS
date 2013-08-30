using System.Web.Mvc;

namespace Umbraco.Web.Mvc
{
    /// <summary>
    /// This attribute can be used for when child actions execute and will automatically merge in the viewdata from the parent context to the 
    /// child action result.
    /// </summary>
    /// <remarks>
    /// This will retain any custom viewdata put into the child viewdata if the same key persists in the parent context's view data. You can always still
    /// access the parent's view data normally. 
    /// This just simplifies working with ChildActions and view data.
    /// 
    /// NOTE: This does not mean that the parent context's view data will be merged before the action executes, if you need access to the parent context's view
    /// data during controller execution you can access it normally.
    /// </remarks>
    public class MergeParentContextViewDataAttribute : ActionFilterAttribute
    {
        /// <summary>
        /// Merge in the parent context's view data if this is a child action when the result is being executed
        /// </summary>
        /// <param name="filterContext"></param>
        public override void OnResultExecuting(ResultExecutingContext filterContext)
        {
            if (filterContext.IsChildAction)
            {
                if (filterContext.ParentActionViewContext != null && filterContext.ParentActionViewContext.ViewData != null)
                {
                    filterContext.Controller.ViewData.MergeViewDataFrom(filterContext.ParentActionViewContext.ViewData);
                }
            }

            base.OnResultExecuting(filterContext);
        }
    }
}