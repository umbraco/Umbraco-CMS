using System.Runtime.Serialization;

namespace Umbraco.Cms.Infrastructure.Models;

[DataContract]
public class DateTimeEditorValue
{
    [DataMember(Name = "date")]
    public string? Date { get; set; }

    [DataMember(Name = "timeZone")]
    public string? TimeZone { get; set; }
}
