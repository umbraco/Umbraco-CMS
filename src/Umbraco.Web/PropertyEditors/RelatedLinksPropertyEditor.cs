using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    [DataEditor(Constants.PropertyEditors.Aliases.RelatedLinks, "Related links", "relatedlinks", ValueType = ValueTypes.Json, Icon = "icon-thumbnail-list", Group = "pickers")]
    public class RelatedLinksPropertyEditor : DataEditor
    {
        public RelatedLinksPropertyEditor(ILogger logger)
            : base(logger)
        { }

        protected override IConfigurationEditor CreateConfigurationEditor() => new RelatedLinksConfigurationEditor();
    }
}
