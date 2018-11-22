using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace Umbraco.Core.Configuration
{
    public class FileSystemProvidersSection : ConfigurationSection, IFileSystemProvidersSection
    {

        [ConfigurationProperty("", IsDefaultCollection = true, IsRequired = true)]
        public FileSystemProviderElementCollection Providers
        {
            get { return ((FileSystemProviderElementCollection)(base[""])); }
        }

        private IDictionary<string, IFileSystemProviderElement> _providers;

        IDictionary<string, IFileSystemProviderElement> IFileSystemProvidersSection.Providers
        {
            get
            {
                if (_providers != null) return _providers;
                _providers = Providers.ToDictionary(x => x.Alias, x => x);
                return _providers;
            }
        }
    }
}
