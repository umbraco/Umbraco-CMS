using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Models;

/// <summary>
///     A model representing a tour.
/// </summary>
[DataContract(Name = "tour", Namespace = "")]
public class BackOfficeTour
{
    public BackOfficeTour() => RequiredSections = new List<string>();

    [DataMember(Name = "name")]
    public string? Name { get; set; }

    [DataMember(Name = "alias")]
    public string Alias { get; set; } = null!;

    [DataMember(Name = "group")]
    public string? Group { get; set; }

    [DataMember(Name = "groupOrder")]
    public int GroupOrder { get; set; }

    [DataMember(Name = "hidden")]
    public bool Hidden { get; set; }

    [DataMember(Name = "allowDisable")]
    public bool AllowDisable { get; set; }

    [DataMember(Name = "requiredSections")]
    public List<string> RequiredSections { get; set; }

    [DataMember(Name = "steps")]
    public BackOfficeTourStep[]? Steps { get; set; }

    [DataMember(Name = "culture")]
    public string? Culture { get; set; }

    [DataMember(Name = "contentType")]
    public string? ContentType { get; set; }
}
