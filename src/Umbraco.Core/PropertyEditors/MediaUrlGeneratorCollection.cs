using System.Collections.Generic;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.PropertyEditors
{
    public class MediaUrlGeneratorCollection : BuilderCollectionBase<IMediaUrlGenerator>
    {
        public MediaUrlGeneratorCollection(IEnumerable<IMediaUrlGenerator> items) : base(items)
        {
        }

        public bool TryGetMediaPath(string propertyEditorAlias, object value, out string mediaPath)
        {
            // We can't get a media path from a null value
            // The value will be null when uploading a brand new image, since we try to get the "old path" which doesn't exist yet.
            if (value is null)
            {
                mediaPath = null;
                return false;
            }

            foreach(IMediaUrlGenerator generator in this)
            {
                if (generator.TryGetMediaPath(propertyEditorAlias, value, out var mp))
                {
                    mediaPath = mp;
                    return true;
                }
            }
            mediaPath = null;
            return false;
        }


    }
}
