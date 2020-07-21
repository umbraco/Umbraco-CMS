using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;
using System;
using System.Collections.Generic;
using static Umbraco.Core.Models.Blocks.BlockEditorData;

namespace Umbraco.Core.Models.Blocks
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

        public BlockEditorData Convert(string json)
        {
            var value = JsonConvert.DeserializeObject<BlockValue>(json);

            if (value.Layout == null)
                return BlockEditorData.Empty;

            if (!value.Layout.TryGetValue(_propertyEditorAlias, out var layout))
                return BlockEditorData.Empty;

            var references = GetBlockReferences(layout);
            var contentData = value.ContentData.ToList();
            var settingsData = value.SettingsData.ToList();

            return new BlockEditorData(layout, references, contentData, settingsData);
        }

        /// <summary>
        /// Return the collection of <see cref="IBlockReference"/> from the block editor's Layout (which could be an array or an object depending on the editor)
        /// </summary>
        /// <param name="jsonLayout"></param>
        /// <returns></returns>
        protected abstract IReadOnlyList<ContentAndSettingsReference> GetBlockReferences(JToken jsonLayout);

    }
}
