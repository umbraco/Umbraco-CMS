using System.Collections.Generic;
using System.Configuration;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    internal class CharCollection : ConfigurationElementCollection, IEnumerable<CharElement>
    {
        internal void Add(CharElement c)
        {
            BaseAdd(c);
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new CharElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((CharElement)element).Char;
        }

        IEnumerator<CharElement> IEnumerable<CharElement>.GetEnumerator()
        {
            for (var i = 0; i < Count; i++)
            {
                yield return BaseGet(i) as CharElement;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}