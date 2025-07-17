// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Core.PropertyEditors;

public class DateTimeWithTimeZoneConfiguration
{
    [ConfigurationField("timeZones")]
    public List<string> TimeZones { get; set; } = [];
}
