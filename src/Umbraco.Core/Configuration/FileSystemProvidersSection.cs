using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace Umbraco.Core.Configuration
{
    public class FileSystemProvidersSection : ConfigurationSection
    {

        [ConfigurationProperty("", IsDefaultCollection = true, IsRequired = true)]
        public FileSystemProviderElementCollection Providers
        {
            get { return ((FileSystemProviderElementCollection)(base[""])); }
        }
    }
}
