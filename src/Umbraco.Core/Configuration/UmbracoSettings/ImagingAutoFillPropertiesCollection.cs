using System.Collections.Generic;
using System.Configuration;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    internal class ImagingAutoFillPropertiesCollection : ConfigurationElementCollection, IEnumerable<IImagingAutoFillUploadField>
    {

        protected override ConfigurationElement CreateNewElement()
        {
            return new ImagingAutoFillUploadFieldElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((ImagingAutoFillUploadFieldElement)element).Alias;
        }
        
        internal void Add(ImagingAutoFillUploadFieldElement item)
        {
            BaseAdd(item);
        }

        IEnumerator<IImagingAutoFillUploadField> IEnumerable<IImagingAutoFillUploadField>.GetEnumerator()
        {
            for (var i = 0; i < Count; i++)
            {
                yield return BaseGet(i) as IImagingAutoFillUploadField;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}