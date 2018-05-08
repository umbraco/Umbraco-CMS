using System.Collections.Generic;
using System.Configuration;

namespace Umbraco.Core.Configuration.HealthChecks
{
    [ConfigurationCollection(typeof(DisabledHealthCheckElement), AddItemName = "check")]
    public class DisabledHealthChecksElementCollection : ConfigurationElementCollection, IEnumerable<IDisabledHealthCheck> 
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new DisabledHealthCheckElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((DisabledHealthCheckElement)(element)).Id;
        }

        public new DisabledHealthCheckElement this[string key]
        {
            get
            {
                return (DisabledHealthCheckElement)BaseGet(key);
            }
        }

        IEnumerator<IDisabledHealthCheck> IEnumerable<IDisabledHealthCheck>.GetEnumerator()
        {
            for (var i = 0; i < Count; i++)
            {
                yield return BaseGet(i) as DisabledHealthCheckElement;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
