using System.Configuration;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    internal class ScheduledTaskElement : ConfigurationElement, IScheduledTask
    {
        [ConfigurationProperty("alias")]
        public string Alias
        {
            get { return (string)base["alias"]; }
        }

        [ConfigurationProperty("log")]
        public bool Log
        {
            get { return (bool)base["log"]; }
        }

        [ConfigurationProperty("interval")]
        public int Interval
        {
            get { return (int)base["interval"]; }
        }

        [ConfigurationProperty("url")]
        public string Url
        {
            get { return (string)base["url"]; }
        }
    }
}