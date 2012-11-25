using Umbraco.Tests.CodeFirst.Attributes;

namespace Umbraco.Tests.CodeFirst.TestModels.Composition
{
    public class Meta : IMeta
    {
         
    }

    [Alias("meta", Name = "Meta")]
    public interface IMeta
    {}
}