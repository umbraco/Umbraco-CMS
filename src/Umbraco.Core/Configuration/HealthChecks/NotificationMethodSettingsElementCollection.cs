using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace Umbraco.Core.Configuration.HealthChecks
{
    [ConfigurationCollection(typeof(NotificationMethodSettingsElement), AddItemName = "add")]
    public class NotificationMethodSettingsElementCollection : ConfigurationElementCollection, IEnumerable<INotificationMethodSettings>, IReadOnlyDictionary<string, INotificationMethodSettings>
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new NotificationMethodSettingsElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((NotificationMethodSettingsElement)(element)).Key;
        }

        IEnumerator<KeyValuePair<string, INotificationMethodSettings>> IEnumerable<KeyValuePair<string, INotificationMethodSettings>>.GetEnumerator()
        {
            for (var i = 0; i < Count; i++)
            {
                var val = (NotificationMethodSettingsElement)BaseGet(i);
                var key = (string)BaseGetKey(i);
                yield return new KeyValuePair<string, INotificationMethodSettings>(key, val);
            }
        }

        IEnumerator<INotificationMethodSettings> IEnumerable<INotificationMethodSettings>.GetEnumerator()
        {
            for (var i = 0; i < Count; i++)
            {
                yield return (NotificationMethodSettingsElement)BaseGet(i);
            }
        }

        bool IReadOnlyDictionary<string, INotificationMethodSettings>.ContainsKey(string key)
        {
            return ((IReadOnlyDictionary<string, INotificationMethodSettings>)this).Keys.Any(x => x == key);
        }

        bool IReadOnlyDictionary<string, INotificationMethodSettings>.TryGetValue(string key, out INotificationMethodSettings value)
        {
            try
            {
                var val = (NotificationMethodSettingsElement)BaseGet(key);
                value = val;
                return true;
            }
            catch (Exception)
            {
                value = null;
                return false;
            }
        }

        INotificationMethodSettings IReadOnlyDictionary<string, INotificationMethodSettings>.this[string key]
        {
            get { return (NotificationMethodSettingsElement)BaseGet(key); }
        }

        IEnumerable<string> IReadOnlyDictionary<string, INotificationMethodSettings>.Keys
        {
            get { return BaseGetAllKeys().Cast<string>(); }
        }

        IEnumerable<INotificationMethodSettings> IReadOnlyDictionary<string, INotificationMethodSettings>.Values
        {
            get
            {
                for (var i = 0; i < Count; i++)
                {
                    yield return (NotificationMethodSettingsElement)BaseGet(i);
                }
            }
        }
    }
}
