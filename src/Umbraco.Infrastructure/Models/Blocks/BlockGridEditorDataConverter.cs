using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Core.Models.Blocks
{
    /// <summary>
    /// Data converter for the block grid property editor
    /// </summary>
    public class BlockGridEditorDataConverter : BlockEditorDataConverter
    {
        private readonly IJsonSerializer _jsonSerializer;

        public BlockGridEditorDataConverter(IJsonSerializer jsonSerializer) : base(Cms.Core.Constants.PropertyEditors.Aliases.BlockGrid)
        {
            _jsonSerializer = jsonSerializer;
        }

        protected override IEnumerable<ContentAndSettingsReference> GetBlockReferences(JToken jsonLayout)
        {

            var blockListLayouts = _jsonSerializer.Deserialize<BlockGridLayoutItem[]>(jsonLayout.ToString())!;

            var result = new List<ContentAndSettingsReference>();

            foreach (BlockGridLayoutItem blockGridLayoutItem in blockListLayouts)
            {
                AddToResult(blockGridLayoutItem, result);
            }

            return result;
        }

        private void AddToResult(BlockGridLayoutItem x, List<ContentAndSettingsReference> result)
        {
            if (x is null)
            {
                return;
            }

           result.Add(new ContentAndSettingsReference(x.ContentUdi, x.SettingsUdi));

           foreach (var child in x.Areas.SelectMany(x=>x.Items))
           {
               AddToResult(child, result);
           }
        }
    }
}
