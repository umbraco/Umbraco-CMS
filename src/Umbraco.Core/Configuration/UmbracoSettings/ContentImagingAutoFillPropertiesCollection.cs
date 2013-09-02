using System.Collections.Generic;
using System.Configuration;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    internal class ContentImagingAutoFillPropertiesCollection : ConfigurationElementCollection, IEnumerable<ContentImagingAutoFillUploadFieldElement>
    {

        protected override ConfigurationElement CreateNewElement()
        {
            return new ContentImagingAutoFillUploadFieldElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((ContentImagingAutoFillUploadFieldElement)element).Alias;
        }
        
        internal void Add(ContentImagingAutoFillUploadFieldElement item)
        {
            BaseAdd(item);
        }

        IEnumerator<ContentImagingAutoFillUploadFieldElement> IEnumerable<ContentImagingAutoFillUploadFieldElement>.GetEnumerator()
        {
            for (var i = 0; i < Count; i++)
            {
                yield return BaseGet(i) as ContentImagingAutoFillUploadFieldElement;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}