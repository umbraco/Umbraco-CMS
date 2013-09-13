using System;
using System.Configuration;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    internal class RepositoryElement : ConfigurationElement, IRepository
    {
        [ConfigurationProperty("name")]
        public string Name
        {
            get { return (string)base["name"]; }
            set { base["name"] = value; }
        }

        [ConfigurationProperty("guid")]
        public Guid Id
        {
            get { return (Guid)base["guid"]; }
            set { base["guid"] = value; }
        }

    }
}