using System.ComponentModel.DataAnnotations;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.Validation;

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
    public Guid? ParentId { get; set; }

    /// <summary>
    ///     Gets or sets the translations.
    /// </summary>
    public IEnumerable<DictionaryTranslationViewModel> Translations { get; set; } = Enumerable.Empty<DictionaryTranslationViewModel>();

    /// <summary>
    ///     Apps for the dictionary item
    /// </summary>
    public IEnumerable<ContentApp> ContentApps { get; set; }

    /// <inheritdoc />
    /// <summary>
    ///     This is used to add custom localized messages/strings to the response for the app to use for localized UI purposes.
    /// </summary>
    public List<BackOfficeNotification> Notifications { get; private set; }

    [RequiredForPersistence(AllowEmptyStrings = false, ErrorMessage = "Required")]
    [Required]
    public string Name { get; set; } = null!;

    /// <summary>
    ///     Gets or sets the Key for the object
    /// </summary>
    public Guid Key { get; set; }

    /// <summary>
    ///     The path of the entity
    /// </summary>
    public string Path { get; set; } = string.Empty;
}
