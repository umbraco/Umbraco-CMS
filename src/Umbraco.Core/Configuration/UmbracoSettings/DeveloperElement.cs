using System.Collections.Generic;
using System.Configuration;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    internal class DeveloperElement : ConfigurationElement, IDeveloperSection
    {
        private AppCodeFileExtensionsElement _default;

        [ConfigurationProperty("appCodeFileExtensions")]
        internal AppCodeFileExtensionsElement AppCodeFileExtensions
        {
            get
            {
                if (_default != null)
                {
                    return _default;
                }

                //here we need to check if this element is defined, if it is not then we'll setup the defaults
                var prop = Properties["appCodeFileExtensions"];
                var autoFill = this[prop] as ConfigurationElement;
                if (autoFill != null && autoFill.ElementInformation.IsPresent == false)
                {
                    var collection = new AppCodeFileExtensionsCollection
                        {
                            new FileExtensionElement {RawValue = "cs"},
                            new FileExtensionElement {RawValue = "vb"}
                        };
                    _default = new AppCodeFileExtensionsElement
                        {
                            AppCodeFileExtensionsCollection = collection
                        };

                    return _default;
                }

                return (AppCodeFileExtensionsElement)base["appCodeFileExtensions"];
            }
        }

        IEnumerable<IFileExtension> IDeveloperSection.AppCodeFileExtensions
        {
            get { return AppCodeFileExtensions.AppCodeFileExtensionsCollection; }
        }
    }
}