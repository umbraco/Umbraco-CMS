namespace Umbraco.Web.Scheduling
{
    /// <summary>
    /// Provides options to the <see cref="BackgroundTaskRunner{T}"/> class.
    /// </summary>
    internal class BackgroundTaskRunnerOptions
    {
        //TODO: Could add options for using a stack vs queue if required

        /// <summary>
        /// Initializes a new instance of the <see cref="BackgroundTaskRunnerOptions"/> class.
        /// </summary>
        public BackgroundTaskRunnerOptions()
        {
            LongRunning = false;
            KeepAlive = false;
            AutoStart = false;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the running task should be a long-running,
        /// coarse grained operation.
        /// </summary>
        public bool LongRunning { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the running task should block and wait
        /// on the queue, or end, when the queue is empty.
        /// </summary>
        public bool KeepAlive { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the running task should start immediately
        /// or only once a task has been added to the queue.
        /// </summary>
        public bool AutoStart { get; set; }

        /// <summary>
        /// Gets or setes a value indicating whether the running task should be preserved
        /// once completed, or reset to null. For unit tests.
        /// </summary>
        public bool PreserveRunningTask { get; set; }
    }
}