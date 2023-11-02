using NUnit.Framework;

namespace Umbraco.Cms.Tests.Common.Attributes;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
public class NonCritical : CategoryAttribute
{
    public NonCritical()
        : base(TestConstants.Categories.NonCritical)
    {
    }
}
