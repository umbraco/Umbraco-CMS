using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Umbraco.Core.Logging;

namespace Umbraco.Core.Profiling
{
    /// <summary>
    /// A profiler that outputs its results to the LogHelper
    /// </summary>
    public class LogProfiler : IProfiler
    {
        public string Render()
        {
            return string.Empty;
        }

        public IDisposable Step(string name)
        {
            LogHelper.Debug(typeof(LogProfiler), "Starting - " + name);
            return DisposableTimer.Start(l => LogHelper.Info(typeof (LogProfiler), () => name + " (took " + l + "ms)"));
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
