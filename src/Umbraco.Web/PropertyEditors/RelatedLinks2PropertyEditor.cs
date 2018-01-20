using System.Collections.Generic;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    [PropertyEditor(Constants.PropertyEditors.Aliases.RelatedLinks2, "Related links", "relatedlinks", ValueType = PropertyEditorValueTypes.Json, Icon = "icon-thumbnail-list", Group = "pickers")]
    public class RelatedLinks2PropertyEditor : PropertyEditor
    {
        public RelatedLinks2PropertyEditor(ILogger logger)
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
            return new RelatedLinksPreValueEditor();
        }

        internal class RelatedLinksPreValueEditor : PreValueEditor
        {
            [DataTypeConfigurationField("max", "Maximum number of links", "number", Description = "Enter the maximum amount of links to be added, enter 0 for unlimited")]
            public int Maximum { get; set; }
        }
    }
}
