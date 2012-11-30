using Umbraco.Tests.CodeFirst.Attributes;
using umbraco.editorControls.textfield;

namespace Umbraco.Tests.CodeFirst.TestModels
{
    public class TextPage : ContentTypeBase
    {
        [PropertyType(typeof(TextFieldDataType), PropertyGroup = "Content")]
        public string Author { get; set; }

        [PropertyType(typeof(TextFieldDataType), PropertyGroup = "Content")]
        public string Title { get; set; }
    }
}