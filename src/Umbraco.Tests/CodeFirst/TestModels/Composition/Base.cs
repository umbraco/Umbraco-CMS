using Umbraco.Tests.CodeFirst.Attributes;

namespace Umbraco.Tests.CodeFirst.TestModels.Composition
{
    public class Base : ContentTypeBase, IBase
    {
        [Richtext(PropertyGroup = "Content")]
        public string BodyContent { get; set; }
    }

    [Mixin(typeof(Base))]
    public interface IBase
    {
        
    }
}