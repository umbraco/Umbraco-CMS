using System.Collections;
using System.Collections.Generic;
using System.Configuration;

namespace Umbraco.Core.Configuration.Dashboard
{
    internal class AreaCollection : ConfigurationElementCollection, IEnumerable<IArea>
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new AreaElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((AreaElement) element).Value;
        }

        IEnumerator<IArea> IEnumerable<IArea>.GetEnumerator()
        {
            for (var i = 0; i < Count; i++)
            {
                yield return BaseGet(i) as IArea;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}