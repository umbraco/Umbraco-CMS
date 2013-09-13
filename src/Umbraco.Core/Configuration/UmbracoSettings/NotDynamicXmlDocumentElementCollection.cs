using System.Collections.Generic;
using System.Configuration;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    internal class NotDynamicXmlDocumentElementCollection : ConfigurationElementCollection, IEnumerable<INotDynamicXmlDocument>
    {
        internal void Add(NotDynamicXmlDocumentElement element)
        {
            BaseAdd(element);
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new NotDynamicXmlDocumentElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((NotDynamicXmlDocumentElement) element).Value;
        }

        IEnumerator<INotDynamicXmlDocument> IEnumerable<INotDynamicXmlDocument>.GetEnumerator()
        {
            for (var i = 0; i < Count; i++)
            {
                yield return BaseGet(i) as INotDynamicXmlDocument;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}