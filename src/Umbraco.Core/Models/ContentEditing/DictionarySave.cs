using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Models.ContentEditing;

/// <summary>
///     Dictionary Save model
/// </summary>
[DataContract(Name = "dictionary", Namespace = "")]
public class DictionarySave : EntityBasic
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="DictionarySave" /> class.
    /// </summary>
    public DictionarySave() => Translations = new List<DictionaryTranslationSave>();

    /// <summary>
    ///     Gets or sets a value indicating whether name is dirty.
    /// </summary>
    [DataMember(Name = "nameIsDirty")]
    public bool NameIsDirty { get; set; }

    /// <summary>
    ///     Gets the translations.
    /// </summary>
    [DataMember(Name = "translations")]
    public List<DictionaryTranslationSave> Translations { get; private set; }

    /// <summary>
    ///     Gets or sets the parent id.
    /// </summary>
    [DataMember(Name = "parentId")]
    public new Guid ParentId { get; set; }
}
