namespace Umbraco.Cms.Core.Models.ContentEditing;

/// <summary>
///     Represents a model for validating content updates.
/// </summary>
public class ValidateContentUpdateModel : ContentUpdateModel
{
    /// <summary>
    ///     Gets or sets the set of cultures to validate, or <c>null</c> to validate all cultures.
    /// </summary>
    public ISet<string>? Cultures { get; set; }
}
