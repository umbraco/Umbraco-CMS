using System.Collections.Generic;
using Umbraco.Core.Composing;

namespace Umbraco.Core.PropertyEditors
{
    public class MediaUrlGeneratorCollection : BuilderCollectionBase<IMediaUrlGenerator>
    {
        public MediaUrlGeneratorCollection(IEnumerable<IMediaUrlGenerator> items) : base(items)
        {
        }

        public bool TryGetMediaPath(string alias, object value, out string mediaPath)
        {
            foreach(var generator in this)
            {
                if (generator.TryGetMediaPath(alias, value, out var mp))
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
