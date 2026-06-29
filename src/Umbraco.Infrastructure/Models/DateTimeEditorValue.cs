using System.Runtime.Serialization;

namespace Umbraco.Cms.Infrastructure.Models;

/// <summary>
/// Represents the value stored by a date and time editor in Umbraco.
/// </summary>
[DataContract]
public class DateTimeEditorValue
{
    /// <summary>
    /// Gets or sets the date component of the editor value as a string, typically representing only the date part (e.g., "yyyy-MM-dd").
    /// </summary>
    [DataMember(Name = "date")]
    public string? Date { get; set; }

    /// <summary>
    /// Gets or sets the time zone identifier associated with the date and time value.
    /// </summary>
    [DataMember(Name = "timeZone")]
    public string? TimeZone { get; set; }
}
