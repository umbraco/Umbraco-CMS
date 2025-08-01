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

    [ConfigurationField("timeZones")]
    public object? TimeZones { get; set; }
}

public enum DateWithTimeZoneFormat
{
    [JsonStringEnumMemberName("date-only")]
    DateOnly,

    [JsonStringEnumMemberName("time-only")]
    TimeOnly,

    [JsonStringEnumMemberName("date-time")]
    DateTime,
}
