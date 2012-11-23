using System;
using Umbraco.Tests.CodeFirst.Attributes;
using umbraco.editorControls.textfield;

namespace Umbraco.Tests.CodeFirst.TestModels
{
    [ContentType("modelPage", AllowedChildContentTypes = new[] { typeof(ContentPage) }, AllowedTemplates = new[]{"umbMaster"})]
    public class DecoratedModelPage
    {
        [PropertyType(typeof(TextFieldDataType), PropertyGroup = "Content")]
        public string Author { get; set; }

        [PropertyType(typeof(TextFieldDataType), PropertyGroup = "Content")]
        public string Title { get; set; }

        [Richtext(PropertyGroup = "Content")]
        public string BodyContent { get; set; }

        public DateTime PublishDate { get; set; } 
    }
}