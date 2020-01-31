using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;

namespace Umbraco.Web.PropertyEditors
{

    [DataEditor(
        Constants.PropertyEditors.Aliases.BlockList,
        "Block List",
        "blocklist",
        Icon = "icon-list",
        Group = Constants.PropertyEditors.Groups.Lists)]
    public class BlockListPropertyEditor : BlockEditorPropertyEditor
    {
        public BlockListPropertyEditor(ILogger logger) : base(logger)
        {
        }
    }
}
