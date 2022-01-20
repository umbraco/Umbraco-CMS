﻿using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Umbraco.Cms.Core.Models.Blocks
{
    /// <summary>
    /// Data converter for the block list property editor
    /// </summary>
    public class BlockListEditorDataConverter : BlockEditorDataConverter
    {
        public BlockListEditorDataConverter() : base(Cms.Core.Constants.PropertyEditors.Aliases.BlockList)
        {
        }

        protected override IEnumerable<ContentAndSettingsReference> GetBlockReferences(JToken jsonLayout)
        {
            var blockListLayout = jsonLayout.ToObject<IEnumerable<BlockListLayoutItem>>();
            return blockListLayout.Select(x => new ContentAndSettingsReference(x.ContentUdi, x.SettingsUdi)).ToList();
        }
    }
}
