using System.Collections.Generic;
using System.Configuration;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    internal class UrlReplacingElement : ConfigurationElement, IUrlReplacing
    {
        [ConfigurationProperty("removeDoubleDashes", DefaultValue = true)]
        internal bool RemoveDoubleDashes
        {
            get { return (bool) base["removeDoubleDashes"]; }
        }

        [ConfigurationCollection(typeof(CharCollection), AddItemName = "char")]
        [ConfigurationProperty("", IsDefaultCollection = true)]
        internal CharCollection CharCollection
        {
            get { return (CharCollection)base[""]; }
            set { base[""] = value; }
        }


        bool IUrlReplacing.RemoveDoubleDashes
        {
            get { return RemoveDoubleDashes; }
        }

        IEnumerable<IChar> IUrlReplacing.CharCollection
        {
            get { return CharCollection; }
        }
    }
}