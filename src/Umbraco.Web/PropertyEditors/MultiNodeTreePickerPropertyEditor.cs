using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    [DataEditor(Constants.PropertyEditors.Aliases.MultiNodeTreePicker, "Multinode Treepicker", "contentpicker", ValueType = ValueTypes.Text, Group = "pickers", Icon = "icon-page-add")]
    public class MultiNodeTreePickerPropertyEditor : DataEditor
    {
        public MultiNodeTreePickerPropertyEditor(ILogger logger)
            : base(logger)
        { }

        protected override IConfigurationEditor CreateConfigurationEditor() => new MultiNodePickerConfigurationEditor();
    }
}
