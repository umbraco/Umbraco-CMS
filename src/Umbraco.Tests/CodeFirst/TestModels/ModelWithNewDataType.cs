using Umbraco.Tests.CodeFirst.Attributes;
using umbraco.editorControls.textfield;
using umbraco.editorControls.tinymce;

namespace Umbraco.Tests.CodeFirst.TestModels
{
    public class ModelWithNewDataType : ContentTypeBase
    {
        [PropertyType(typeof(TextFieldDataType), PropertyGroup = "Content")]
        public string Title { get; set; }

        [PropertyType(typeof(TinyMCEDataType), PropertyGroup = "Content")]
        public string BodyContent { get; set; } 
    }
}