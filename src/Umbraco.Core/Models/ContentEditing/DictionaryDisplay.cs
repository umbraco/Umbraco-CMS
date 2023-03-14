using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Models.ContentEditing;

/// <summary>
///     The dictionary display model
/// </summary>
[DataContract(Name = "dictionary", Namespace = "")]
public class DictionaryDisplay : EntityBasic, INotificationModel
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="DictionaryDisplay" /> class.
    /// </summary>
    public DictionaryDisplay()
    {
        Notifications = new List<BackOfficeNotification>();
        Translations = new List<DictionaryTranslationDisplay>();
        ContentApps = new List<ContentApp>();
    }

    /// <summary>
    ///     Gets or sets the parent id.
    /// </summary>
    [DataMember(Name = "parentId")]
    public new Guid ParentId { get; set; }

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
}
