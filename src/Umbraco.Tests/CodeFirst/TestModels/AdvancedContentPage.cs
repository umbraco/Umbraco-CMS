using System;
using Umbraco.Tests.CodeFirst.Attributes;
using umbraco.editorControls.textfield;

namespace Umbraco.Tests.CodeFirst.TestModels
{
    public class AdvancedContentPage : SimpleContentPage
    {
        [PropertyType(typeof(TextFieldDataType), PropertyGroup = "Content")]
        public string Author { get; set; }

        public DateTime PublishDate { get; set; }
    }
}