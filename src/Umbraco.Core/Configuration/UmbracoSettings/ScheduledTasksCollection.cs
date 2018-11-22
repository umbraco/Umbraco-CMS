using System.Collections.Generic;
using System.Configuration;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    internal class ScheduledTasksCollection : ConfigurationElementCollection, IEnumerable<IScheduledTask>
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new ScheduledTaskElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((ScheduledTaskElement)element).Alias;
        }

        IEnumerator<IScheduledTask> IEnumerable<IScheduledTask>.GetEnumerator()
        {
            for (var i = 0; i < Count; i++)
            {
                yield return BaseGet(i) as IScheduledTask;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}