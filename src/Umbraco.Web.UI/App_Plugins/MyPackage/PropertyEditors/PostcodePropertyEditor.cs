using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Umbraco.Belle.System.PropertyEditors;

namespace Umbraco.Belle.App_Plugins.MyPackage.PropertyEditors
{
    [PropertyEditor("E96E24E5-7124-4FA8-A7D7-C3D3695E100D", "Postal Code",
        "~/App_Plugins/MyPackage/PropertyEditors/Views/PostcodeEditor.html")]
    public class PostcodePropertyEditor : PropertyEditor
    {
        /// <summary>
        /// Creates the value editor with custom validators
        /// </summary>
        /// <returns></returns>
        protected override ValueEditor CreateValueEditor()
        {
            var editor = base.CreateValueEditor();

            editor.Validators = new List<ValidatorBase> { new PostcodeValidator() };

            return editor;
        }
    }
}