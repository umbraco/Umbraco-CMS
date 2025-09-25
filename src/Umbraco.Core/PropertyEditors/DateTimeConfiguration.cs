// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Text.Json.Serialization;

namespace Umbraco.Cms.Core.PropertyEditors;

public class DateTimeConfiguration
{
    /// <summary>
    /// Gets or sets the time zones configuration.
    /// </summary>
    [ConfigurationField("timeZones")]
    public TimeZonesConfiguration? TimeZones { get; set; }

    public class TimeZonesConfiguration
    {
        /// <summary>
        /// The mode for time zones.
        /// </summary>
        public TimeZoneMode Mode { get; set; }

        /// <summary>
        /// A list of time zones to use when the mode is set to Custom.
        /// </summary>
        public List<string> TimeZones { get; set; } = [];
    }

    public enum TimeZoneMode
    {
        /// <summary>
        /// Display all time zones.
        /// </summary>
        [JsonStringEnumMemberName("all")]
        All,

        /// <summary>
        /// Display only the local time zone of the user.
        /// </summary>
        [JsonStringEnumMemberName("local")]
        Local,

        /// <summary>
        /// Display a custom list of time zones defined in the configuration.
        /// </summary>
        [JsonStringEnumMemberName("custom")]
        Custom,
    }
}
