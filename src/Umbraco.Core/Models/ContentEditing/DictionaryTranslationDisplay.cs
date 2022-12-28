using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Models.ContentEditing;

/// <inheritdoc />
/// <summary>
///     The dictionary translation display model
/// </summary>
[DataContract(Name = "dictionaryTranslation", Namespace = "")]
public class DictionaryTranslationDisplay : DictionaryTranslationSave
{
    [DataMember(Name = "id")]
    public int Id { get; set; }

    [DataMember(Name = "key")]
    public Guid Key { get; set; }

    /// <summary>
    ///     Gets or sets the display name.
    /// </summary>
    [DataMember(Name = "displayName")]
    public string? DisplayName { get; set; }
}
