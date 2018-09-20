using System.Collections.Generic;
using System.Configuration;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    internal class LoggingElement : UmbracoConfigurationElement, ILoggingSection
    {

        [ConfigurationProperty("autoCleanLogs")]
        internal InnerTextConfigurationElement<bool> AutoCleanLogs
        {
            get { return GetOptionalTextElement("autoCleanLogs", false); }
        }

        [ConfigurationProperty("enableLogging")]
        internal InnerTextConfigurationElement<bool> EnableLogging
        {
            get { return GetOptionalTextElement("enableLogging", true); }
        }

        [ConfigurationProperty("enableAsyncLogging")]
        internal InnerTextConfigurationElement<bool> EnableAsyncLogging
        {
            get { return GetOptionalTextElement("enableAsyncLogging", true); }
        }

        [ConfigurationProperty("cleaningMiliseconds")]
        internal InnerTextConfigurationElement<int> CleaningMiliseconds
        {
            get { return GetOptionalTextElement("cleaningMiliseconds", -1); }
        }

        [ConfigurationProperty("maxLogAge")]
        internal InnerTextConfigurationElement<int> MaxLogAge
        {
            get { return GetOptionalTextElement("maxLogAge", -1); }
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

        bool ILoggingSection.AutoCleanLogs
        {
            get { return AutoCleanLogs; }
        }

        bool ILoggingSection.EnableLogging
        {
            get { return EnableLogging; }
        }

        int ILoggingSection.CleaningMiliseconds
        {
            get { return CleaningMiliseconds; }
        }

        int ILoggingSection.MaxLogAge
        {
            get { return MaxLogAge; }
        }

        IEnumerable<ILogType> ILoggingSection.DisabledLogTypes
        {
            get { return DisabledLogTypes; }
        }

    }
}
