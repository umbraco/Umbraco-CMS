using System;
using System.Threading;
using System.Threading.Tasks;

namespace Umbraco.Web.Scheduling
{
    /// <summary>
    /// Represents a background task.
    /// </summary>
    internal interface IBackgroundTask : IDisposable
    {
        /// <summary>
        /// Runs the background task.
        /// </summary>
        void Run();

        /// <summary>
        /// Runs the task asynchronously.
        /// </summary>
        /// <param name="token">A cancellation token.</param>
        /// <returns>A <see cref="Task"/> instance representing the execution of the background task.</returns>
        /// <exception cref="NotImplementedException">The background task cannot run asynchronously.</exception>
        Task RunAsync(CancellationToken token);

        /// <summary>
        /// Indicates whether the background task can run asynchronously.
        /// </summary>
        bool IsAsync { get; }
    }
}