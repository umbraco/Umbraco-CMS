using System.Runtime.Serialization;
using Umbraco.Cms.Core.Models.Entities;

namespace Umbraco.Cms.Core.Models;

public interface IDictionaryItem : IEntity, IRememberBeingDirty
{
    /// <summary>
    ///     Gets or Sets the Parent Id of the Dictionary Item
    /// </summary>
    [DataMember]
    Guid? ParentId { get; set; }

    /// <summary>
    ///     Gets or sets the Key for the Dictionary Item
    /// </summary>
    [DataMember]
    string ItemKey { get; set; }

    /// <summary>
    ///     Gets or sets a list of translations for the Dictionary Item
    /// </summary>
    [DataMember]
    IEnumerable<IDictionaryTranslation> Translations { get; set; }
}
