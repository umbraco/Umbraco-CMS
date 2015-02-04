namespace Umbraco.Web.Scheduling
{
    internal class BackgroundTaskRunnerOptions
    {
        //TODO: Could add options for using a stack vs queue if required

        public BackgroundTaskRunnerOptions()
        {
            DedicatedThread = false;
            PersistentThread = false;
            OnlyProcessLastItem = false;
        }

        public bool DedicatedThread { get; set; }
        public bool PersistentThread { get; set; }

        /// <summary>
        /// If this is true, the task runner will skip over all items and only process the last/final 
        /// item registered
        /// </summary>
        public bool OnlyProcessLastItem { get; set; }
    }
}