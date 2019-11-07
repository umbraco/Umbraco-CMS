using System;
using System.Collections.Generic;
using System.Configuration;

namespace Umbraco.Core.Configuration.HealthChecks
{
    public class NotificationMethodElement : ConfigurationElement, INotificationMethod
    {
        private const string AliasKey = "alias";
        private const string EnabledKey = "enabled";
        private const string VerbosityKey = "verbosity";
        private const string FailureonlyKey = "failureOnly";
        private const string SettingsKey = "settings";

        [ConfigurationProperty(AliasKey, IsKey = true, IsRequired = true)]
        public string Alias
        {
            get
            {
                return (string)base[AliasKey];
            }
        }

        [ConfigurationProperty(EnabledKey, IsKey = true, IsRequired = true)]
        public bool Enabled
        {
            get
            {
                return (bool)base[EnabledKey];
            }
        }

        [ConfigurationProperty(VerbosityKey, IsRequired = true)]
        public HealthCheckNotificationVerbosity Verbosity
        {
            get
            {
                return (HealthCheckNotificationVerbosity)base[VerbosityKey];
            }
        }

        [ConfigurationProperty(FailureonlyKey, IsRequired = false)]
        public bool FailureOnly
        {
            get
            {
                return (bool)base[FailureonlyKey];
            }
        }

        [ConfigurationProperty(SettingsKey, IsDefaultCollection = true, IsRequired = false)]
        public NotificationMethodSettingsElementCollection Settings
        {
            get
            {
                return (NotificationMethodSettingsElementCollection)base[SettingsKey];
            }
        }

        string INotificationMethod.Alias
        {
            get { return Alias; }
        }

        bool INotificationMethod.Enabled
        {
            get { return Enabled; }
        }

        HealthCheckNotificationVerbosity INotificationMethod.Verbosity
        {
            get { return Verbosity; }
        }

        bool INotificationMethod.FailureOnly
        {
            get { return FailureOnly; }
        }

        IReadOnlyDictionary<string, INotificationMethodSettings> INotificationMethod.Settings
        {
            get { return Settings; }
        }
    }
}
