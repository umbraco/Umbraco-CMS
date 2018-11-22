using System.Collections.Generic;
using System.Configuration;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    internal class DisabledLogTypesCollection : ConfigurationElementCollection, IEnumerable<ILogType>
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new LogTypeElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((LogTypeElement)element).Value;
        }

        IEnumerator<ILogType> IEnumerable<ILogType>.GetEnumerator()
        {
            for (var i = 0; i < Count; i++)
            {
                yield return BaseGet(i) as ILogType;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}