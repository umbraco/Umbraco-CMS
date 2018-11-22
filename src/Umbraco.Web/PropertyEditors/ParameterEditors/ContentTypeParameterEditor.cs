using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors.ParameterEditors
{
    [ParameterEditor("contentType", "Content Type Picker", "entitypicker")]
    public class ContentTypeParameterEditor : ParameterEditor
    {
        public ContentTypeParameterEditor()
        {
            Configuration.Add("multiple", "0");
            Configuration.Add("entityType", "DocumentType");
        }
    }
}
