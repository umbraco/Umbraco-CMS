namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents the configuration for the flexible dropdown property editor.
/// </summary>
public class DropDownFlexibleConfiguration : ValueListConfiguration
{
    /// <summary>
    ///     Gets or sets a value indicating whether multiple selections are allowed.
    /// </summary>
    [Obsolete("A dropdown holding a single value is now its own property editor. Scheduled for removal in Umbraco 21.")]
    [ConfigurationField("multiple")]
    public bool Multiple { get; set; }
}
