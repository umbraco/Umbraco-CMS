using NUnit.Framework;

namespace Umbraco.Tests.Search.Examine.Integration;

[SetUpFixture]
public class CustomGlobalSetupTeardown
{
    private static GlobalSetupTeardown _setupTearDown;

    [OneTimeSetUp]
    public void SetUp()
    {
        _setupTearDown = new GlobalSetupTeardown();
        _setupTearDown.SetUp();
    }

    [OneTimeTearDown]
    public void TearDown()
        => _setupTearDown.TearDown();
}
