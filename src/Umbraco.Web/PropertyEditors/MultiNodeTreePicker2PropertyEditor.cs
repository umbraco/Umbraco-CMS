using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    [ValueEditor(Constants.PropertyEditors.Aliases.MultiNodeTreePicker2, "Multinode Treepicker", "contentpicker", ValueTypes.Text, Group = "pickers", Icon = "icon-page-add")]
    public class MultiNodeTreePicker2PropertyEditor : PropertyEditor
    {
        public MultiNodeTreePicker2PropertyEditor(ILogger logger)
            : base(logger)
        { }

        protected override ConfigurationEditor CreateConfigurationEditor() => new MultiNodePickerConfigurationEditor();
    }
}
