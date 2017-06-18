using System;
using System.Collections.Generic;
using System.Configuration;

namespace Umbraco.Core.Configuration.HealthChecks
{
    [ConfigurationCollection(typeof(NotificationMethodSettingsElement), AddItemName = "add")]
    public class NotificationMethodSettingsElementCollection : ConfigurationElementCollection, IEnumerable<NotificationMethodSettingsElement> 
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new NotificationMethodSettingsElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((NotificationMethodSettingsElement)(element)).Key;
        }

        new public NotificationMethodSettingsElement this[string key]
        {
            get
            {
                return (NotificationMethodSettingsElement)BaseGet(key);
            }
        }

        IEnumerator<NotificationMethodSettingsElement> IEnumerable<NotificationMethodSettingsElement>.GetEnumerator()
        {
            for (var i = 0; i < Count; i++)
            {
                yield return BaseGet(i) as NotificationMethodSettingsElement;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
