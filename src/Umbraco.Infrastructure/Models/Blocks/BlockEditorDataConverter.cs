using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Umbraco.Cms.Core.Models.Blocks
{
    /// <summary>
    /// Converts the block json data into objects
    /// </summary>
    public abstract class BlockEditorDataConverter
    {
        private readonly string _propertyEditorAlias;

        protected BlockEditorDataConverter(string propertyEditorAlias)
        {
            _propertyEditorAlias = propertyEditorAlias;
        }

        public BlockEditorData ConvertFrom(JToken json)
        {
            var value = json.ToObject<BlockValue>();
            return Convert(value);
        }

        public bool TryDeserialize(string json, [MaybeNullWhen(false)] out BlockEditorData blockEditorData)
        {
            try
            {
                var value = JsonConvert.DeserializeObject<BlockValue>(json);
                blockEditorData = Convert(value);
                return true;
            }
            catch (System.Exception)
            {
                blockEditorData = null;
                return false;
            }
        }

        public BlockEditorData Deserialize(string json)
        {
            var value = JsonConvert.DeserializeObject<BlockValue>(json);
            return Convert(value);
        }

        private BlockEditorData Convert(BlockValue? value)
        {
            if (value?.Layout == null)
                return BlockEditorData.Empty;

            var references = value.Layout.TryGetValue(_propertyEditorAlias, out var layout)
                ? GetBlockReferences(layout)
                : Enumerable.Empty<ContentAndSettingsReference>();

            return new BlockEditorData(_propertyEditorAlias, references!, value);
        }

        /// <summary>
        /// Return the collection of <see cref="IBlockReference"/> from the block editor's Layout (which could be an array or an object depending on the editor)
        /// </summary>
        /// <param name="jsonLayout"></param>
        /// <returns></returns>
        protected abstract IEnumerable<ContentAndSettingsReference>? GetBlockReferences(JToken jsonLayout);

    }
}
