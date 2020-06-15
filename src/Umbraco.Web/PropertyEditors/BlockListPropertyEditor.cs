using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.Blocks;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;

namespace Umbraco.Web.PropertyEditors
{
    /// <summary>
    /// Represents a block list property editor.
    /// </summary>
    [DataEditor(
        Constants.PropertyEditors.Aliases.BlockList,
        "Block List",
        "blocklist",
        ValueType = ValueTypes.Json,
        Group = Constants.PropertyEditors.Groups.Lists,
        Icon = "icon-thumbnail-list")]
    public class BlockListPropertyEditor : BlockEditorPropertyEditor
    {
        public BlockListPropertyEditor(ILogger logger, Lazy<PropertyEditorCollection> propertyEditors, IDataTypeService dataTypeService, IContentTypeService contentTypeService, ILocalizedTextService localizedTextService)
            : base(logger, propertyEditors, dataTypeService, contentTypeService, new DataHelper(), localizedTextService)
        { }

        #region Pre Value Editor

        protected override IConfigurationEditor CreateConfigurationEditor() => new BlockListConfigurationEditor();

        #endregion

        #region IBlockEditorDataHelper

        // TODO: Rename this we don't want to use the name "Helper"
        private class DataHelper : IBlockEditorDataHelper
        {
            public IEnumerable<IBlockReference> GetBlockReferences(JObject layout)
            {
                if (!(layout?[Constants.PropertyEditors.Aliases.BlockList] is JArray blLayouts))
                    yield break;

                foreach (var blLayout in blLayouts)
                {
                    if (!(blLayout is JObject blockRef) || !(blockRef[UdiPropertyKey] is JValue udiRef) || udiRef.Type != JTokenType.String || !Udi.TryParse(udiRef.ToString(), out var udi)) continue;
                    yield return new SimpleRef(udi);
                }
            }

            public bool IsEditorSpecificPropertyKey(string propertyKey) => false;

            private class SimpleRef : IBlockReference
            {
                public SimpleRef(Udi udi)
                {
                    Udi = udi;
                }

                public Udi Udi { get; }
            }
        }

        #endregion
    }
}
