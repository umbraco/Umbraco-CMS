using System;

namespace Umbraco.Core.Profiling
{
    public static class ProfilerExtensions
    {
        /// <summary>
        /// Writes out a step prefixed with the type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="profiler"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static IDisposable Step<T>(this IProfiler profiler, string name)
        {
            return profiler.Step(string.Format("[" + typeof (T).Name + "] " + name));
        }
    }
}