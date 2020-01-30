using System;
using Umbraco.Core.Models.PublishedContent;

namespace Umbraco.Core.Models.Blocks
{
    /// <summary>
    /// Represents a layout item for the Block List editor
    /// </summary>
    public class BlockListLayoutReference : IBlockElement<IPublishedElement>
    {
        public BlockListLayoutReference(Udi udi, IPublishedElement settings)
        {
            Udi = udi ?? throw new ArgumentNullException(nameof(udi));
            Settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        public Udi Udi { get; }
        public IPublishedElement Settings { get; }
    }
}
