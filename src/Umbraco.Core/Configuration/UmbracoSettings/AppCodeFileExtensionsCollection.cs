using System.Collections.Generic;
using System.Configuration;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    internal class AppCodeFileExtensionsCollection : ConfigurationElementCollection, IEnumerable<IFileExtension>
    {
        internal void Add(FileExtensionElement element)
        {
            base.BaseAdd(element);
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new FileExtensionElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((FileExtensionElement)element).Value;
        }

        IEnumerator<IFileExtension> IEnumerable<IFileExtension>.GetEnumerator()
        {
            for (var i = 0; i < Count; i++)
            {
                yield return BaseGet(i) as IFileExtension;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}