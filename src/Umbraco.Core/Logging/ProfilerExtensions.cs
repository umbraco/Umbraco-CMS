using System;

namespace Umbraco.Core.Logging
{
    internal static class ProfilerExtensions
    {
        /// <summary>
        /// Writes out a step prefixed with the type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="profiler"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        internal static IDisposable Step<T>(this IProfiler profiler, string name)
        {
            if (profiler == null) throw new ArgumentNullException("profiler");
            return profiler.Step(typeof (T), name);
        }

        /// <summary>
        /// Writes out a step prefixed with the type
        /// </summary>
        /// <param name="profiler"></param>
        /// <param name="objectType"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        internal static IDisposable Step(this IProfiler profiler, Type objectType, string name)
        {
            if (profiler == null) throw new ArgumentNullException("profiler");
            if (objectType == null) throw new ArgumentNullException("objectType");
            if (name == null) throw new ArgumentNullException("name");
            return profiler.Step(string.Format("[{0}] {1}", objectType.Name, name));
        }
    }
}