using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace Umbraco.Core.Configuration
{
    [ConfigurationCollection(typeof(FileSystemProviderElement), AddItemName = "Provider")]
    public class FileSystemProviderElementCollection : ConfigurationElementCollection   
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new FileSystemProviderElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((FileSystemProviderElement)(element)).Alias;
        }

        new public FileSystemProviderElement this[string key]
        {
            get
            {
                return (FileSystemProviderElement)BaseGet(key);
            }
        }
    }
}
