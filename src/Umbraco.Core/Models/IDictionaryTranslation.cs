using System.Runtime.Serialization;
using Umbraco.Cms.Core.Models.Entities;

namespace Umbraco.Cms.Core.Models;

public interface IDictionaryTranslation : IEntity, IRememberBeingDirty
{
    string LanguageIsoCode { get; }

    /// <summary>
    ///     Gets or sets the translated text
    /// </summary>
    [DataMember]
    string Value { get; set; }
}
