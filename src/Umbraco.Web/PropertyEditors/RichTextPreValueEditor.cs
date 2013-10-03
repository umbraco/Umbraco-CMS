using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    //need to figure out how to use this...
    internal class RichTextPreValueEditor : PreValueEditor
    {
        public RichTextPreValueEditor()
        {
            //use a custom editor too
            Fields.Add(new PreValueField() { 
                    View = "views/propertyeditors/rte/rte.prevalues.html", 
                    HideLabel= true, Key="editor"});
        }

        
    }
}
