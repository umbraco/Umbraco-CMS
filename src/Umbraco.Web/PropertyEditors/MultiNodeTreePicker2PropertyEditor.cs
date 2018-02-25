using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    [DataEditor(Constants.PropertyEditors.Aliases.MultiNodeTreePicker2, "Multinode Treepicker", "contentpicker", ValueType = ValueTypes.Text, Group = "pickers", Icon = "icon-page-add")]
    public class MultiNodeTreePicker2PropertyEditor : DataEditor
    {
        public MultiNodeTreePicker2PropertyEditor(ILogger logger)
            : base(logger)
        { }

        protected override IConfigurationEditor CreateConfigurationEditor() => new MultiNodePickerConfigurationEditor();
    }
}
