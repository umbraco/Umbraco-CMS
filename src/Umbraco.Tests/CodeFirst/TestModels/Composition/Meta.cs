using Umbraco.Tests.CodeFirst.Attributes;
using umbraco.editorControls.textfield;
using umbraco.editorControls.textfieldmultiple;

namespace Umbraco.Tests.CodeFirst.TestModels.Composition
{
    public class Meta : ContentTypeBase, IMeta
    {
        [PropertyType(typeof(TextFieldDataType), PropertyGroup = "Content")]
        public string MetaKeywords { get; set; }

        [PropertyType(typeof(textfieldMultipleDataType))]
        public string MetaDescription { get; set; }
    }

    [Mixin(typeof(Meta))]
    public interface IMeta
    {}
}