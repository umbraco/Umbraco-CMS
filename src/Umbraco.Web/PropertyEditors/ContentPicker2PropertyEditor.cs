using System.Collections.Generic;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    /// <summary>
    /// Content property editor that stores UDI
    /// </summary>
    [ValueEditor(Constants.PropertyEditors.Aliases.ContentPicker2Alias, "Content Picker", "contentpicker", ValueTypes.String, IsMacroParameterEditor = true, Group = "Pickers")]
    public class ContentPicker2PropertyEditor : PropertyEditor
    {
        public ContentPicker2PropertyEditor(ILogger logger)
            : base(logger)
        {
            InternalPreValues = new Dictionary<string, object>
            {
                { "startNodeId", -1 },
                { "showOpenButton", 0 },
                { "showEditButton", 0 },
                { "showPathOnHover", 0 },
                { "idType", "udi" }
            };
        }

        internal IDictionary<string, object> InternalPreValues;

        public override IDictionary<string, object> DefaultConfiguration
        {
            get => InternalPreValues;
            set => InternalPreValues = value;
        }

        protected override ConfigurationEditor CreateConfigurationEditor()
        {
            return new ContentPickerConfigurationEditor();
        }
    }
}
