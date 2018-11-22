using System.Collections;
using System.Collections.Generic;
using System.Configuration;

namespace Umbraco.Core.Configuration.Dashboard
{
    internal class SectionCollection : ConfigurationElementCollection, IEnumerable<ISection>
    {
        internal void Add(SectionElement c)
        {
            BaseAdd(c);
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new SectionElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((SectionElement)element).Alias;
        }

        IEnumerator<ISection> IEnumerable<ISection>.GetEnumerator()
        {
            for (var i = 0; i < Count; i++)
            {
                yield return BaseGet(i) as ISection;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}