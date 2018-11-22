using Umbraco.Tests.CodeFirst.Attributes;
using umbraco.editorControls.textfieldmultiple;

namespace Umbraco.Tests.CodeFirst.TestModels.Composition
{
    public class MetaSeo : ContentTypeBase, IMetaSeo
    {
        [PropertyType(typeof(textfieldMultipleDataType))]
        public string FriendlySeoStuff { get; set; }
    }

    [Mixin(typeof(MetaSeo))]
    public interface IMetaSeo
    {}
}