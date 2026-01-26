namespace Umbraco.Cms.Core.Models.ContentEditing;

/// <summary>
/// Model for partial content updates (PATCH operations).
/// </summary>
public class ContentPatchModel
{
    /// <summary>
    /// Template to apply. Null means preserve existing.
    /// </summary>
    public Guid? TemplateKey { get; set; }

    /// <summary>
    /// Variants to patch. Only these variants will be modified.
    /// Null means preserve all existing variants.
    /// </summary>
    public IEnumerable<VariantPatchModel>? Variants { get; set; }

    /// <summary>
    /// Property values to patch. Only these properties will be modified.
    /// Null means preserve all existing property values.
    /// </summary>
    public IEnumerable<PropertyPatchModel>? Properties { get; set; }

    /// <summary>
    /// Cultures explicitly affected by this patch. Used for authorization checks.
    /// </summary>
    public IEnumerable<string> AffectedCultures { get; set; } = Array.Empty<string>();
}
