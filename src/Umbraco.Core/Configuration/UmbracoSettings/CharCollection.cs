using System.Collections.Generic;
using System.Configuration;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    internal class CharCollection : ConfigurationElementCollection, IEnumerable<IChar>
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

        IEnumerator<IChar> IEnumerable<IChar>.GetEnumerator()
        {
            for (var i = 0; i < Count; i++)
            {
                yield return BaseGet(i) as IChar;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}