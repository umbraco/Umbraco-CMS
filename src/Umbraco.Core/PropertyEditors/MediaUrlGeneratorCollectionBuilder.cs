using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.PropertyEditors;

public class MediaUrlGeneratorCollectionBuilder : SetCollectionBuilderBase<MediaUrlGeneratorCollectionBuilder, MediaUrlGeneratorCollection, IMediaUrlGenerator>
{
    protected override MediaUrlGeneratorCollectionBuilder This => this;
}
