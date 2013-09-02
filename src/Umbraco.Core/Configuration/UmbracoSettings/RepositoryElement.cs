using System;
using System.Configuration;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    internal class RepositoryElement : ConfigurationElement
    {
        [ConfigurationProperty("name")]
        internal string Name
        {
            get { return (string)base["name"]; }
        }

        [ConfigurationProperty("guid")]
        internal Guid Id
        {
            get { return (Guid)base["guid"]; }
        }

    }
}