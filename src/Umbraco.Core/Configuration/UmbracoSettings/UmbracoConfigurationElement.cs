using System.Collections.Concurrent;
using System.Configuration;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    /// <summary>
    /// Base class with shared helper methods
    /// </summary>
    internal class UmbracoConfigurationElement : ConfigurationElement
    {
        /// <summary>
        /// Used so the RawElement types are not re-created every time they are accessed
        /// </summary>
        private readonly ConcurrentDictionary<string, RawXmlConfigurationElement> _rawElements = new ConcurrentDictionary<string, RawXmlConfigurationElement>();

        protected OptionalInnerTextConfigurationElement<T> GetOptionalTextElement<T>(string name, T defaultVal)
        {
            return (OptionalInnerTextConfigurationElement<T>) _rawElements.GetOrAdd(
                name,
                s => new OptionalInnerTextConfigurationElement<T>(
                    (InnerTextConfigurationElement<T>) this[s],
                    //set the default
                    defaultVal));
        }

        protected OptionalCommaDelimitedConfigurationElement GetOptionalDelimitedElement(string name, string[] defaultVal)
        {
            return (OptionalCommaDelimitedConfigurationElement) _rawElements.GetOrAdd(
                name,
                s => new OptionalCommaDelimitedConfigurationElement(
                    (CommaDelimitedConfigurationElement) this[name],
                    //set the default
                    defaultVal));
        }
    }
}