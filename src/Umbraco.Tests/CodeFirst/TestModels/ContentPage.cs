using Umbraco.Tests.CodeFirst.Attributes;

namespace Umbraco.Tests.CodeFirst.TestModels
{
    public class ContentPage : ContentTypeBase
    {
        [Richtext(PropertyGroup = "Content")]
        public string BodyContent { get; set; }
    }
}