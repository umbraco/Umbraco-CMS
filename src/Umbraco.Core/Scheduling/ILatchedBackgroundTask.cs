using System;
using System.Threading.Tasks;
using Umbraco.Core.CodeAnnotations;

namespace Umbraco.Web.Scheduling
{
    /// <summary>
    /// Represents a latched background task.
    /// </summary>
    /// <remarks>Latched background tasks can suspend their execution until
    /// a condition is met. However if the tasks runner has to terminate,
    /// latched background tasks can be executed immediately, depending on
    /// the value returned by RunsOnShutdown.</remarks>
    [UmbracoVolatile]
    public interface ILatchedBackgroundTask : IBackgroundTask
    {
        /// <summary>
        /// Gets a task on latch.
        /// </summary>
        /// <exception cref="InvalidOperationException">The task is not latched.</exception>
        Task Latch { get; }

        /// <summary>
        /// Gets a value indicating whether the task is latched.
        /// </summary>
        /// <remarks>Should return false as soon as the condition is met.</remarks>
        bool IsLatched { get; }

        /// <summary>
        ///  Gets a value indicating whether the task can be executed immediately if the task runner has to terminate.
        /// </summary>
        bool RunsOnShutdown { get; }
    }
}
