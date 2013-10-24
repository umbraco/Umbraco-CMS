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
            //SD: You can add pre-val fields here like you are doing, or you can add fields using attributes (http://issues.umbraco.org/issue/U4-2692),
            // see below for examples.

            //use a custom editor too
            Fields.Add(new PreValueField()
            {
                View = "views/propertyeditors/rte/rte.prevalues.html",
                HideLabel = true,
                Key = "editor"
            });

            Fields.Add(new PreValueField()
            {
                Name = "Hide Label",
                View = "boolean",
                Key = "hideLabel"
            });
        }

        //SD: You can declare a field like this if you want to instead of in the ctor, there's some options here:
        //#1 - the property name becomes the Key:
        //
        // [PreValueField("", "views/propertyeditors/rte/rte.prevalues.html", HideLabel = true)]
        // public string Editor { get; set; }

        //#2 - You can specify a custom Key:
        //
        // [PreValueField("editor", "", "views/propertyeditors/rte/rte.prevalues.html", HideLabel = true)]
        // public string Editor { get; set; }

        //#3 - If you require custom server side validation for your field then you have to specify a custom PreValueField type to use,
        //     this is why in this case I find it easier to use the ctor logic but thats just an opinion
        //     - Any value specified for this property attribute will override the values set in on the class instance of the field, this
        //       allows you to re-use class instances of fields if you want.
        //
        // [PreValueField(typeof(EditorPreValueField))]
        // public string Editor { get; set; }
           
        // [PreValueField("", "views/propertyeditors/rte/rte.prevalues.html", HideLabel = true)]
        // public class EditorPreValueField : PreValueField
        // {
        //     public EditorPreValueField()
        //     {
        //         //add any required server validators for this field
        //         Validators.Add(new RegexValidator("^\\d*$"));
        //         //You could also set the field properties directly here if you wanted instead of the attribute
        //     }
        // }

    }
}
