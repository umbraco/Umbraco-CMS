using System.Runtime.Serialization;
using Umbraco.Cms.Core.Models.ContentEditing;

namespace Umbraco.Cms.ManagementApi.ViewModels.Dictionary;

/// <summary>
///     The dictionary display model
/// </summary>
[DataContract(Name = "dictionary", Namespace = "")]
public class DictionaryViewModel : EntityBasic, INotificationModel
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="DictionaryDisplay" /> class.
    /// </summary>
    public DictionaryViewModel()
    {
        Notifications = new List<BackOfficeNotification>();
        Translations = new List<DictionaryTranslationDisplay>();
        ContentApps = new List<ContentApp>();
    }

    /// <summary>
    ///     Gets or sets the parent id.
    /// </summary>
    [DataMember(Name = "parentId")]
    public new Guid? ParentId { get; set; }

    /// <summary>
    ///     Gets the translations.
    /// </summary>
    [DataMember(Name = "translations")]
    public List<DictionaryTranslationDisplay> Translations { get; private set; }

    /// <summary>
    ///     Apps for the dictionary item
    /// </summary>
    [DataMember(Name = "apps")]
    public List<ContentApp> ContentApps { get; private set; }

    /// <inheritdoc />
    /// <summary>
    ///     This is used to add custom localized messages/strings to the response for the app to use for localized UI purposes.
    /// </summary>
    [DataMember(Name = "notifications")]
    public List<BackOfficeNotification> Notifications { get; private set; }

    /// <summary>
    ///     Gets or sets a value indicating whether name is dirty.
    /// </summary>
    [DataMember(Name = "nameIsDirty")]
    public bool NameIsDirty { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating when the object was created.
    /// </summary>
    [DataMember(Name = "createDate")]
    public DateTime CreateDate { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating when the object was updated.
    /// </summary>
    [DataMember(Name = "updateDate")]
    public DateTime UpdateDate { get; set; }
}
