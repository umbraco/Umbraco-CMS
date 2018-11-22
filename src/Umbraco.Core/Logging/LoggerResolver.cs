using Umbraco.Core.ObjectResolution;

namespace Umbraco.Core.Logging
{
    public sealed class LoggerResolver : SingleObjectResolverBase<LoggerResolver, ILogger>
    {
        public LoggerResolver(ILogger logger)
            : base(logger)
        {
            
        }

        /// <summary>
        /// Method allowing to change the logger during startup
        /// </summary>
        /// <param name="profiler"></param>
        internal void SetLogger(ILogger profiler)
        {
            Value = profiler;
        }

        /// <summary>
        /// Gets the current logger
        /// </summary>
        public ILogger Logger
        {
            get { return Value; }
        }

    }
}