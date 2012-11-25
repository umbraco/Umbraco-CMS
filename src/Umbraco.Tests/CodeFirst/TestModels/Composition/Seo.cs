using Umbraco.Tests.CodeFirst.Attributes;

namespace Umbraco.Tests.CodeFirst.TestModels.Composition
{
    public class Seo : ISeo
    {
         
    }

    [Alias("seo", Name = "Seo")]
    public interface ISeo : IBase
    {}
}