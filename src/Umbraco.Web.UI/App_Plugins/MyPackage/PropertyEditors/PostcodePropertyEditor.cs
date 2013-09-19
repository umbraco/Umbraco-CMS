using System.Collections.Generic;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.UI.App_Plugins.MyPackage.PropertyEditors
{
    [PropertyEditor("MyPackage.PostalCode", "Postal Code",
        "~/App_Plugins/MyPackage/PropertyEditors/Views/PostcodeEditor.html")]
    public class PostcodePropertyEditor : PropertyEditor
    {
        /// <summary>
        /// Creates the value editor with custom validators
        /// </summary>
        /// <returns></returns>
        protected override PropertyValueEditor CreateValueEditor()
        {
            var editor = base.CreateValueEditor();

            editor.Validators.Add(  new PostcodeValidator() );

            return editor;
        }
    }
}