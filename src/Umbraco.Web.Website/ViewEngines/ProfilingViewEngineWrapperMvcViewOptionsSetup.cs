using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.Extensions.Options;
using Umbraco.Core.Logging;

namespace Umbraco.Web.Website.ViewEngines
{
    public class ProfilingViewEngineWrapperMvcViewOptionsSetup : IConfigureOptions<MvcViewOptions>
    {
        private readonly IProfiler _profiler;

        public ProfilingViewEngineWrapperMvcViewOptionsSetup(IProfiler profiler)
        {
            _profiler = profiler ?? throw new ArgumentNullException(nameof(profiler));
        }

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
            if (viewEngines == null || viewEngines.Count == 0) return;

            var originalEngines = viewEngines.ToList();
            viewEngines.Clear();
            foreach (var engine in originalEngines)
            {
                var wrappedEngine = engine is ProfilingViewEngine ? engine : new ProfilingViewEngine(engine, _profiler);
                viewEngines.Add(wrappedEngine);
            }
        }
    }
}
