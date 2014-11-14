using Umbraco.Tests.CodeFirst.Attributes;
using Umbraco.Web.PropertyEditors;

namespace Umbraco.Tests.CodeFirst.TestModels
{
    public class TextPage : ContentTypeBase
    {
        [PropertyType(typeof(TextboxPropertyEditor), PropertyGroup = "Content")]
        public string Author { get; set; }

        [PropertyType(typeof(TextboxPropertyEditor), PropertyGroup = "Content")]
        public string Title { get; set; }
    }
}