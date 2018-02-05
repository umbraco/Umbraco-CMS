using System.Collections.Generic;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    [ValueEditor(Constants.PropertyEditors.Aliases.RelatedLinks2, "Related links", "relatedlinks", ValueType = ValueTypes.Json, Icon = "icon-thumbnail-list", Group = "pickers")]
    public class RelatedLinks2PropertyEditor : PropertyEditor
    {
        public RelatedLinks2PropertyEditor(ILogger logger)
            : base(logger)
        { }

        protected override ConfigurationEditor CreateConfigurationEditor() => new RelatedLinksConfigurationEditor();
    }
}
