using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Umbraco.Core.Models.PublishedContent;

namespace Umbraco.Core.Models.Blocks
{
    /// <summary>
    /// The base class for any strongly typed model for a Block editor implementation
    /// </summary>
    public abstract class BlockEditorModel
    {
        protected BlockEditorModel(IEnumerable<IPublishedElement> data)
        {
            Data = data ?? throw new ArgumentNullException(nameof(data));
        }

        public BlockEditorModel()
        {
        }


        /// <summary>
        /// The data items of the Block List editor
        /// </summary>
        [DataMember(Name = "data")]
        public IEnumerable<IPublishedElement> Data { get; set; }
    }
}
