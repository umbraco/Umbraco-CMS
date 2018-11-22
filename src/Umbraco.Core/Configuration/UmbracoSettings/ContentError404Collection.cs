using System.Collections.Generic;
using System.Configuration;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    internal class ContentError404Collection : ConfigurationElementCollection, IEnumerable<ContentErrorPageElement>
    {
        internal void Add(ContentErrorPageElement element)
        {
            BaseAdd(element);
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new ContentErrorPageElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((ContentErrorPageElement)element).Culture
                + ((ContentErrorPageElement)element).Value;
        }

        IEnumerator<ContentErrorPageElement> IEnumerable<ContentErrorPageElement>.GetEnumerator()
        {
            for (var i = 0; i < Count; i++)
            {
                yield return BaseGet(i) as ContentErrorPageElement;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}