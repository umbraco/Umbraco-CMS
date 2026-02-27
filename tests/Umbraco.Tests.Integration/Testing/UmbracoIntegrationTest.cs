using NUnit.Framework;

namespace Umbraco.Cms.Tests.Integration.Testing;

/// <summary>
///     Abstract class for integration tests
/// </summary>
/// <remarks>
///     This will use a Host Builder to boot and install Umbraco ready for use
/// </remarks>
public abstract class UmbracoIntegrationTest : UmbracoIntegrationFixtureBase
{
    [SetUp]
    public void Setup()
    {
        BuildAndStartHost();
    }

    [TearDown]
    public async Task TearDownAsync()
    {
        await StopHost();
    }

    [TearDown]
    public void TearDown()
    {
        ExecuteTearDownQueue();
    }

    [SetUp]
    public virtual void SetUp_Logging() => TestContext.Out.Write($"Start test {TestCount++}: {TestContext.CurrentContext.Test.Name}");

    [TearDown]
    public void TearDown_Logging() => TestContext.Out.Write($"  {TestContext.CurrentContext.Result.Outcome.Status}");
}
