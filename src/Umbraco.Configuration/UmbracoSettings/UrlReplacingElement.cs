using System.Collections.Generic;
using System.Configuration;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    internal class UrlReplacingElement : ConfigurationElement
    {
        [ConfigurationProperty("removeDoubleDashes", DefaultValue = true)]
        internal bool RemoveDoubleDashes => (bool) base["removeDoubleDashes"];

        [ConfigurationProperty("toAscii", DefaultValue = "false")]
        internal string ConvertUrlsToAscii => (string) base["toAscii"];

        [ConfigurationCollection(typeof(CharCollection), AddItemName = "char")]
        [ConfigurationProperty("", IsDefaultCollection = true)]
        internal CharCollection CharCollection
        {
            get => (CharCollection)base[""];
            set => base[""] = value;
        }

    }
}
