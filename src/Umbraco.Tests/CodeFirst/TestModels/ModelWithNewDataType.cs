using Umbraco.Tests.CodeFirst.Attributes;
using Umbraco.Web.PropertyEditors;

namespace Umbraco.Tests.CodeFirst.TestModels
{
    public class ModelWithNewDataType : ContentTypeBase
    {
        [PropertyType(typeof(TextboxPropertyEditor), PropertyGroup = "Content")]
        public string Title { get; set; }

        [PropertyType(typeof(RichTextPropertyEditor), PropertyGroup = "Content")]
        public string BodyContent { get; set; } 
    }
}