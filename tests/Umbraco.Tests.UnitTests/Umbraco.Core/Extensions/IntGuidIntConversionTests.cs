using NUnit.Framework;
using Umbraco.Cms.Core.Extensions;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Extensions;

[TestFixture]
public class IntGuidIntConversionTests
{
    [TestCase(int.MinValue)]
    [TestCase(int.MaxValue)]
    [TestCase(int.MinValue / 2)]
    [TestCase(int.MinValue / 5)]
    [TestCase(int.MinValue / 707)]
    [TestCase(int.MinValue / 1313)]
    [TestCase(int.MinValue / 17017)]
    [TestCase(int.MaxValue / 2)]
    [TestCase(int.MaxValue / 5)]
    [TestCase(int.MaxValue / 707)]
    [TestCase(int.MaxValue / 1313)]
    [TestCase(int.MaxValue / 17017)]
    public void IntoToGuidToInt_NoChange(int startValue)
    {
        var intermediateValue = startValue.ToGuid();
        var endValue = intermediateValue.ToInt();

        Assert.AreEqual(startValue, endValue);
    }
}
