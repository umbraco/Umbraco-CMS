using System;

namespace Umbraco.Core.Profiling
{
    /// <summary>
    /// Defines an object for use in the application to profile operations
    /// </summary>
    public interface IProfiler
    {
        /// <summary>
        /// Render the UI to display the profiler 
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Generally used for HTML displays
        /// </remarks>
        string Render();

        /// <summary>
        /// Profile an operation
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        /// <remarks>
        /// Use the 'using(' syntax
        /// </remarks>
        IDisposable Step(string name);
   
        /// <summary>
        /// Start the profiler
        /// </summary>
        void Start();

        /// <summary>
        /// Start the profiler
        /// </summary>
        /// <remarks>
        /// set discardResults to false when you want to abandon all profiling, this is useful for 
        /// when someone is not authenticated or you want to clear the results based on some other mechanism.
        /// </remarks>
        void Stop(bool discardResults = false);
    }
}