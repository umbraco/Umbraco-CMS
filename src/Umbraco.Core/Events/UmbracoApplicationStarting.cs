namespace Umbraco.Core.Events
{
    public class UmbracoApplicationStarting : INotification
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UmbracoApplicationStarting"/> class.
        /// </summary>
        /// <param name="runtimeLevel">The runtime level</param>
        public UmbracoApplicationStarting(RuntimeLevel runtimeLevel) => RuntimeLevel = runtimeLevel;

        /// <summary>
        /// Gets the runtime level of execution.
        /// </summary>
        public RuntimeLevel RuntimeLevel { get; }
    }
}
