using System.Collections.Generic;
using System.Configuration;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    internal class ContentImagingCropCollection : ConfigurationElementCollection, IEnumerable<IImagingCrop>
    {
        internal void Add(ContentImagingCropElement c)
        {
            BaseAdd(c);
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new ContentImagingCropElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((ContentImagingCropElement)element).MediaTypeAlias;
        }

        IEnumerator<IImagingCrop> IEnumerable<IImagingCrop>.GetEnumerator()
        {
            for (var i = 0; i < Count; i++)
            {
                yield return BaseGet(i) as IImagingCrop;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}