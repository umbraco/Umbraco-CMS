using Umbraco.Core;
using Umbraco.Core.Logging;
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
    public class BlockListPropertyEditor : DataEditor
    {
        public BlockListPropertyEditor(ILogger logger)
            : base(logger)
        { }

        #region Pre Value Editor
        //protected override IConfigurationEditor CreateConfigurationEditor() => new BlockEditorListConfigurationEditor();

        #endregion

    }
}
