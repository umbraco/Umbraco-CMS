using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Configuration.HealthChecks;

namespace Umbraco.Core.Configuration.Models
{
    public class HealthChecksSettings
    {
        // TODO: implement
        public IEnumerable<IDisabledHealthCheck> DisabledChecks { get; set; } = Enumerable.Empty<IDisabledHealthCheck>();

        public HealthCheckNotificationSettings NotificationSettings { get; set; } = new HealthCheckNotificationSettings();

        /*
        public IEnumerable<IDisabledHealthCheck> DisabledChecks => _configuration
            .GetSection(Prefix+"DisabledChecks")
            .GetChildren()
            .Select(
                x => new DisabledHealthCheck
                {
                    Id = x.GetValue<Guid>("Id"),
                    DisabledOn = x.GetValue<DateTime>("DisabledOn"),
                    DisabledBy = x.GetValue<int>("DisabledBy")
                });

        public IHealthCheckNotificationSettings NotificationSettings =>
            new HealthCheckNotificationSettings(
                _configuration.GetSection(Prefix+"NotificationSettings"));


        private class DisabledHealthCheck : IDisabledHealthCheck
        {
            public Guid Id { get; set; }
            public DateTime DisabledOn { get; set; }
            public int DisabledBy { get; set; }
        }
        */

        // TODO: move to new file
        public class HealthCheckNotificationSettings
        {
            public bool Enabled { get; set; } = false;

            public string FirstRunTime { get; set; }

            public int PeriodInHours { get; set; } = 24;

            // TODO: implement

            public IReadOnlyDictionary<string, INotificationMethod> NotificationMethods { get; set; } = new Dictionary<string, INotificationMethod>();

            public IEnumerable<IDisabledHealthCheck> DisabledChecks { get; set; } = Enumerable.Empty<IDisabledHealthCheck>();

            /*
            public IReadOnlyDictionary<string, INotificationMethod> NotificationMethods => _configurationSection
                .GetSection("NotificationMethods")
                .GetChildren()
                .ToDictionary(x => x.Key, x => (INotificationMethod) new NotificationMethod(x.Key, x), StringComparer.InvariantCultureIgnoreCase);

            public IEnumerable<IDisabledHealthCheck> DisabledChecks => _configurationSection
                .GetSection("DisabledChecks").GetChildren().Select(
                    x => new DisabledHealthCheck
                    {
                        Id = x.GetValue<Guid>("Id"),
                        DisabledOn = x.GetValue<DateTime>("DisabledOn"),
                        DisabledBy = x.GetValue<int>("DisabledBy")
                    });
            */
        }

        /*
        private class NotificationMethod : INotificationMethod
        {
            private readonly IConfigurationSection _configurationSection;

            public NotificationMethod(string alias, IConfigurationSection configurationSection)
            {
                Alias = alias;
                _configurationSection = configurationSection;
            }

            public string Alias { get; }
            public bool Enabled => _configurationSection.GetValue("Enabled", false);

            public HealthCheckNotificationVerbosity Verbosity =>
                _configurationSection.GetValue("Verbosity", HealthCheckNotificationVerbosity.Summary);

            public bool FailureOnly => _configurationSection.GetValue("FailureOnly", true);

            public IReadOnlyDictionary<string, INotificationMethodSettings> Settings => _configurationSection
                .GetSection("Settings").GetChildren().ToDictionary(x => x.Key,
                    x => (INotificationMethodSettings) new NotificationMethodSettings(x.Key, x.Value), StringComparer.InvariantCultureIgnoreCase);
        }

        private class NotificationMethodSettings : INotificationMethodSettings
        {
            public NotificationMethodSettings(string key, string value)
            {
                Key = key;
                Value = value;
            }

            public string Key { get; }
            public string Value { get; }
        }
        */
    }
}
