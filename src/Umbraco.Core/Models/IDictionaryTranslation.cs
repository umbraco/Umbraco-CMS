using System.Runtime.Serialization;
using Umbraco.Cms.Core.Models.Entities;

namespace Umbraco.Cms.Core.Models;

public interface IDictionaryTranslation : IEntity, IRememberBeingDirty
{
    /// <summary>
    ///     Gets or sets the <see cref="Language" /> for the translation
    /// </summary>
    [DataMember]
    ILanguage? Language { get; set; }

    int LanguageId { get; }

    /// <summary>
    ///     Gets or sets the translated text
    /// </summary>
    [DataMember]
    string Value { get; set; }
}
