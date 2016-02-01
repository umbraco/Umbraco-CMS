using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    [PropertyEditor(Constants.PropertyEditors.RelatedLinksAlias, "Related links", "relatedlinks", ValueType ="JSON", Icon="icon-thumbnail-list", Group="pickers")]
    public class RelatedLinksPropertyEditor : PropertyEditor
    {
        protected override PreValueEditor CreatePreValueEditor()
        {
            return new RelatedLinksPreValueEditor();
        }

        internal class RelatedLinksPreValueEditor : PreValueEditor
        {
            [PreValueField("maxNumber", "Maximum number of items", "number")]
            public int MaxNumber { get; set; }
        }
    }
}
