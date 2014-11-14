using Umbraco.Tests.CodeFirst.Attributes;
using Umbraco.Web.PropertyEditors;

namespace Umbraco.Tests.CodeFirst.TestModels
{
    public class SimpleContentPage : ContentTypeBase
    {
        [PropertyType(typeof(TextboxPropertyEditor), PropertyGroup = "Content")]
        public string Title { get; set; }

        [Richtext(PropertyGroup = "Content")]
        public string BodyContent { get; set; }
    }
}