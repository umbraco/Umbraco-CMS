using System.Linq;
using System.Web.Mvc;

namespace Umbraco.Web.Mvc
{
	/// <summary>
	/// When a ChildAction is executing and we want the ModelState from the Parent context to be merged in
	/// to help with validation, this filter can be used.
	/// </summary>
	/// <remarks>
	/// By default, this filter will only merge when an Http POST is detected but this can be modified in the ctor
	/// </remarks>
	public class MergeModelStateToChildActionAttribute : ActionFilterAttribute
	{
		private readonly string[] _verb;

		public MergeModelStateToChildActionAttribute()
			: this(HttpVerbs.Post)
		{

		}

		public MergeModelStateToChildActionAttribute(params HttpVerbs[] verb)
		{
			_verb = verb.Select(x => x.ToString().ToUpper()).ToArray();
		}

		public override void OnActionExecuting(ActionExecutingContext filterContext)
		{
			//check if the verb matches, if so merge the ModelState before the action is executed.
			if (_verb.Contains(filterContext.HttpContext.Request.HttpMethod))
			{
				if (filterContext.Controller.ControllerContext.IsChildAction)
				{
					filterContext.Controller.ViewData.ModelState.Merge(
						filterContext.Controller.ControllerContext.ParentActionViewContext.ViewData.ModelState);
				}
			}
			base.OnActionExecuting(filterContext);
		}
	}
}