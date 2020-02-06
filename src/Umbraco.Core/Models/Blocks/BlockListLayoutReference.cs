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
            Settings = settings; // can be null
        }

        /// <summary>
        /// The Id of the data item
        /// </summary>
        [DataMember(Name = "udi")]
        public Udi Udi { get; }

        /// <summary>
        /// The settings for the layout item
        /// </summary>
        [DataMember(Name = "settings")]
        public IPublishedElement Settings { get; }

    }
}
