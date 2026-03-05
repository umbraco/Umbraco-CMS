using System.Runtime.Serialization;
using Umbraco.Cms.Core.Models.Entities;

namespace Umbraco.Cms.Core.Models;

/// <summary>
///     Represents a translation for a <see cref="DictionaryItem" />
/// </summary>
[Serializable]
[DataContract(IsReference = true)]
public class DictionaryTranslation : EntityBase, IDictionaryTranslation
{
    // note: this will be memberwise cloned
    private string _value;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DictionaryTranslation" /> class.
    /// </summary>
    /// <param name="language">The language for this translation.</param>
    /// <param name="value">The translated text.</param>
    public DictionaryTranslation(ILanguage language, string value)
    {
        LanguageIsoCode = language.IsoCode;
        _value = value;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="DictionaryTranslation" /> class with a unique identifier.
    /// </summary>
    /// <param name="language">The language for this translation.</param>
    /// <param name="value">The translated text.</param>
    /// <param name="uniqueId">The unique identifier for the translation.</param>
    public DictionaryTranslation(ILanguage language, string value, Guid uniqueId)
        : this(language, value) =>
        Key = uniqueId;

    /// <inheritdoc />
    public string LanguageIsoCode { get; private set; }

    /// <summary>
    ///     Gets or sets the translated text
    /// </summary>
    [DataMember]
    public string Value
    {
        get => _value;
        set => SetPropertyValueAndDetectChanges(value, ref _value!, nameof(Value));
    }
}
