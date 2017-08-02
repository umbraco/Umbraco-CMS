using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace Umbraco.Core.Configuration.HealthChecks
{
    [ConfigurationCollection(typeof(NotificationMethodElement), AddItemName = "notificationMethod")]
    public class NotificationMethodsElementCollection : ConfigurationElementCollection, IEnumerable<INotificationMethod>, IReadOnlyDictionary<string, INotificationMethod>
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new NotificationMethodElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((NotificationMethodElement)(element)).Alias;
        }
        
        IEnumerator<KeyValuePair<string, INotificationMethod>> IEnumerable<KeyValuePair<string, INotificationMethod>>.GetEnumerator()
        {
            for (var i = 0; i < Count; i++)
            {
                var val = (NotificationMethodElement)BaseGet(i);
                var key = (string)BaseGetKey(i);
                yield return new KeyValuePair<string, INotificationMethod>(key, val);
            }
        }

        IEnumerator<INotificationMethod> IEnumerable<INotificationMethod>.GetEnumerator()
        {
            for (var i = 0; i < Count; i++)
            {
                yield return (NotificationMethodElement)BaseGet(i);
            }
        }

        bool IReadOnlyDictionary<string, INotificationMethod>.ContainsKey(string key)
        {
            return ((IReadOnlyDictionary<string, INotificationMethod>) this).Keys.Any(x => x == key);
        }

        bool IReadOnlyDictionary<string, INotificationMethod>.TryGetValue(string key, out INotificationMethod value)
        {
            try
            {
                var val = (NotificationMethodElement)BaseGet(key);
                value = val;
                return true;
            }
            catch (Exception)
            {
                value = null;
                return false;
            }
        }

        INotificationMethod IReadOnlyDictionary<string, INotificationMethod>.this[string key]
        {
            get { return (NotificationMethodElement)BaseGet(key); }
        }

        IEnumerable<string> IReadOnlyDictionary<string, INotificationMethod>.Keys
        {
            get { return BaseGetAllKeys().Cast<string>(); }
        }

        IEnumerable<INotificationMethod> IReadOnlyDictionary<string, INotificationMethod>.Values
        {
            get
            {
                for (var i = 0; i < Count; i++)
                {
                    yield return (NotificationMethodElement)BaseGet(i);
                }
            }
        }
    }
}
