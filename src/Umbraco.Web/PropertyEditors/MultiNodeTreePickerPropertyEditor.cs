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

        public class MultiNodeTreePickerPropertyValueEditor : DataValueEditor, IDataValueReference
        {
            public MultiNodeTreePickerPropertyValueEditor(DataEditorAttribute attribute): base(attribute)
            {

            }

            public IEnumerable<UmbracoEntityReference> GetReferences(object value)
            {
                var asString = value == null ? string.Empty : value is string str ? str : value.ToString();

                var udiPaths = asString.Split(Constants.CharArrays.Comma);
                foreach (var udiPath in udiPaths)
                    if (Udi.TryParse(udiPath, out var udi))
                        yield return new UmbracoEntityReference(udi);
            }
        }
    }


}
