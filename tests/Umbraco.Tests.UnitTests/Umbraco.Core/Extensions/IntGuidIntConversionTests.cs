using NUnit.Framework;
using Umbraco.Cms.Core.Extensions;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Extensions;

[TestFixture]
public class IntGuidIntConversionTests
{
    [TestCase(0)]
    [TestCase(1)]
    [TestCase(-1)]
    [TestCase(int.MinValue)]
    [TestCase(int.MaxValue)]
    [TestCase(int.MinValue / 2)]
    [TestCase(int.MaxValue / 2)]
    [TestCase(-555555555)]
    [TestCase(555555555)]
    public void Can_Round_Trip_Int_Through_Guid(int startValue)
    {
        Guid guid = startValue.ToGuid();
        int endValue = guid.ToInt();

        Assert.AreEqual(startValue, endValue);
    }

    [TestCase(0)]
    [TestCase(1)]
    [TestCase(-1)]
    [TestCase(int.MinValue)]
    [TestCase(int.MaxValue)]
    public void Can_Detect_Guid_Created_From_Int_As_Fake(int value)
    {
        Guid guid = value.ToGuid();

        Assert.IsTrue(guid.IsFakeGuid());
    }

    // A "fake" guid must have all bytes zero except the first 4. Each of these has a stray
    // non-zero byte outside that range, so none should be reported as fake.
    [TestCase("0d93047e-558d-4311-8a9d-b89e6fca0337")] // fully random guid
    [TestCase("00000014-0001-0000-0000-000000000000")] // non-zero byte immediately after the int region
    [TestCase("00000014-0000-0001-0000-000000000000")]
    [TestCase("00000014-0000-0000-0001-000000000000")]
    [TestCase("00000014-0000-0000-0000-000000000001")] // non-zero final byte
    public void Cannot_Detect_Real_Guid_As_Fake(string guidValue)
    {
        Guid guid = Guid.Parse(guidValue);

        Assert.IsFalse(guid.IsFakeGuid());
    }
}
