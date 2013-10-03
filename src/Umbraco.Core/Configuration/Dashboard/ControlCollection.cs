using System.Collections;
using System.Collections.Generic;
using System.Configuration;

namespace Umbraco.Core.Configuration.Dashboard
{
    internal class ControlCollection : ConfigurationElementCollection, IEnumerable<IControl>
    {
        internal void Add(ControlElement c)
        {
            BaseAdd(c);
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new ControlElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((ControlElement)element).Value;
        }

        IEnumerator<IControl> IEnumerable<IControl>.GetEnumerator()
        {
            for (var i = 0; i < Count; i++)
            {
                yield return BaseGet(i) as IControl;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}