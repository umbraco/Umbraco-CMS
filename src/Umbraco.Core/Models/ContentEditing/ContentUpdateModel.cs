namespace Umbraco.Cms.Core.Models.ContentEditing;

/// <summary>
///     Represents a model for updating content.
/// </summary>
public class ContentUpdateModel : ContentEditingModelBase
{
    /// <summary>
    ///     Gets or sets the optional template key to associate with the content.
    /// </summary>
    public Guid? TemplateKey { get; set; }
}
