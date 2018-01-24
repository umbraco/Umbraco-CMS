using System.Collections.Generic;
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
        {
            InternalPreValues = new Dictionary<string, object>
            {
                { "multiPicker", 1 },
                { "showOpenButton", 0 },
                { "showEditButton", 0 },
                { "showPathOnHover", 0 },
                { "idType", "udi" }
            };
        }

        protected override ConfigurationEditor CreateConfigurationEditor()
        {
            return new MultiNodePickerConfigurationEditor();
        }

        internal IDictionary<string, object> InternalPreValues;

        public override IDictionary<string, object> DefaultPreValues
        {
            get => InternalPreValues;
            set => InternalPreValues = value;
        }
    }
}
