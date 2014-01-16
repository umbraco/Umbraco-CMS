using System.Collections.Generic;
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

        [ConfigurationProperty("toAscii", DefaultValue = false)]
        internal bool ConvertUrlsToAscii
        {
            get { return (bool)base["toAscii"]; }
        }

        [ConfigurationCollection(typeof(CharCollection), AddItemName = "char")]
        [ConfigurationProperty("", IsDefaultCollection = true)]
        internal CharCollection CharCollection
        {
            get { return (CharCollection)base[""]; }
            set { base[""] = value; }
        }
        
    }
}