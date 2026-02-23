namespace Umbraco.Cms.Core.Models.ContentEditing;

/// <summary>
///     Represents a model for creating content.
/// </summary>
public class ContentCreateModel : ContentCreationModelBase
{
    /// <summary>
    ///     Gets or sets the optional template key to associate with the content.
    /// </summary>
    public Guid? TemplateKey { get; set; }
}
