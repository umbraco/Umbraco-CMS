using System.Collections.Generic;
using Umbraco.Core.Events;
using Umbraco.Core.Services.Implement;

namespace Umbraco.Web.Cache
{
    /// <summary>
    /// Binds events to the distributed cache.
    /// </summary>
    /// <remarks>
    /// <para>Use <see cref="BindEvents"/> to bind actual events, eg <see cref="ContentService.Saved"/>, to
    /// the distributed cache, so that the proper refresh operations are executed when these events trigger.</para>
    /// <para>Use <see cref="HandleEvents"/> to handle events that have not actually triggered, but have
    /// been queued, so that the proper refresh operations are also executed.</para>
    /// </remarks>
    public interface IDistributedCacheBinder
    {
        /// <summary>
        /// Handles events from definitions.
        /// </summary>
        void HandleEvents(IEnumerable<IEventDefinition> events);

        /// <summary>
        /// Binds actual events to the distributed cache.
        /// </summary>
        /// <param name="enableUnbinding">A value indicating whether to support unbinding the events.</param>
        void BindEvents(bool enableUnbinding = false);

        /// <summary>
        /// Unbinds bounded events.
        /// </summary>
        void UnbindEvents();
    }
}
