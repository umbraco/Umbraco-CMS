using NUnit.Framework;

namespace Umbraco.Tests.UnitTests;

[TestFixture]
public class SonarCloudPipelineTests
{
    [Test]
    public void FailingTest_ToVerifyCoverageStillUploads_RemoveMe()
        => Assert.Fail("Intentional failure to verify pipeline continue-on-error behavior — remove this test.");
}
