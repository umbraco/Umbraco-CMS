using System.Configuration;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    internal class DeveloperElement : ConfigurationElement
    {
        [ConfigurationProperty("appCodeFileExtensions")]
        internal AppCodeFileExtensionsElement AppCodeFileExtensions
        {
            get
            {
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
                    var element = new AppCodeFileExtensionsElement
                        {
                            AppCodeFileExtensionsCollection = collection
                        };

                    return element;
                }

                return (AppCodeFileExtensionsElement)base["appCodeFileExtensions"];
            }
        }
    }
}