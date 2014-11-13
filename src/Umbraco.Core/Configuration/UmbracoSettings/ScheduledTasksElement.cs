using System.Collections.Generic;
using System.Configuration;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    internal class ScheduledTasksElement : ConfigurationElement, IScheduledTasksSection
    {
        [ConfigurationCollection(typeof(ScheduledTasksCollection), AddItemName = "task")]
        [ConfigurationProperty("", IsDefaultCollection = true)]
        internal ScheduledTasksCollection Tasks
        {
            get { return (ScheduledTasksCollection)base[""]; }
        }

        IEnumerable<IScheduledTask> IScheduledTasksSection.Tasks
        {
            get { return Tasks; }
        }

        [ConfigurationProperty("baseUrl", IsRequired = false, DefaultValue = null)]
        public string BaseUrl
        {
            get { return (string)base["baseUrl"]; }
        }
    }
}