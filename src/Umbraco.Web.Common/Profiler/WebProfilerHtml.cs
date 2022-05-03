using Microsoft.AspNetCore.Http;
using StackExchange.Profiling;
using StackExchange.Profiling.Internal;
using Umbraco.Cms.Core.Logging;

namespace Umbraco.Cms.Web.Common.Profiler;

public class WebProfilerHtml : IProfilerHtml
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public WebProfilerHtml(IHttpContextAccessor httpContextAccessor) =>

        // create our own provider, which can provide a profiler even during boot
        _httpContextAccessor = httpContextAccessor;

    /// <inheritdoc />
    /// <remarks>
    ///     Normally we would call MiniProfiler.Current.RenderIncludes(...), but because the requeststate is not set, this
    ///     method does not work.
    ///     We fake the requestIds from the RequestState here.
    /// </remarks>
    public string Render()
    {
        MiniProfiler? profiler = MiniProfiler.Current;
        if (profiler == null)
        {
            return string.Empty;
        }

        HttpContext? context = _httpContextAccessor.HttpContext;

        var path = (profiler.Options as MiniProfilerOptions)?.RouteBasePath.Value.EnsureTrailingSlash();

        var result = StackExchange.Profiling.Internal.Render.Includes(
            profiler,
            context is not null ? context.Request.PathBase + path : null,
            true,
            new List<Guid> { profiler.Id },
            RenderPosition.Right,
            profiler.Options.PopupShowTrivial,
            profiler.Options.PopupShowTimeWithChildren,
            profiler.Options.PopupMaxTracesToShow,
            profiler.Options.ShowControls,
            profiler.Options.PopupStartHidden);

        return result;
    }
}
