using System.Configuration;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    internal class LoggingElement : ConfigurationElement
    {
        [ConfigurationProperty("autoCleanLogs")]
        internal InnerTextConfigurationElement<bool> AutoCleanLogs
        {
            get
            {
                return new OptionalInnerTextConfigurationElement<bool>(
                    (InnerTextConfigurationElement<bool>)this["autoCleanLogs"],
                    //set the default
                    false);            
            }
        }

        [ConfigurationProperty("enableLogging")]
        internal InnerTextConfigurationElement<bool> EnableLogging
        {
            get { return (InnerTextConfigurationElement<bool>)this["enableLogging"]; }
        }

        [ConfigurationProperty("enableAsyncLogging")]
        internal InnerTextConfigurationElement<bool> EnableAsyncLogging
        {
            get { return (InnerTextConfigurationElement<bool>)this["enableAsyncLogging"]; }
        }

        [ConfigurationProperty("cleaningMiliseconds")]
        internal InnerTextConfigurationElement<int> CleaningMiliseconds
        {
            get
            {
                return new OptionalInnerTextConfigurationElement<int>(
                    (InnerTextConfigurationElement<int>)this["cleaningMiliseconds"],
                    //set the default
                    -1);                      
            }
        }

        [ConfigurationProperty("maxLogAge")]
        internal InnerTextConfigurationElement<int> MaxLogAge
        {
            get
            {
                return new OptionalInnerTextConfigurationElement<int>(
                    (InnerTextConfigurationElement<int>)this["maxLogAge"],
                    //set the default
                    -1);
            }
        }

        [ConfigurationCollection(typeof(DisabledLogTypesCollection), AddItemName = "logTypeAlias")]
        [ConfigurationProperty("disabledLogTypes", IsDefaultCollection = true)]
        public DisabledLogTypesCollection DisabledLogTypes
        {
            get { return (DisabledLogTypesCollection)base["disabledLogTypes"]; }
        }

        [ConfigurationProperty("externalLogger")]
        internal ExternalLoggerElement ExternalLogger
        {
            get { return (ExternalLoggerElement)this["externalLogger"]; }
        }
    }
}