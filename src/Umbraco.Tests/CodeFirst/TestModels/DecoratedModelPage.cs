using System;
using Umbraco.Tests.CodeFirst.Attributes;
using umbraco.editorControls.textfield;

namespace Umbraco.Tests.CodeFirst.TestModels
{
    [ContentType("modelPage", 
        AllowedChildContentTypes = new[] { typeof(ContentPage) }, 
        AllowedTemplates = new[]{"umbMaster"})]
    public class DecoratedModelPage : ContentTypeBase
    {
        [PropertyType(typeof(TextFieldDataType), PropertyGroup = "Content")]
        public string Author { get; set; }

        [PropertyType(typeof(TextFieldDataType), PropertyGroup = "Content")]
        public string Title { get; set; }

        [Richtext(PropertyGroup = "Content")]
        [Description("Richtext field to enter the main content of the page")]
        public string BodyContent { get; set; }

        [Alias("publishedDate", Name = "Publish Date")]
        public DateTime PublishDate { get; set; }
    }
}