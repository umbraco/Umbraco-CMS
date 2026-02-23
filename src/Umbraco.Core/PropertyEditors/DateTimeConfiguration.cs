// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Text.Json.Serialization;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents the configuration for the DateTime property editor.
/// </summary>
public class DateTimeConfiguration
{
    /// <summary>
    /// Gets or sets the time zones configuration.
    /// </summary>
    [ConfigurationField("timeZones")]
    public TimeZonesConfiguration? TimeZones { get; set; }

    /// <summary>
    /// Represents the time zone configuration options for the DateTime property editor.
    /// </summary>
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

    /// <summary>
    /// Specifies the time zone display mode for the DateTime property editor.
    /// </summary>
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
