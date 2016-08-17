using Umbraco.Core.ObjectResolution;

namespace Umbraco.Core.Logging
{
    /// <summary>
    /// A resolver exposing the current profiler
    /// </summary>
    /// <remarks>
    /// NOTE: This is a 'special' resolver in that it gets initialized before most other things, it cannot use IoC so it cannot implement ContainerObjectResolverBase
    /// </remarks>
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