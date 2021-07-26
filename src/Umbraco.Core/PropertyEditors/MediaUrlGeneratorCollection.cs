using System.Collections.Generic;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.PropertyEditors
{
    public class MediaUrlGeneratorCollection : BuilderCollectionBase<IMediaUrlGenerator>
    {
        public MediaUrlGeneratorCollection(System.Func<IEnumerable<IMediaUrlGenerator>> items) : base(items)
        {
        }

        public bool TryGetMediaPath(string propertyEditorAlias, object value, out string mediaPath)
        {
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
