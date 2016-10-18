using System.Configuration;

namespace Umbraco.Web.Search.Factory
{
    public class UmbracoSearcherFactorySection : ConfigurationSection
    {
        [ConfigurationProperty("type", IsRequired = false)]
        public string Type
        {
            get
            {
                return (string)this["type"];
            }
            set
            {
                this["type"] = value;
            }
        }
    }
}