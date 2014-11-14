using Umbraco.Tests.CodeFirst.Attributes;
using Umbraco.Web.PropertyEditors;

namespace Umbraco.Tests.CodeFirst.TestModels.Composition
{
    /// <summary>
    /// Deriving class is parent, interfaces are compositions
    /// </summary>
    public class News : Base, IMetaSeo, IMeta
    {
        [PropertyType(typeof(TextboxPropertyEditor), PropertyGroup = "Content")]
        public string Author { get; set; }
    }
}