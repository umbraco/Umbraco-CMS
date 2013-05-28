using System.Collections.Generic;
using Umbraco.Belle.System.PropertyEditors;

namespace Umbraco.Belle.App_Plugins.MyPackage.PropertyEditors
{
    [PropertyEditor("23A66468-30E2-4537-8039-625F8BC5CA1E", "File upload",
        "~/App_Plugins/MyPackage/PropertyEditors/Views/FileUploadEditor.html")]
    public class FileUploadPropertyEditor : PropertyEditor
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