// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Text.Json.Serialization;

namespace Umbraco.Cms.Core.PropertyEditors;

public class DateTime2Configuration
{
    /// <summary>
    /// Gets or sets the format of the date and time value.
    /// </summary>
    [ConfigurationField("format")]
    public DateTimeFormat Format { get; set; } = DateTimeFormat.DateTime;

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
        public TimeZoneMode? Mode { get; set; } = TimeZoneMode.None;

        /// <summary>
        /// A list of time zones to use when the mode is set to Custom.
        /// </summary>
        public List<string> TimeZones { get; set; } = [];
    }

    public enum TimeZoneMode
    {
        /// <summary>
        /// Do not display any time zones.
        /// </summary>
        [JsonStringEnumMemberName("none")]
        None,

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

    public enum DateTimeFormat
    {
        /// <summary>
        /// Display the date only, without time.
        /// </summary>
        [JsonStringEnumMemberName("date-only")]
        DateOnly,

        /// <summary>
        /// Display the time only, without date.
        /// </summary>
        [JsonStringEnumMemberName("time-only")]
        TimeOnly,

        /// <summary>
        /// Display both date and time.
        /// </summary>
        [JsonStringEnumMemberName("date-time")]
        DateTime,
    }
}
