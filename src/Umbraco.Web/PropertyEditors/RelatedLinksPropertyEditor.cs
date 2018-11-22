using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    [Obsolete("This editor is obsolete, use RelatedLinks2PropertyEditor instead which stores UDI")]
    [PropertyEditor(Constants.PropertyEditors.RelatedLinksAlias, "(Obsolete) Related links", "relatedlinks", ValueType = PropertyEditorValueTypes.Json, Icon="icon-thumbnail-list", Group="pickers", IsDeprecated = true)]
    public class RelatedLinksPropertyEditor : RelatedLinks2PropertyEditor
    {
        public RelatedLinksPropertyEditor()
        {
            InternalPreValues = new Dictionary<string, object>
            {
                {"idType", "int"}
            };
        }
    }
}
