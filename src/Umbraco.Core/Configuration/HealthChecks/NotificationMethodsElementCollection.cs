using System;
using System.Collections.Generic;
using System.Configuration;

namespace Umbraco.Core.Configuration.HealthChecks
{
    [ConfigurationCollection(typeof(NotificationMethodElement), AddItemName = "notificationMethod")]
    public class NotificationMethodsElementCollection : ConfigurationElementCollection, IEnumerable<NotificationMethodElement> 
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new NotificationMethodElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((NotificationMethodElement)(element)).Alias;
        }

        new public NotificationMethodElement this[string key]
        {
            get
            {
                return (NotificationMethodElement)BaseGet(key);
            }
        }

        IEnumerator<NotificationMethodElement> IEnumerable<NotificationMethodElement>.GetEnumerator()
        {
            for (var i = 0; i < Count; i++)
            {
                yield return BaseGet(i) as NotificationMethodElement;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
