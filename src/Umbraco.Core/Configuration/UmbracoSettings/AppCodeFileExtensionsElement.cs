using System.Collections.Generic;
using System.Configuration;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    internal class AppCodeFileExtensionsElement : ConfigurationElement, IAppCodeFileExtensions
    {
        [ConfigurationCollection(typeof(AppCodeFileExtensionsCollection), AddItemName = "ext")]
        [ConfigurationProperty("", IsDefaultCollection = true)]
        internal AppCodeFileExtensionsCollection AppCodeFileExtensionsCollection
        {
            get { return (AppCodeFileExtensionsCollection)base[""]; }
            set { base[""] = value; }
        }

        IEnumerable<IFileExtension> IAppCodeFileExtensions.AppCodeFileExtensions
        {
            get { return AppCodeFileExtensionsCollection; }
        }
    }
}