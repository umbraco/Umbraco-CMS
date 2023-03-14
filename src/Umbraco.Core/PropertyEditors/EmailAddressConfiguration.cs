namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents the configuration for the email address value editor.
/// </summary>
public class EmailAddressConfiguration
{
    [ConfigurationField("IsRequired", "Required?", "hidden", Description = "Deprecated; Make this required by selecting mandatory when adding to the document type")]
    [Obsolete("No longer used, use `Mandatory` for the property instead. Will be removed in the next major version")]
    public bool IsRequired { get; set; }
}
