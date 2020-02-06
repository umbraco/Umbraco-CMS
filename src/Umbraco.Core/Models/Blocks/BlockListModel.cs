using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Umbraco.Core.Models.PublishedContent;

namespace Umbraco.Core.Models.Blocks
{
    /// <summary>
    /// The strongly typed model for the Block List editor
    /// </summary>
    [DataContract(Name = "blockList", Namespace = "")]
    public class BlockListModel : BlockEditorModel
    {
        public BlockListModel(IEnumerable<IPublishedElement> data, IEnumerable<BlockListLayoutReference> layout)
            : base(data)
        {
            Layout = layout;
        }

        /// <summary>
        /// The layout items of the Block List editor
        /// </summary>
        [DataMember(Name = "layout")]
        public IEnumerable<BlockListLayoutReference> Layout { get; }

        /// <summary>
        /// Returns the data item associated with the layout udi reference
        /// </summary>
        /// <param name="udi"></param>
        /// <returns></returns>
        public IPublishedElement GetData(Udi udi)
        {
            if (!(udi is GuidUdi guidUdi))
                return null;
            return Data.FirstOrDefault(x => x.Key == guidUdi.Guid);
        }

    }
}
