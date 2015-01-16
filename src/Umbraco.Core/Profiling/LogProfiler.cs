using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Umbraco.Core.Logging;

namespace Umbraco.Core.Profiling
{
    /// <summary>
    /// A profiler that outputs its results to an ILogger
    /// </summary>
    internal class LogProfiler : IProfiler
    {
        private readonly ILogger _logger;

        public LogProfiler(ILogger logger)
        {
            _logger = logger;
        }

        public string Render()
        {
            return string.Empty;
        }

        public IDisposable Step(string name)
        {
            _logger.Debug(typeof(LogProfiler), "Starting - " + name);
            return new DisposableTimer(l => _logger.Info(typeof(LogProfiler), () => name + " (took " + l + "ms)"));
        }

        public void Start()
        {
            //the log will alwasy be started   
        }

        public void Stop(bool discardResults = false)
        {
            //we don't need to do anything here
        }
    }
}
