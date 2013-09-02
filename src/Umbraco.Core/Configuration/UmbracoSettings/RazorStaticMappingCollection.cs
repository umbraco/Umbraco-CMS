using System.Collections.Generic;
using System.Configuration;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    internal class RazorStaticMappingCollection : ConfigurationElementCollection, IEnumerable<RazorStaticMappingElement>
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new RazorStaticMappingElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((RazorStaticMappingElement) element).DataTypeGuid
                   + ((RazorStaticMappingElement) element).NodeTypeAlias
                   + ((RazorStaticMappingElement) element).DocumentTypeAlias;
        }


        IEnumerator<RazorStaticMappingElement> IEnumerable<RazorStaticMappingElement>.GetEnumerator()
        {
            for (var i = 0; i < Count; i++)
            {
                yield return BaseGet(i) as RazorStaticMappingElement;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}