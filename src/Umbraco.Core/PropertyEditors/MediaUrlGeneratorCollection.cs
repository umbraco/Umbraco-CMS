using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
/// Represents a collection of <see cref="IMediaUrlGenerator"/> instances.
/// </summary>
public class MediaUrlGeneratorCollection : BuilderCollectionBase<IMediaUrlGenerator>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MediaUrlGeneratorCollection"/> class.
    /// </summary>
    /// <param name="items">A factory function that returns the collection items.</param>
    public MediaUrlGeneratorCollection(Func<IEnumerable<IMediaUrlGenerator>> items)
        : base(items)
    {
    }

    /// <summary>
    /// Tries to get the media path from the specified value using the registered generators.
    /// </summary>
    /// <param name="propertyEditorAlias">The property editor alias.</param>
    /// <param name="value">The property value.</param>
    /// <param name="mediaPath">When this method returns, contains the media path if found; otherwise, null.</param>
    /// <returns><c>true</c> if a media path was found; otherwise, <c>false</c>.</returns>
    public bool TryGetMediaPath(string? propertyEditorAlias, object? value, out string? mediaPath)
    {
        // We can't get a media path from a null value
        // The value will be null when uploading a brand new image, since we try to get the "old path" which doesn't exist yet
        if (value is not null)
        {
            foreach (IMediaUrlGenerator generator in this)
            {
                if (generator.TryGetMediaPath(propertyEditorAlias, value, out var generatorMediaPath))
                {
                    mediaPath = generatorMediaPath;
                    return true;
                }
            }
        }

        mediaPath = null;
        return false;
    }
}
