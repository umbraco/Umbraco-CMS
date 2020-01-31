using System;
using System.Runtime.Serialization;
using Umbraco.Core.Models.PublishedContent;

namespace Umbraco.Core.Models.Blocks
{
    /// <summary>
    /// Represents a layout item for the Block List editor
    /// </summary>
    [DataContract(Name = "blockListLayout", Namespace = "")]
    public class BlockListLayoutReference : IBlockElement<IPublishedElement>
    {
        public BlockListLayoutReference(Udi udi, IPublishedElement settings)
        {
            Udi = udi ?? throw new ArgumentNullException(nameof(udi));
            Settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        [DataMember(Name = "udi")]
        public Udi Udi { get; set; }

        [DataMember(Name = "settings")]
        public IPublishedElement Settings { get; set; }
    }
}
