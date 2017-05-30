using System;
using System.Configuration;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    internal class RepositoryElement : ConfigurationElement, IRepository
    {
        [ConfigurationProperty("name", IsRequired = true)]
        public string Name
        {
            get { return (string)base["name"]; }
            set { base["name"] = value; }
        }

        [ConfigurationProperty("guid", IsRequired = true)]
        public Guid Id
        {
            get { return (Guid)base["guid"]; }
            set { base["guid"] = value; }
        }

        [ConfigurationProperty("repositoryurl", DefaultValue = "http://packages.umbraco.org")]
        public string RepositoryUrl
        {
            get { return (string)base["repositoryurl"]; }
            set { base["repositoryurl"] = value; }
        }

        [ConfigurationProperty("webserviceurl", DefaultValue = "/umbraco/webservices/api/repository.asmx")]
        public string WebServiceUrl
        {
            get { return (string)base["webserviceurl"]; }
            set { base["webserviceurl"] = value; }
        }

        public bool HasCustomWebServiceUrl
        {
            get
            {
                var prop = Properties["webserviceurl"];
                return (string) prop.DefaultValue != (string) this[prop];                
            }
        }

        [ConfigurationProperty("restapiurl", DefaultValue = "https://our.umbraco.org/webapi/packages/v1")]
        public string RestApiUrl
        {
            get { return (string)base["restapiurl"]; }
            set { base["restapiurl"] = value; }
        }
        
    }
}