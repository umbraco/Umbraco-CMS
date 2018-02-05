using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{

    // FIXME obviously the idea is to turn it into a ConfigurationEditor<ContentPickerConfiguration>
    // but
    // before that we have to find a way to have "default configuration" works for base ConfigurationEditors

    public class ContentPickerConfiguration
    {
        [ConfigurationField("showOpenButton", "Show open button (this feature is in beta!)", "boolean", Description = "Opens the node in a dialog")]
        public bool ShowOpenButton { get; set; }

        [ConfigurationField("startNodeId", "Start node", "treepicker")]
        public int StartNodeId { get; set; } = -1; // default value is -1
    }

    internal class ContentPickerConfigurationEditor : ConfigurationEditor //<ContentPickerConfiguration>
    {
        public ContentPickerConfigurationEditor()
        {
            Fields.Add(new ConfigurationField
            {                    
                Key = "showOpenButton",
                View = "boolean",
                Name = "Show open button (this feature is in preview!)",
                Description = "Opens the node in a dialog",
                PropertyName = nameof(ContentPickerConfiguration.ShowOpenButton)
            });
            Fields.Add(new ConfigurationField
            {
                Key = "startNodeId",
                View = "treepicker",
                Name = "Start node",
                PropertyName = nameof(ContentPickerConfiguration.StartNodeId)
            });

            // configure fields
            Field(nameof(ContentPickerConfiguration.StartNodeId))
                .Config = new Dictionary<string, object> { { "idType", "udi" } };
        }

        // fixme - cache this to allocate only once!
        // fixme - should derive from the ContentPickerConfiguration class!
        // fixme - we have way more stuff in here than pure fields defined above = what does it mean?
        public override IDictionary<string, object> DefaultConfiguration => new Dictionary<string, object>
        {
            { "startNodeId", -1 },
            { "showOpenButton", 0 },
            { "showEditButton", 0 },
            { "showPathOnHover", 0 },
            { "idType", "udi" }
        };
    }
}