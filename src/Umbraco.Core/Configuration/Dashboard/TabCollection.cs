using System.Collections;
using System.Collections.Generic;
using System.Configuration;

namespace Umbraco.Core.Configuration.Dashboard
{
    internal class TabCollection : ConfigurationElementCollection, IEnumerable<IDashboardTab>
    {
        internal void Add(TabElement c)
        {
            BaseAdd(c);
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new TabElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((TabElement)element).Caption;
        }

        IEnumerator<IDashboardTab> IEnumerable<IDashboardTab>.GetEnumerator()
        {
            for (var i = 0; i < Count; i++)
            {
                yield return BaseGet(i) as IDashboardTab;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}