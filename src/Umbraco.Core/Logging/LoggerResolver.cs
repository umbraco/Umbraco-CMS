using Umbraco.Core.ObjectResolution;

namespace Umbraco.Core.Logging
{
    /// <summary>
    /// The logger resolver
    /// </summary>
    /// <remarks>
    /// NOTE: This is a 'special' resolver in that it gets initialized before most other things, it cannot use IoC so it cannot implement ContainerObjectResolverBase
    /// </remarks>
    public sealed class LoggerResolver : SingleObjectResolverBase<LoggerResolver, ILogger>
    {
        public LoggerResolver(ILogger logger)
            : base(logger)
        {
            
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