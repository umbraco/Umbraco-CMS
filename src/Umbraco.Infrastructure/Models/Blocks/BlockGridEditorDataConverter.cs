using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Umbraco.Cms.Core.Models.Blocks
{
    /// <summary>
    /// Data converter for the block grid property editor
    /// </summary>
    public class BlockGridEditorDataConverter : BlockEditorDataConverter
    {
        public BlockGridEditorDataConverter() : base(Cms.Core.Constants.PropertyEditors.Aliases.BlockGrid)
        {
        }

        protected override IEnumerable<ContentAndSettingsReference> GetBlockReferences(JToken jsonLayout)
        {
            var blockListLayout = jsonLayout.ToObject<IEnumerable<BlockGridLayoutItem>>();
            return blockListLayout.Select(x => new ContentAndSettingsReference(x.ContentUdi, x.SettingsUdi)).ToList();
        }
    }
}
