using System.Collections.Generic;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    [DataEditor(Constants.PropertyEditors.Aliases.RelatedLinks2, "Related links", "relatedlinks", ValueType = ValueTypes.Json, Icon = "icon-thumbnail-list", Group = "pickers")]
    public class RelatedLinks2PropertyEditor : DataEditor
    {
        public RelatedLinks2PropertyEditor(ILogger logger)
            : base(logger)
        { }

        protected override IConfigurationEditor CreateConfigurationEditor() => new RelatedLinksConfigurationEditor();
    }
}
