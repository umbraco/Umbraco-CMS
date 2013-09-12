using System.Configuration;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    internal class UrlReplacingElement : ConfigurationElement
    {
        [ConfigurationProperty("removeDoubleDashes", DefaultValue = true)]
        internal bool RemoveDoubleDashes
        {
            get { return (bool) base["removeDoubleDashes"]; }
        }

        [ConfigurationCollection(typeof(CharCollection), AddItemName = "char")]
        [ConfigurationProperty("", IsDefaultCollection = true)]
        public CharCollection CharCollection
        {
            get { return (CharCollection)base[""]; }
            set { base[""] = value; }
        }

    }
}