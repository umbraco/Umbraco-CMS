using Umbraco.Tests.CodeFirst.Attributes;
using umbraco.editorControls.textfield;

namespace Umbraco.Tests.CodeFirst.TestModels.Composition
{
    /// <summary>
    /// Deriving class is parent, interfaces are compositions
    /// </summary>
    public class News : Base, IMetaSeo, IMeta
    {
        [PropertyType(typeof(TextFieldDataType), PropertyGroup = "Content")]
        public string Author { get; set; }
    }
}