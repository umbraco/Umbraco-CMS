using System.Collections.Generic;
using System.Configuration;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    internal class ContentImagingCropSizeCollection : ConfigurationElementCollection, IEnumerable<IImagingCropSize>
    {
        internal void Add(ContentImagingCropSizeElement c)
        {
            BaseAdd(c);
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new ContentImagingCropSizeElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((ContentImagingCropSizeElement)element).Alias;
        }

        IEnumerator<IImagingCropSize> IEnumerable<IImagingCropSize>.GetEnumerator()
        {
            for (var i = 0; i < Count; i++)
            {
                yield return BaseGet(i) as IImagingCropSize;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}