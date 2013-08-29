using Umbraco.Tests.CodeFirst.Attributes;
using umbraco.editorControls.textfield;
using umbraco.editorControls.tinyMCE3;

namespace Umbraco.Tests.CodeFirst.TestModels
{
    public class ModelWithNewDataType : ContentTypeBase
    {
        [PropertyType(typeof(TextFieldDataType), PropertyGroup = "Content")]
        public string Title { get; set; }

        [PropertyType(typeof(tinyMCE3dataType), PropertyGroup = "Content")]
        public string BodyContent { get; set; } 
    }
}