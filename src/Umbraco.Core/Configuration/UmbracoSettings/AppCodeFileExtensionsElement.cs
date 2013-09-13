using System.Collections.Generic;
using System.Configuration;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    internal class AppCodeFileExtensionsElement : ConfigurationElement
    {
        [ConfigurationCollection(typeof(AppCodeFileExtensionsCollection), AddItemName = "ext")]
        [ConfigurationProperty("", IsDefaultCollection = true)]
        internal AppCodeFileExtensionsCollection AppCodeFileExtensionsCollection
        {
            get { return (AppCodeFileExtensionsCollection)base[""]; }
            set { base[""] = value; }
        }

    }
}