using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace Umbraco.Core.Configuration
{
    [ConfigurationCollection(typeof(FileSystemProviderElement), AddItemName = "Provider")]
    public class FileSystemProviderElementCollection : ConfigurationElementCollection, IEnumerable<IFileSystemProviderElement>
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new FileSystemProviderElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((FileSystemProviderElement)(element)).Alias;
        }

        public new FileSystemProviderElement this[string key]
        {
            get
            {
                return (FileSystemProviderElement)BaseGet(key);
            }
        }

        IEnumerator<IFileSystemProviderElement> IEnumerable<IFileSystemProviderElement>.GetEnumerator()
        {
            for (var i = 0; i < Count; i++)
            {
                yield return BaseGet(i) as IFileSystemProviderElement;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
