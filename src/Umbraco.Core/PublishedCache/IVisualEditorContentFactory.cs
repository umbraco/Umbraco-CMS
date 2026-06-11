using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.PublishedCache;

/// <summary>
/// Builds an <see cref="IPublishedContent"/> for the visual editor preview: the requested document's
/// draft content with a set of unsaved property values overlaid on top, converted to their published form.
/// </summary>
public interface IVisualEditorContentFactory
{
    /// <summary>
    /// Resolves the draft content for <paramref name="documentKey"/> and returns a preview
    /// <see cref="IPublishedContent"/> whose overridden aliases yield the converted unsaved values.
    /// Returns <c>null</c> if the document does not exist.
    /// </summary>
    Task<IPublishedContent?> CreateWithOverridesAsync(
        Guid documentKey,
        IReadOnlyCollection<VisualEditorPropertyOverride> overrides);
}
