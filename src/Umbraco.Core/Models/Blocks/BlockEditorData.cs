using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace Umbraco.Core.Models.Blocks
{

    /// <summary>
    /// Convertable block data from json
    /// </summary>
    public class BlockEditorData
    {
        private readonly string _propertyEditorAlias;

        public static BlockEditorData Empty { get; } = new BlockEditorData();

        private BlockEditorData()
        {
            BlockValue = new BlockValue();
        }

        public BlockEditorData(string propertyEditorAlias,
            IEnumerable<ContentAndSettingsReference> references,
            BlockValue blockValue)
        {
            if (string.IsNullOrWhiteSpace(propertyEditorAlias))
                throw new ArgumentException($"'{nameof(propertyEditorAlias)}' cannot be null or whitespace", nameof(propertyEditorAlias));
            _propertyEditorAlias = propertyEditorAlias;
            BlockValue = blockValue ?? throw new ArgumentNullException(nameof(blockValue));
            References = references != null ? new List<ContentAndSettingsReference>(references) : throw new ArgumentNullException(nameof(references));
        }

        /// <summary>
        /// Returns the layout for this specific property editor
        /// </summary>
        public JToken Layout => BlockValue.Layout.TryGetValue(_propertyEditorAlias, out var layout) ? layout : null;

        /// <summary>
        /// Returns the reference to the original BlockValue
        /// </summary>
        public BlockValue BlockValue { get; }

        public List<ContentAndSettingsReference> References { get; } = new List<ContentAndSettingsReference>();
    }
}
