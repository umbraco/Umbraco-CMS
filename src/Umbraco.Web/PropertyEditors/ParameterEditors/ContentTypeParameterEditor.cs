using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors.ParameterEditors
{
    [ParameterEditor("contentType", "Content Type Picker", "contenttypepicker")]
    public class ContentTypeParameterEditor : ParameterEditor
    {
        
    }

    [ParameterEditor("contentTypeMultiple", "Multiple Content Type Picker", "contenttypepicker")]
    public class MultipleContentTypeParameterEditor : ParameterEditor
    {
        public MultipleContentTypeParameterEditor()
        {
            Configuration.Add("multiple", "1");
        }
    }
}
