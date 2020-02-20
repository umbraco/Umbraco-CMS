using Umbraco.Core.Composing;

namespace Umbraco.Core.PropertyEditors
{
    public class MediaUrlGeneratorCollectionBuilder : LazyCollectionBuilderBase<MediaUrlGeneratorCollectionBuilder, MediaUrlGeneratorCollection, IMediaUrlGenerator>
    {
        protected override MediaUrlGeneratorCollectionBuilder This => this;
    }
}
