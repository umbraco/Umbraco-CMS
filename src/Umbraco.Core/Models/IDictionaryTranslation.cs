using System.Runtime.Serialization;
using Umbraco.Cms.Core.Models.Entities;

namespace Umbraco.Cms.Core.Models;

public interface IDictionaryTranslation : IEntity, IRememberBeingDirty
{
    /// <summary>
    ///     Gets or sets the <see cref="Language" /> for the translation.
    /// </summary>
    [Obsolete("This will be removed in V14. From V14 onwards you should get languages by ISO code from ILanguageService.")]
    [DataMember]
    ILanguage? Language { get; set; }

    [Obsolete("This will be replaced by language ISO code in V14.")]
    int LanguageId { get; }

    /// <summary>
    ///     Gets or sets the translated text.
    /// </summary>
    [DataMember]
    string Value { get; set; }

    /// <summary>
    ///     Gets the ISO code of the language.
    /// </summary>
    [DataMember]
    string LanguageIsoCode => Language?.IsoCode ?? string.Empty;
}
