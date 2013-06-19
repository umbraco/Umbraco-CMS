using Umbraco.Core;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    [PropertyEditor(Constants.PropertyEditors.UploadField, "File upload", "fileupload")]
    public class FileUploadPropertyEditor : PropertyEditor
    {
        /// <summary>
        /// Creates our custom value editor
        /// </summary>
        /// <returns></returns>
        protected override ValueEditor CreateValueEditor()
        {
            //TODO: Ensure we assign custom validation for uploaded file types!
            
            var baseEditor = base.CreateValueEditor();

            return new FileUploadValueEditor
                {
                    View = baseEditor.View
                };
        }
    }
}