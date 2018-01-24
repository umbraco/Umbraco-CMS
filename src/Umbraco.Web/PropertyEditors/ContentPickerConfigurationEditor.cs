using System.Collections.Generic;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    internal class ContentPickerConfigurationEditor : ConfigurationEditor
    {
        public ContentPickerConfigurationEditor()
        {
            Fields.Add(new ConfigurationField
            {
                Key = "showOpenButton",
                View = "boolean",
                Name = "Show open button (this feature is in preview!)",
                Description = "Opens the node in a dialog"
            });

            Fields.Add(new ConfigurationField
            {
                Key = "startNodeId",
                View = "treepicker",
                Name = "Start node",
                Config = new Dictionary<string, object>
                {
                    { "idType", "udi" }
                }
            });
        }
    }
}