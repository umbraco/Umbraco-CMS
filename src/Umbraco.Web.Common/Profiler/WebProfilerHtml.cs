using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using StackExchange.Profiling;
using StackExchange.Profiling.Internal;
using Umbraco.Cms.Core.Logging;

namespace Umbraco.Cms.Web.Common.Profiler
{
    public class WebProfilerHtml : IProfilerHtml
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public WebProfilerHtml(IHttpContextAccessor httpContextAccessor)
        {
            // create our own provider, which can provide a profiler even during boot
            _httpContextAccessor = httpContextAccessor;
        }

        /// <inheritdoc />
        /// <remarks>
        /// Normally we would call MiniProfiler.Current.RenderIncludes(...), but because the requeststate is not set, this method does not work.
        /// We fake the requestIds from the RequestState here.
        /// </remarks>
        public string Render()
        {

            var profiler = MiniProfiler.Current;
            if (profiler == null) return string.Empty;

            var context = _httpContextAccessor.HttpContext;

            var path = (profiler.Options as MiniProfilerOptions)?.RouteBasePath.Value.EnsureTrailingSlash();

            var result = StackExchange.Profiling.Internal.Render.Includes(
                profiler,
                path: context is not null ? context.Request.PathBase + path : null,
                isAuthorized: true,
                requestIDs: new List<Guid> { profiler.Id },
                position: RenderPosition.Right,
                showTrivial: profiler.Options.PopupShowTrivial,
                showTimeWithChildren: profiler.Options.PopupShowTimeWithChildren,
                maxTracesToShow: profiler.Options.PopupMaxTracesToShow,
                showControls: profiler.Options.ShowControls,
                startHidden: profiler.Options.PopupStartHidden);

            return result;
        }
    }
}
