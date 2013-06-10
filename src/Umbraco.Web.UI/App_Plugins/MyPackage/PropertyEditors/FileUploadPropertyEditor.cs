using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.UI.App_Plugins.MyPackage.PropertyEditors
{
    [PropertyEditor("5032a6e6-69e3-491d-bb28-cd31cd11086c", "File upload",
        "~/App_Plugins/MyPackage/PropertyEditors/Views/FileUploadEditor.html")]
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