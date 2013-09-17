using System.Collections.Generic;
using System.Configuration;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    internal class RazorStaticMappingCollection : ConfigurationElementCollection, IEnumerable<IRazorStaticMapping>
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new RazorStaticMappingElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((RazorStaticMappingElement) element).DataTypeGuid
                   + ((RazorStaticMappingElement) element).NodeTypeAlias
                   + ((RazorStaticMappingElement) element).PropertyTypeAlias;
        }


        IEnumerator<IRazorStaticMapping> IEnumerable<IRazorStaticMapping>.GetEnumerator()
        {
            for (var i = 0; i < Count; i++)
            {
                yield return BaseGet(i) as IRazorStaticMapping;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}