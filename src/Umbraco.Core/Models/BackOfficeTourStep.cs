using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Models;

/// <summary>
///     A model representing a step in a tour.
/// </summary>
[DataContract(Name = "step", Namespace = "")]
public class BackOfficeTourStep
{
    [DataMember(Name = "title")]
    public string? Title { get; set; }

    [DataMember(Name = "content")]
    public string? Content { get; set; }

    [DataMember(Name = "type")]
    public string? Type { get; set; }

    [DataMember(Name = "element")]
    public string? Element { get; set; }

    [DataMember(Name = "elementPreventClick")]
    public bool ElementPreventClick { get; set; }

    [DataMember(Name = "backdropOpacity")]
    public float? BackdropOpacity { get; set; }

    [DataMember(Name = "event")]
    public string? Event { get; set; }

    [DataMember(Name = "view")]
    public string? View { get; set; }

    [DataMember(Name = "eventElement")]
    public string? EventElement { get; set; }

    [DataMember(Name = "customProperties")]
    public object? CustomProperties { get; set; }

    [DataMember(Name = "skipStepIfVisible")]
    public string? SkipStepIfVisible { get; set; }
}
