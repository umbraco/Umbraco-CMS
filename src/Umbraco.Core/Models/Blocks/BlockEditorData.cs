using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using Umbraco.Core.Serialization;

namespace Umbraco.Core.Models.Blocks
{

    /// <summary>
    /// Converted block data from json
    /// </summary>
    public class BlockEditorData
    {
        public static BlockEditorData Empty { get; } = new BlockEditorData();

        private BlockEditorData()
        {
        }

        public BlockEditorData(JToken layout, IReadOnlyList<Udi> layoutBlockReferences, IReadOnlyList<BlockItemData> blocks)
        {
            Layout = layout ?? throw new ArgumentNullException(nameof(layout));
            LayoutBlockReferences = layoutBlockReferences ?? throw new ArgumentNullException(nameof(layoutBlockReferences));
            Blocks = blocks ?? throw new ArgumentNullException(nameof(blocks));
        }

        public JToken Layout { get; }
        public IReadOnlyList<Udi> LayoutBlockReferences { get; } = new List<Udi>();
        public IReadOnlyList<BlockItemData> Blocks { get; } = new List<BlockItemData>();

        internal class BlockValue
        {
            [JsonProperty("layout")]
            public IDictionary<string, JToken> Layout { get; set; }

            [JsonProperty("data")]
            public IEnumerable<BlockItemData> Data { get; set; }
        }

        /// <summary>
        /// Represents a single block's data in raw form
        /// </summary>
        public class BlockItemData
        {
            [JsonProperty("contentTypeKey")]
            public Guid ContentTypeKey { get; set; }

            [JsonProperty("udi")]
            [JsonConverter(typeof(UdiJsonConverter))]
            public Udi Udi { get; set; }

            /// <summary>
            /// The remaining properties will be serialized to a dictionary
            /// </summary>
            /// <remarks>
            /// The JsonExtensionDataAttribute is used to put the non-typed properties into a bucket
            /// http://www.newtonsoft.com/json/help/html/DeserializeExtensionData.htm
            /// NestedContent serializes to string, int, whatever eg
            ///   "stringValue":"Some String","numericValue":125,"otherNumeric":null
            /// </remarks>
            [JsonExtensionData]
            public Dictionary<string, object> RawPropertyValues { get; set; } = new Dictionary<string, object>();
        }
    }
}
