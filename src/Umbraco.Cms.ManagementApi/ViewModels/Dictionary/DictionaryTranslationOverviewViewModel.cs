using System.Runtime.Serialization;

namespace Umbraco.Cms.ManagementApi.ViewModels.Dictionary;

public class DictionaryTranslationOverviewViewModel
{
    /// <summary>
    ///     Gets or sets the display name.
    /// </summary>
    [DataMember(Name = "displayName")]
    public string? DisplayName { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether has translation.
    /// </summary>
    [DataMember(Name = "hasTranslation")]
    public bool HasTranslation { get; set; }
}
