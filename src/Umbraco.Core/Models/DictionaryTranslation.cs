using System.Runtime.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Core.Models;

/// <summary>
///     Represents a translation for a <see cref="DictionaryItem" />
/// </summary>
[Serializable]
[DataContract(IsReference = true)]
public class DictionaryTranslation : EntityBase, IDictionaryTranslation
{
    private ILanguage? _language;
    private string? _languageIsoCode;

    // note: this will be memberwise cloned
    private string _value;

    public DictionaryTranslation(ILanguage language, string value)
    {
        _language = language ?? throw new ArgumentNullException("language");
        LanguageId = _language.Id;
        _value = value;
        LanguageIsoCode = language.IsoCode;
    }

    public DictionaryTranslation(ILanguage language, string value, Guid uniqueId)
    {
        _language = language ?? throw new ArgumentNullException("language");
        LanguageId = _language.Id;
        _value = value;
        LanguageIsoCode = language.IsoCode;
        Key = uniqueId;
    }

    [Obsolete("Please use constructor that accepts ILanguage. This will be removed in V14.")]
    public DictionaryTranslation(int languageId, string value)
    {
        LanguageId = languageId;
        _value = value;
    }

    [Obsolete("Please use constructor that accepts ILanguage. This will be removed in V14.")]
    public DictionaryTranslation(int languageId, string value, Guid uniqueId)
    {
        LanguageId = languageId;
        _value = value;
        Key = uniqueId;
    }

    public Func<int, ILanguage?>? GetLanguage { get; set; }

    /// <summary>
    ///     Gets or sets the <see cref="Language" /> for the translation
    /// </summary>
    /// <remarks>
    ///     Marked as DoNotClone - TODO: this member shouldn't really exist here in the first place, the DictionaryItem
    ///     class will have a deep hierarchy of objects which all get deep cloned which we don't want. This should have simply
    ///     just referenced a language ID not the actual language object. In v8 we need to fix this.
    ///     We're going to have to do the same hacky stuff we had to do with the Template/File contents so that this is
    ///     returned
    ///     on a callback.
    /// </remarks>
    [Obsolete("This will be removed in V14. From V14 onwards you should get languages by ISO code from ILanguageService.")]
    [DataMember]
    [DoNotClone]
    public ILanguage? Language
    {
        get
        {
            if (_language != null)
            {
                return _language;
            }

            // else, must lazy-load
            if (GetLanguage != null && LanguageId > 0)
            {
                _language = GetLanguage(LanguageId);
            }

            return _language;
        }

        set
        {
            SetPropertyValueAndDetectChanges(value, ref _language, nameof(Language));
            LanguageId = _language == null ? -1 : _language.Id;
        }
    }

    [Obsolete("This will be replaced by language ISO code in V14.")]
    public int LanguageId { get; private set; }

    /// <summary>
    ///     Gets or sets the translated text
    /// </summary>
    [DataMember]
    public string Value
    {
        get => _value;
        set => SetPropertyValueAndDetectChanges(value, ref _value!, nameof(Value));
    }

    /// <inheritdoc />
    public string LanguageIsoCode
    {
        get
        {
            // TODO: this won't be necessary after obsoleted ctors are removed in v14.
            if (_languageIsoCode is null)
            {
                var _languageService = StaticServiceProvider.Instance.GetRequiredService<ILocalizationService>();
                _languageIsoCode = _languageService.GetLanguageById(LanguageId)?.IsoCode ?? string.Empty;
            }

            return _languageIsoCode;
        }

        private set => SetPropertyValueAndDetectChanges(value, ref _languageIsoCode!, nameof(LanguageIsoCode));
    }

    protected override void PerformDeepClone(object clone)
    {
        base.PerformDeepClone(clone);

        var clonedEntity = (DictionaryTranslation)clone;

        // clear fields that were memberwise-cloned and that we don't want to clone
        clonedEntity._language = null;
    }
}
