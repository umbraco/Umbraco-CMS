using System.Collections.Generic;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    /// <summary>
    /// Media picker property editors that stores UDI
    /// </summary>
    [PropertyEditor(Constants.PropertyEditors.Aliases.MediaPicker2, "Media Picker", PropertyEditorValueTypes.Text, "mediapicker", IsParameterEditor = true, Group = "media", Icon = "icon-picture")]
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

        protected override PreValueEditor CreateConfigurationEditor()
        {
            return new MediaPickerPreValueEditor();
        }

        internal class MediaPickerPreValueEditor : PreValueEditor
        {
            public MediaPickerPreValueEditor()
            {
                //create the fields
                Fields.Add(new DataTypeConfigurationField
                {
                    Key = "multiPicker",
                    View = "boolean",
                    Name = "Pick multiple items"
                });
                Fields.Add(new DataTypeConfigurationField
                {
                    Key = "onlyImages",
                    View = "boolean",
                    Name = "Pick only images",
                    Description = "Only let the editor choose images from media."
                });
                Fields.Add(new DataTypeConfigurationField
                {
                    Key = "disableFolderSelect",
                    View = "boolean",
                    Name = "Disable folder select",
                    Description = "Do not allow folders to be picked."
                });
                Fields.Add(new DataTypeConfigurationField
                {
                    Key = "startNodeId",
                    View = "mediapicker",
                    Name = "Start node",
                    Config = new Dictionary<string, object>
                    {
                        {"idType", "udi"}
                    }
                });
            }
        }
    }
}
