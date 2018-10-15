using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete("This is no longer used and will be removed in future versions")]
    internal class HelpElement : ConfigurationElement, IHelpSection
    {
        [ConfigurationProperty("defaultUrl", DefaultValue = "https://our.umbraco.com/wiki/umbraco-help/{0}/{1}")]
        public string DefaultUrl
        {
            get { return (string) base["defaultUrl"]; }
        }

        [ConfigurationCollection(typeof (LinksCollection), AddItemName = "link")]
        [ConfigurationProperty("", IsDefaultCollection = true)]
        public LinksCollection Links
        {
            get { return (LinksCollection) base[""]; }
        }

        string IHelpSection.DefaultUrl
        {
            get { return DefaultUrl; }
        }

        IEnumerable<ILink> IHelpSection.Links
        {
            get { return Links; }
        }
    }
}