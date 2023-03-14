using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.PropertyEditors;

public class MediaUrlGeneratorCollection : BuilderCollectionBase<IMediaUrlGenerator>
{
    public MediaUrlGeneratorCollection(Func<IEnumerable<IMediaUrlGenerator>> items)
        : base(items)
    {
    }

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
