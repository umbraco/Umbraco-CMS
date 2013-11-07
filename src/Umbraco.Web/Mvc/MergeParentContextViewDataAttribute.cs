using System.Linq;
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
    /// 
    /// NOTE: This recursively merges in all ParentActionViewContext ancestry in case there's child actions inside of child actions.
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
                MergeCurrentParent(filterContext.Controller, filterContext.ParentActionViewContext);
            }

            base.OnResultExecuting(filterContext);
        }

        /// <summary>
        /// Recursively merges in each parent view context into the target
        /// </summary>
        /// <param name="target"></param>
        /// <param name="currentParent"></param>
        private static void MergeCurrentParent(ControllerBase target, ViewContext currentParent)
        {
            if (currentParent != null && currentParent.ViewData != null && currentParent.ViewData.Any())
            {
                target.ViewData.MergeViewDataFrom(currentParent.ViewData);

                //Recurse!
                if (currentParent.IsChildAction)
                {
                    MergeCurrentParent(target, currentParent.ParentActionViewContext);    
                }
                
            }
        }
    }
}