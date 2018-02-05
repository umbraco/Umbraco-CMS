using System.Collections.Generic;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    /// <summary>
    /// Represents the configuration for the multinode picker value editor.
    /// </summary>
    public class MultiNodePickerConfiguration
    {
        // all fields added in the editor ctor

        public string TreeSource { get; set; } // fixme Udi?

        public string Filter { get; set; }

        public int MinNumber { get; set; }

        public int MaxNumber { get; set; }

        public bool ShowOpen { get; set; }
    }

    /// <summary>
    /// Represents the configuration for the multinode picker value editor.
    /// </summary>
    public class MultiNodePickerConfigurationEditor : ConfigurationEditor<MultiNodePickerConfiguration>
    {
        public MultiNodePickerConfigurationEditor()
        {
            Fields.Add(new ConfigurationField()
            {
                Key = "startNode",
                View = "treesource",
                Name = "Node type",
                PropertyName = nameof(MultiNodePickerConfiguration.TreeSource),
                Config = new Dictionary<string, object>
                {
                    { "idType", "udi" }
                }
            });

            Fields.Add(new ConfigurationField()
            {
                Key = "filter",
                View = "textstring",
                Name = "Allow items of type",
                PropertyName = nameof(MultiNodePickerConfiguration.Filter),
                Description = "Separate with comma"
            });

            Fields.Add(new ConfigurationField()
            {
                Key = "minNumber",
                View = "number",
                Name = "Minimum number of items",
                PropertyName = nameof(MultiNodePickerConfiguration.MinNumber)
            });

            Fields.Add(new ConfigurationField()
            {
                Key = "maxNumber",
                View = "number",
                Name = "Maximum number of items",
                PropertyName = nameof(MultiNodePickerConfiguration.MaxNumber)
            });

            Fields.Add(new ConfigurationField()
            {
                Key = "showOpenButton",
                View = "boolean",
                Name = "Show open button (this feature is in preview!)",
                PropertyName = nameof(MultiNodePickerConfiguration.ShowOpen),
                Description = "Opens the node in a dialog"
            });
        }

        // fixme
        public override IDictionary<string, object> DefaultConfiguration => new Dictionary<string, object>
        {
            { "multiPicker", 1 },
            { "showOpenButton", 0 },
            { "showEditButton", 0 },
            { "showPathOnHover", 0 },
            { "idType", "udi" }
        };

        /// <inheritdoc />
        public override Dictionary<string, object> ToConfigurationEditor(MultiNodePickerConfiguration configuration)
        {
            // sanitize configuraiton
            var output = base.ToConfigurationEditor(configuration);

            output["multiPicker"] = configuration.MaxNumber > 1 ? "1" : "0";

            return output;
        }
    }
}