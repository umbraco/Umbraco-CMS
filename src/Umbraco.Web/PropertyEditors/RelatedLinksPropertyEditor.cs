using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    [PropertyEditor(Constants.PropertyEditors.RelatedLinksAlias, "Related links", "relatedlinks", ValueType = PropertyEditorValueTypes.Json, Icon="icon-thumbnail-list", Group="pickers")]
    public class RelatedLinksPropertyEditor : PropertyEditor
    {
        /// <summary>
        /// The constructor will setup the property editor based on the attribute if one is found
        /// </summary>
        public RelatedLinksPropertyEditor(ILogger logger) : base(logger)
        {
        }
        protected override PreValueEditor CreatePreValueEditor()
        {
            return new RelatedLinksPreValueEditor();
        }

        internal class RelatedLinksPreValueEditor : PreValueEditor
        {
            [PreValueField("max", "Maximum number of links", "number", Description = "Enter the maximum amount of links to be added, enter 0 for unlimited")]
            public int Maximum { get; set; }
        }
    }
}
