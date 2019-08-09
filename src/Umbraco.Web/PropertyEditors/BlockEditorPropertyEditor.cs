using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    [DataEditor(Constants.PropertyEditors.Aliases.BlockEditor, EditorType.PropertyValue , "Block Editor", "blockeditor", ValueType = ValueTypes.Json, Group="rich content", Icon="icon-application-window-alt")]
    public class BlockEditorPropertyEditor : DataEditor
    {
        public BlockEditorPropertyEditor(ILogger logger)
            : base(logger)
        { }

        protected override IConfigurationEditor CreateConfigurationEditor() => new BlockEditorConfigurationEditor();

        protected override IDataValueEditor CreateValueEditor() => base.CreateValueEditor();
    }
}
