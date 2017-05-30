using System;
using CoreCurrent = Umbraco.Core.Composing.Current;

// ReSharper disable once CheckNamespace
namespace Umbraco.Core.Logging
{
    public class ProfilerResolver
    {
        private ProfilerResolver()
        { }

        public static bool HasCurrent => true;

        public static ProfilerResolver Current { get; }
            = new ProfilerResolver();

        public IProfiler Profiler => CoreCurrent.Profiler;

        public void SetProfiler(IProfiler profiler)
        {
            throw new NotSupportedException("The profiler is configured during the IRuntime Compose() method. Implement a"
                + " custom IRuntime if you need to specify your own custom profiler.");
        }
    }
}
