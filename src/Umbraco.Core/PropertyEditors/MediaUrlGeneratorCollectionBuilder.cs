using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
/// Represents a builder for the <see cref="MediaUrlGeneratorCollection"/>.
/// </summary>
public class MediaUrlGeneratorCollectionBuilder : SetCollectionBuilderBase<MediaUrlGeneratorCollectionBuilder, MediaUrlGeneratorCollection, IMediaUrlGenerator>
{
    /// <inheritdoc />
    protected override MediaUrlGeneratorCollectionBuilder This => this;
}
