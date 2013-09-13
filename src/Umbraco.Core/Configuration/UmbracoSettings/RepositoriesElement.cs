using System;
using System.Collections.Generic;
using System.Configuration;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    internal class RepositoriesElement : ConfigurationElement, IRepositories
    {

        [ConfigurationCollection(typeof(RepositoriesCollection), AddItemName = "repository")]
        [ConfigurationProperty("", IsDefaultCollection = true)]
        internal RepositoriesCollection Repositories
        {
            get { return (RepositoriesCollection) base[""]; }
            set { base[""] = value; }
        }

        IEnumerable<IRepository> IRepositories.Repositories
        {
            get { return Repositories; }
        }
    }
}