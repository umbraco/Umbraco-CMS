using Umbraco.Tests.CodeFirst.Attributes;
using umbraco.editorControls.textfield;

namespace Umbraco.Tests.CodeFirst.TestModels
{
    public class SimpleContentPage : ContentTypeBase
    {
        [PropertyType(typeof(TextFieldDataType), PropertyGroup = "Content")]
        public string Title { get; set; }

        [Richtext(PropertyGroup = "Content")]
        public string BodyContent { get; set; }
    }
}