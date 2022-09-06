using System.ComponentModel.DataAnnotations;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.Validation;
using System.Text.Json.Serialization;

namespace Umbraco.Cms.ManagementApi.ViewModels.Dictionary;

/// <summary>
///     The dictionary display model
/// </summary>
public class DictionaryViewModel : INotificationModel
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="DictionaryDisplay" /> class.
    /// </summary>
    public DictionaryViewModel()
    {
        Notifications = new List<BackOfficeNotification>();
        Translations = new List<DictionaryTranslationViewModel>();
        ContentApps = new List<ContentApp>();
    }

    /// <summary>
    ///     Gets or sets the parent id.
    /// </summary>
    [JsonPropertyName("parentId")]
    public Guid? ParentId { get; set; }

    /// <summary>
    ///     Gets or sets the translations.
    /// </summary>
    [JsonPropertyName("translations")]
    public IEnumerable<DictionaryTranslationViewModel> Translations { get; set; }

    /// <summary>
    ///     Apps for the dictionary item
    /// </summary>
    [JsonPropertyName("apps")]
    public IEnumerable<ContentApp> ContentApps { get; set; }

    /// <inheritdoc />
    /// <summary>
    ///     This is used to add custom localized messages/strings to the response for the app to use for localized UI purposes.
    /// </summary>
    [JsonPropertyName("notifications")]
    public List<BackOfficeNotification> Notifications { get; private set; }

    /// <summary>
    ///     Gets or sets a value indicating whether name is dirty.
    /// </summary>
    [JsonPropertyName("nameIsDirty")]
    public bool NameIsDirty { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating when the object was created.
    /// </summary>
    [JsonPropertyName("createDate")]
    public DateTime CreateDate { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating when the object was updated.
    /// </summary>
    [JsonPropertyName("updateDate")]
    public DateTime UpdateDate { get; set; }

    [JsonPropertyName("name")]
    [RequiredForPersistence(AllowEmptyStrings = false, ErrorMessage = "Required")]
    [Required]
    public string Name { get; set; } = null!;

    /// <summary>
    ///     Gets or sets the Key for the object
    /// </summary>
    [JsonPropertyName("key")]
    public Guid Key { get; set; }

    /// <summary>
    ///     The path of the entity
    /// </summary>
    [JsonPropertyName("path")]
    public string Path { get; set; } = string.Empty;
}
