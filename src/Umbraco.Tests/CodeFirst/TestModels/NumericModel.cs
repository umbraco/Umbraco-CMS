using Umbraco.Tests.CodeFirst.Attributes;

namespace Umbraco.Tests.CodeFirst.TestModels
{
    public class NumericModel : ContentTypeBase
    {
        [Numeric("Number DataType", PreValue = "5", PropertyGroup = "Content")]
        public int Number { get; set; } 
    }
}