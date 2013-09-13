using System.Collections.Generic;
using System.Configuration;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    internal class LoggingElement : ConfigurationElement, ILogging
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
            get
            {
                return new OptionalInnerTextConfigurationElement<bool>(
                    (InnerTextConfigurationElement<bool>)this["enableLogging"],
                    //set the default
                    true);            
            }
        }

        [ConfigurationProperty("enableAsyncLogging")]
        internal InnerTextConfigurationElement<bool> EnableAsyncLogging
        {
            get
            {
                return new OptionalInnerTextConfigurationElement<bool>(
                    (InnerTextConfigurationElement<bool>)this["enableAsyncLogging"],
                    //set the default
                    true);           
            }
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
        internal DisabledLogTypesCollection DisabledLogTypes
        {
            get { return (DisabledLogTypesCollection)base["disabledLogTypes"]; }
        }

        [ConfigurationProperty("externalLogger", IsRequired = false)]
        internal ExternalLoggerElement ExternalLogger
        {
            get { return (ExternalLoggerElement) base["externalLogger"]; }
        }

        public bool ExternalLoggerIsConfigured
        {
            get
            {
                var externalLoggerProperty = Properties["externalLogger"];
                var externalLogger = this[externalLoggerProperty] as ConfigurationElement;
                if (externalLogger != null && externalLogger.ElementInformation.IsPresent)
                {
                    return true;
                }
                return false;
            }
        }


        bool ILogging.AutoCleanLogs
        {
            get { return AutoCleanLogs; }
        }

        bool ILogging.EnableLogging
        {
            get { return EnableLogging; }
        }

        bool ILogging.EnableAsyncLogging
        {
            get { return EnableAsyncLogging; }
        }

        int ILogging.CleaningMiliseconds
        {
            get { return CleaningMiliseconds; }
        }

        int ILogging.MaxLogAge
        {
            get { return MaxLogAge; }
        }

        IEnumerable<ILogType> ILogging.DisabledLogTypes
        {
            get { return DisabledLogTypes; }
        }

        IExternalLogger ILogging.ExternalLogger
        {
            get { return ExternalLogger; }
        }

    }
}