using NUnit.Framework;
using Umbraco.Cms.Infrastructure.Persistence;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Persistence;

[TestFixture]
public class DistributedLockTimeoutExtensionsTests
{
    [Test]
    [TestCase(1000, ExpectedResult = 6)]
    [TestCase(5000, ExpectedResult = 10)]
    [TestCase(60000, ExpectedResult = 65)]
    public int Can_Allow_A_Margin_Over_The_Lock_Timeout(int milliseconds)
        => TimeSpan.FromMilliseconds(milliseconds).ToLockCommandTimeoutSeconds();

    [Test]
    [TestCase(1, ExpectedResult = 6)]
    [TestCase(500, ExpectedResult = 6)]
    [TestCase(1001, ExpectedResult = 7)]
    [TestCase(1500, ExpectedResult = 7)]
    public int Can_Round_Partial_Second_Up(int milliseconds)
        => TimeSpan.FromMilliseconds(milliseconds).ToLockCommandTimeoutSeconds();

    [Test]
    public void Cannot_Derive_A_Command_Timeout_Shorter_Than_The_Lock_Timeout()
    {
        // The point of the margin is that the mechanism gets to report its own lock timeout before the
        // client abandons the command, so the derived value must always outlast the wait.
        TimeSpan lockTimeout = TimeSpan.FromMilliseconds(2500);

        Assert.Greater(lockTimeout.ToLockCommandTimeoutSeconds(), lockTimeout.TotalSeconds);
    }

    [Test]
    public void Cannot_Derive_No_Limit_At_All()
    {
        // Zero seconds means no limit to supported providers, which would leave the command unbounded.
        Assert.AreNotEqual(0, TimeSpan.Zero.ToLockCommandTimeoutSeconds());
    }
}
