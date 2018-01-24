using System.Collections.Generic;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    /// <summary>
    /// Media picker property editors that stores UDI
    /// </summary>
    [ValueEditor(Constants.PropertyEditors.Aliases.MediaPicker2, "Media Picker", "mediapicker", ValueTypes.Text, IsMacroParameterEditor = true, Group = "media", Icon = "icon-picture")]
    public class MediaPicker2PropertyEditor : PropertyEditor
    {
        public MediaPicker2PropertyEditor(ILogger logger)
            : base(logger)
        {
            InternalPreValues = new Dictionary<string, object>
            {
                {"idType", "udi"}
            };
        }

        internal IDictionary<string, object> InternalPreValues;

        public override IDictionary<string, object> DefaultPreValues
        {
            get => InternalPreValues;
            set => InternalPreValues = value;
        }

        protected override ConfigurationEditor CreateConfigurationEditor()
        {
            return new MediaPickerConfigurationEditor();
        }
    }
}
