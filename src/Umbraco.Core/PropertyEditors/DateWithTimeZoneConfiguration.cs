// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Text.Json.Serialization;

namespace Umbraco.Cms.Core.PropertyEditors;

public class DateWithTimeZoneConfiguration
{
    /// <summary>
    /// The format of the date and time value.
    /// </summary>
    [ConfigurationField("format")]
    public DateWithTimeZoneFormat Format { get; set; } = DateWithTimeZoneFormat.DateTime;

    /// <summary>
    /// The time zones configuration.
    /// </summary>
    [ConfigurationField("timeZones")]
    public DateWithTimeZoneTimeZones? TimeZones { get; set; }
}

public class DateWithTimeZoneTimeZones
{
    /// <summary>
    /// The mode for time zones.
    /// </summary>
    public DateWithTimeZoneMode? Mode { get; set; } = DateWithTimeZoneMode.None;

    /// <summary>
    /// A list of time zones to use when the mode is set to Custom.
    /// </summary>
    public List<string> TimeZones { get; set; } = [];
}

public enum DateWithTimeZoneMode
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

public enum DateWithTimeZoneFormat
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
