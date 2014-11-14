using Umbraco.Tests.CodeFirst.Attributes;
using Umbraco.Web.PropertyEditors;

namespace Umbraco.Tests.CodeFirst.TestModels.Composition
{
    public class Meta : ContentTypeBase, IMeta
    {
        [PropertyType(typeof(TextboxPropertyEditor), PropertyGroup = "Content")]
        public string MetaKeywords { get; set; }

        [PropertyType(typeof(MultipleTextStringPropertyEditor))]
        public string MetaDescription { get; set; }
    }

    [Mixin(typeof(Meta))]
    public interface IMeta
    {}
}