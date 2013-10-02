using System.Web.Mvc;

namespace Umbraco.Core.Profiling
{
	public class ProfilingViewEngine: IViewEngine
	{
		private readonly IViewEngine _inner;
		private readonly string _name;

		public ProfilingViewEngine(IViewEngine inner)
		{
			_inner = inner;
			_name = inner.GetType().Name;
		}

		public ViewEngineResult FindPartialView(ControllerContext controllerContext, string partialViewName, bool useCache)
		{
			using (ProfilerResolver.Current.Profiler.Step(string.Format("{0}.FindPartialView, {1}, {2}", _name, partialViewName, useCache)))
			{
				return WrapResult(_inner.FindPartialView(controllerContext, partialViewName, useCache));
			}
		}

		public ViewEngineResult FindView(ControllerContext controllerContext, string viewName, string masterName, bool useCache)
		{
			using (ProfilerResolver.Current.Profiler.Step(string.Format("{0}.FindView, {1}, {2}, {3}", _name, viewName, masterName, useCache)))
			{
				return WrapResult(_inner.FindView(controllerContext, viewName, masterName, useCache));
			}
		}

		private static ViewEngineResult WrapResult(ViewEngineResult innerResult)
		{
			var profiledResult = innerResult.View != null ?
				                     new ViewEngineResult(new ProfilingView(innerResult.View), innerResult.ViewEngine) :
				                     new ViewEngineResult(innerResult.SearchedLocations);
			return profiledResult;
		}

		public void ReleaseView(ControllerContext controllerContext, IView view)
		{
			using (ProfilerResolver.Current.Profiler.Step(string.Format("{0}.ReleaseView, {1}", _name, view.GetType().Name)))
			{
				_inner.ReleaseView(controllerContext, view);
			}
		}
	}
}