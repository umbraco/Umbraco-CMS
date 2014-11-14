using Umbraco.Tests.CodeFirst.Attributes;
using Umbraco.Web.PropertyEditors;

namespace Umbraco.Tests.CodeFirst.TestModels.Composition
{
    public class MetaSeo : ContentTypeBase, IMetaSeo
    {
        [PropertyType(typeof(MultipleTextStringPropertyEditor))]
        public string FriendlySeoStuff { get; set; }
    }

    [Mixin(typeof(MetaSeo))]
    public interface IMetaSeo
    {}
}