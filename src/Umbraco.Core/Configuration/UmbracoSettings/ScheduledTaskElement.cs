using System.Configuration;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    internal class ScheduledTaskElement : ConfigurationElement
    {
        [ConfigurationProperty("alias")]
        internal string Alias
        {
            get { return (string)base["alias"]; }
        }

        [ConfigurationProperty("log")]
        internal bool Log
        {
            get { return (bool)base["log"]; }
        }

        [ConfigurationProperty("interval")]
        internal int Interval
        {
            get { return (int)base["interval"]; }
        }

        [ConfigurationProperty("url")]
        internal string Url
        {
            get { return (string)base["url"]; }
        }
    }
}