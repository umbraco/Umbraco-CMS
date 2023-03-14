using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Models.ContentEditing;

/// <summary>
///     The macro parameter display.
/// </summary>
[DataContract(Name = "parameter", Namespace = "")]
public class MacroParameterDisplay
{
    /// <summary>
    ///     Gets or sets the key.
    /// </summary>
    [DataMember(Name = "key")]
    public string Key { get; set; } = null!;

    /// <summary>
    ///     Gets or sets the label.
    /// </summary>
    [DataMember(Name = "label")]
    public string? Label { get; set; }

    /// <summary>
    ///     Gets or sets the editor.
    /// </summary>
    [DataMember(Name = "editor")]
    public string Editor { get; set; } = null!;

    /// <summary>
    ///     Gets or sets the id.
    /// </summary>
    [DataMember(Name = "id")]
    public int Id { get; set; }
}
