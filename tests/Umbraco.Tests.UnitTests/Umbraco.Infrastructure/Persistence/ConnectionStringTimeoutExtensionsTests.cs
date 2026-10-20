using NUnit.Framework;
using Umbraco.Cms.Infrastructure.Persistence;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Persistence;

[TestFixture]
public class ConnectionStringTimeoutExtensionsTests
{
    [Test]
    [TestCase(0, ExpectedResult = 0)]
    [TestCase(1000, ExpectedResult = 1)]
    [TestCase(30000, ExpectedResult = 30)]
    [TestCase(300000, ExpectedResult = 300)]
    public int Can_Convert_Whole_Number_Of_Seconds_Unchanged(int milliseconds)
        => TimeSpan.FromMilliseconds(milliseconds).ToConnectionStringTimeoutSeconds();

    [Test]
    [TestCase(1, ExpectedResult = 1)]
    [TestCase(500, ExpectedResult = 1)]
    [TestCase(999, ExpectedResult = 1)]
    [TestCase(1001, ExpectedResult = 2)]
    [TestCase(1500, ExpectedResult = 2)]
    public int Can_Round_Partial_Second_Up(int milliseconds)
        => TimeSpan.FromMilliseconds(milliseconds).ToConnectionStringTimeoutSeconds();

    [Test]
    public void Cannot_Convert_Configured_Timeout_To_No_Limit()
    {
        // Zero seconds means no limit to supported providers, so the smallest configurable timeout must not
        // truncate into it.
        Assert.AreNotEqual(0, TimeSpan.FromTicks(1).ToConnectionStringTimeoutSeconds());
    }

    [Test]
    public void Can_Preserve_Explicitly_Configured_No_Limit()
    {
        Assert.AreEqual(0, TimeSpan.Zero.ToConnectionStringTimeoutSeconds());
    }
}
