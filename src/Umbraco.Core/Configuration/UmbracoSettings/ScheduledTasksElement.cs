using System.Collections.Generic;
using System.Configuration;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    internal class ScheduledTasksElement : ConfigurationElement, IScheduledTasks
    {
        [ConfigurationCollection(typeof(ScheduledTasksCollection), AddItemName = "task")]
        [ConfigurationProperty("", IsDefaultCollection = true)]
        internal ScheduledTasksCollection Tasks
        {
            get { return (ScheduledTasksCollection)base[""]; }
        }

        IEnumerable<IScheduledTask> IScheduledTasks.Tasks
        {
            get { return Tasks; }
        }
    }
}