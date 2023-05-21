using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Logging;

namespace Umbraco.Cms.Web.Website.ViewEngines;

/// <summary>
///     Wraps all view engines with a <see cref="ProfilingViewEngine" />
/// </summary>
public class ProfilingViewEngineWrapperMvcViewOptionsSetup : IConfigureOptions<MvcViewOptions>
{
    private readonly IProfiler _profiler;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ProfilingViewEngineWrapperMvcViewOptionsSetup" /> class.
    /// </summary>
    /// <param name="profiler">The <see cref="IProfiler" /></param>
    public ProfilingViewEngineWrapperMvcViewOptionsSetup(IProfiler profiler) =>
        _profiler = profiler ?? throw new ArgumentNullException(nameof(profiler));

    /// <inheritdoc />
    public void Configure(MvcViewOptions options)
    {
        if (options == null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        WrapViewEngines(options.ViewEngines);
    }

    private void WrapViewEngines(IList<IViewEngine> viewEngines)
    {
        if (viewEngines.Count == 0)
        {
            return;
        }

        var originalEngines = viewEngines.ToList();
        viewEngines.Clear();
        foreach (IViewEngine engine in originalEngines)
        {
            IViewEngine wrappedEngine =
                engine is ProfilingViewEngine ? engine : new ProfilingViewEngine(engine, _profiler);
            viewEngines.Add(wrappedEngine);
        }
    }
}
