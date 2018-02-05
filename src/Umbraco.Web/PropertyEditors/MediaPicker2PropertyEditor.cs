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
        { }

        protected override ConfigurationEditor CreateConfigurationEditor() => new MediaPickerConfigurationEditor();
    }
}
