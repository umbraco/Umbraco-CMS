using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Umbraco.Cms.Core.Logging;

namespace Umbraco.Cms.Web.Website.ViewEngines;

public class ProfilingViewEngine : IViewEngine
{
    private readonly string _name;
    private readonly IProfiler _profiler;
    internal readonly IViewEngine Inner;

    public ProfilingViewEngine(IViewEngine inner, IProfiler profiler)
    {
        Inner = inner;
        _profiler = profiler;
        _name = inner.GetType().Name;
    }

    public ViewEngineResult FindView(ActionContext context, string viewName, bool isMainPage)
    {
        using (_profiler.Step(string.Format("{0}.FindView, {1}, {2}", _name, viewName, isMainPage)))
        {
            return WrapResult(Inner.FindView(context, viewName, isMainPage));
        }
    }

    public ViewEngineResult GetView(string? executingFilePath, string viewPath, bool isMainPage)
    {
        using (_profiler.Step(string.Format("{0}.GetView, {1}, {2}, {3}", _name, executingFilePath, viewPath, isMainPage)))
        {
            return Inner.GetView(executingFilePath, viewPath, isMainPage);
        }
    }

    private static ViewEngineResult WrapResult(ViewEngineResult innerResult)
    {
        ViewEngineResult profiledResult = innerResult.View is null
            ? ViewEngineResult.NotFound(innerResult.ViewName, innerResult.SearchedLocations)
            : ViewEngineResult.Found(innerResult.ViewName, innerResult.View);
        return profiledResult;
    }
}
