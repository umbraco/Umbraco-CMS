using Umbraco.Core.ObjectResolution;

namespace Umbraco.Core.Profiling
{
    /// <summary>
    /// A resolver exposing the current profiler
    /// </summary>
    internal class ProfilerResolver : SingleObjectResolverBase<ProfilerResolver, IProfiler>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="profiler"></param>
        public ProfilerResolver(IProfiler profiler)
            : base(profiler)
        {

        }

        /// <summary>
        /// Method allowing to change the profiler during startup
        /// </summary>
        /// <param name="profiler"></param>
        internal void SetProfiler(IProfiler profiler)
        {
            Value = profiler;
        }

        /// <summary>
        /// Gets the current profiler
        /// </summary>
        public IProfiler Profiler
        {
            get { return Value; }
        }

        
    }
}