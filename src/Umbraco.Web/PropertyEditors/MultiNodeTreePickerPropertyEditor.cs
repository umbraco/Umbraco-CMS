using System.Collections.Generic;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.Editors;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    [DataEditor(
        Constants.PropertyEditors.Aliases.MultiNodeTreePicker,
        "Multinode Treepicker",
        "contentpicker",
        ValueType = ValueTypes.Text,
        Group = Constants.PropertyEditors.Groups.Pickers,
        Icon = "icon-page-add")]
    public class MultiNodeTreePickerPropertyEditor : DataEditor
    {
        public MultiNodeTreePickerPropertyEditor(ILogger logger)
            : base(logger)
        { }

        protected override IConfigurationEditor CreateConfigurationEditor() => new MultiNodePickerConfigurationEditor();

        protected override IDataValueEditor CreateValueEditor() => new MultiNodeTreePickerPropertyValueEditor(Attribute);

        public class MultiNodeTreePickerPropertyValueEditor : DataValueEditor
        {
            public MultiNodeTreePickerPropertyValueEditor(DataEditorAttribute attribute): base(attribute)
            {

            }
        }
    }


}
