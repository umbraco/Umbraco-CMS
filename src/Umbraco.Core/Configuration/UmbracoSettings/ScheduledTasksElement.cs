using System.Configuration;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    internal class ScheduledTasksElement : ConfigurationElement
    {
        [ConfigurationCollection(typeof(ScheduledTasksCollection), AddItemName = "task")]
        [ConfigurationProperty("", IsDefaultCollection = true)]
        public ScheduledTasksCollection Tasks
        {
            get { return (ScheduledTasksCollection)base[""]; }
        }
    }
}