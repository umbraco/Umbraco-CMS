using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Models.ContentEditing;

/// <summary>
///     Represents a tab in the UI
/// </summary>
[DataContract(Name = "tab", Namespace = "")]
public class Tab<T>
{
    [DataMember(Name = "id")]
    public int Id { get; set; }

    [DataMember(Name = "key")]
    public Guid Key { get; set; }

    [DataMember(Name = "type")]
    public string? Type { get; set; }

    [DataMember(Name = "active")]
    public bool IsActive { get; set; }

    [DataMember(Name = "label")]
    public string? Label { get; set; }

    [DataMember(Name = "alias")]
    public string? Alias { get; set; }

    /// <summary>
    ///     The expanded state of the tab
    /// </summary>
    [DataMember(Name = "open")]
    public bool Expanded { get; set; } = true;

    [DataMember(Name = "properties")]
    public IEnumerable<T>? Properties { get; set; }
}
