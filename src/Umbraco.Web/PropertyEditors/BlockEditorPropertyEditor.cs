using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    [DataEditor(Constants.PropertyEditors.Aliases.BlockEditor, "Block Editor", ValueType = ValueTypes.Json, Group="rich content")]
    public class BlockEditorPropertyEditor : DataEditor
    {
        public BlockEditorPropertyEditor(ILogger logger)
            : base(logger)
        { }

        protected override IConfigurationEditor CreateConfigurationEditor() => new BlockEditorConfigurationEditor();

        protected override IDataValueEditor CreateValueEditor() => base.CreateValueEditor();
    }
}
